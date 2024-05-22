using EQGodot2.network_manager.packets;
using Godot;
using System;
using System.Formats.Asn1;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Xml.XPath;

namespace EQGodot2.network_manager.network_session
{

    public partial class NetworkSession : PacketPeerUdp
    {
        private uint ConnectCode;
        private byte[] EncodeKey;
        private byte CRCBytes;
        private byte FilterMode;
        private byte EncodePass2;
        private uint MaxPacketSize = 512;
        private uint ProtocolVersion = 2;
        private ushort SequenceOut = 0;
        private ushort SequenceIn = 0;
        private ushort LastAckReceived = 65535;
        private ushort LastAckSent = 0;
        private byte[][] SentPackets = new byte[0x10000][];
        private byte[][] FuturePackets = new byte[0x10000][];

        [Signal]
        public delegate void SessionEstablishedEventHandler();

        [Signal]
        public delegate void PacketReceivedEventHandler(byte[] packet);

        public new Error ConnectToHost(string host, int port)
        {
            ConnectCode = (uint)GlobalVariables.Rand.Next();
            var result = base.ConnectToHost(host, port);
            GD.Print($"Connected to {host}:{port} => {result}");
            var writer = new PacketWriter();
            writer.WriteShort(0x100);
            writer.WriteUInt(ProtocolVersion);
            writer.WriteUInt(ConnectCode);
            writer.WriteUInt(MaxPacketSize);
            SendPacket(writer);
            return result;
        }

        public void Process()
        {
            if (GetAvailablePacketCount() <= 0)
            {
                return;
            }
            var reader = GetPacketReader();
            ProcessPacket(reader, true);
        }

        private void ProcessPacket(BinaryReader reader, bool processCrc)
        {
            var opcode = reader.ReadUInt16();
            switch (opcode)
            {
                case 0x0200: processCrc = false; break;
                case 0x0600: processCrc = false; break;
            }

            if (processCrc)
            {
                reader.BaseStream.Position = 0;
                int size = (int)reader.BaseStream.Length;
                var contents = reader.ReadBytes(size - 2);
                ushort crc = (ushort)((ushort)(reader.ReadByte() << 8) | reader.ReadByte());
                var calculated = CRC.CalculateCRC16(contents, EncodeKey);
                if (calculated != crc)
                {
                    GD.PrintErr(" CRC Invalid");
                    return;
                }
                var stream = new MemoryStream(contents);
                reader = new BinaryReader(stream);
                opcode = reader.ReadUInt16();
            }

            switch (opcode)
            {
                case 0x0200: ConnectionEstablished(reader); processCrc = false; break;
                case 0x0300: ProcessCombined(reader); break;
                case 0x0600: ProcessKeepAlive(reader); processCrc = false; break;
                case 0x0900: ProcessAppPacket(reader); break;
                case 0x1500: ProcessAck(reader); break;
                default: GD.PrintErr($"Opcode {opcode:X04} not implemented"); throw new NotImplementedException();
            }

        }

        private BinaryReader GetPacketReader()
        {
            var packet = GetPacket();
            GD.Print(" <== ", packet.HexEncode());
            var stream = new MemoryStream(packet);
            return new BinaryReader(stream);
        }

        private void SendPacket(PacketWriter writer)
        {
            var send = writer.ToBytes();
            GD.Print(" ==> ", send.HexEncode());
            PutPacket(send);
        }

        public void SendAppPacket(AppPacket packet)
        {
            var writer = new PacketWriter();
            writer.WriteShort(0x900);
            writer.WriteUShort(SequenceOut);
            var data = packet.ToBytes();
            SentPackets[SequenceOut] = data;
            GD.Print(" APP ", data.HexEncode());
            writer.WriteBytes(data);
            AppendCRC(writer);
            SendPacket(writer);
            SequenceOut++;
        }

        private void ConnectionEstablished(BinaryReader reader)
        {
            var connectCode = reader.ReadUInt32();
            if (connectCode != ConnectCode)
            {
                GD.PrintErr($"Connection code does not match");
                return;
            }
            var encode = reader.ReadUInt32();
            EncodeKey = [
                        (byte) (encode >> 24),
                        (byte) (encode >> 16),
                        (byte) (encode >> 8),
                        (byte) encode
                    ];
            CRCBytes = reader.ReadByte();
            FilterMode = reader.ReadByte();
            EncodePass2 = reader.ReadByte();
            MaxPacketSize = reader.ReadUInt32();
            EmitSignal(SignalName.SessionEstablished);
        }

        private void AppendCRC(PacketWriter writer)
        {
            if (CRCBytes == 2)
            {
                var crc = CRC.CalculateCRC16(writer.ToBytes(), EncodeKey);
                writer.WriteByte((byte)(crc >> 8));
                writer.WriteByte((byte)crc);
            }
        }

        private void ProcessKeepAlive(BinaryReader reader)
        {
            var writer = new PacketWriter();
            writer.WriteUShort(0x600);
            writer.WriteUShort(reader.ReadUInt16());
            SendPacket(writer);
        }

        private void ProcessCombined(BinaryReader reader)
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length - 2)
            {
                var size = reader.ReadByte();
                var packet = reader.ReadBytes(size);
                var stream = new MemoryStream(packet);
                var subreader = new BinaryReader(stream);
                ProcessPacket(subreader, false);
            }
        }

        private void ProcessAck(BinaryReader reader)
        {
            var sequence = reader.ReadUInt16();
            if (SentPackets[sequence] != null)
            {
                SentPackets[sequence] = null;
            }
        }

        private void ProcessAppPacket(BinaryReader reader)
        {
            var sequence = reader.ReadUInt16();
            var packet = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            GD.Print($"{sequence:X04} {SequenceIn:X04} - {reader.BaseStream.Position}/{reader.BaseStream.Length} ");
            if (sequence == SequenceIn)
            {
                var writer = new PacketWriter();
                writer.WriteUShort(0x1500);
                writer.WriteUShort(sequence);
                AppendCRC(writer);
                SendPacket(writer);

                EmitSignal(SignalName.PacketReceived, packet);
            }
        }
    }

}