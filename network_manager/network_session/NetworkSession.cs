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
            writer.WriteShort(0x01);
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

        private void ProcessPacket(PacketReader reader, bool processCrc)
        {
            var opcode = reader.ReadUShort();
            switch (opcode)
            {
                case 0x02: processCrc = false; break;
                case 0x06: processCrc = false; break;
            }

            if (processCrc)
            {
                reader.Reset();
                int size = (int)reader.Remaining();
                var contents = reader.ReadBytes(size - 2);
                ushort crc = reader.ReadUShort();
                var calculated = CRC.CalculateCRC16(contents, EncodeKey);
                if (calculated != crc)
                {
                    GD.PrintErr(" CRC Invalid");
                    return;
                }
                reader = new PacketReader(contents);
                opcode = reader.ReadUShort();
            }

            switch (opcode)
            {
                case 0x02: ConnectionEstablished(reader); processCrc = false; break;
                case 0x03: ProcessCombined(reader); break;
                case 0x06: ProcessKeepAlive(reader); processCrc = false; break;
                case 0x09: ProcessAppPacket(reader); break;
                case 0x15: ProcessAck(reader); break;
                default: GD.PrintErr($"Opcode {opcode:X04} not implemented"); throw new NotImplementedException();
            }

        }

        private PacketReader GetPacketReader()
        {
            var packet = GetPacket();
            GD.Print(" <== ", packet.HexEncode());
            return new PacketReader(packet);
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
            writer.WriteShort(0x09);
            writer.WriteByte((byte)(SequenceOut >> 8));
            writer.WriteByte((byte)SequenceOut);
            var data = packet.ToBytes();
            SentPackets[SequenceOut] = data;
            GD.Print(" APP ", data.HexEncode());
            writer.WriteBytes(data);
            AppendCRC(writer);
            SendPacket(writer);
            SequenceOut++;
        }

        private void ConnectionEstablished(PacketReader reader)
        {
            var connectCode = reader.ReadUInt();
            if (connectCode != ConnectCode)
            {
                GD.PrintErr($"Connection code does not match");
                return;
            }
            var encode = reader.ReadUInt();
            EncodeKey = [
                        (byte) (encode >> 24),
                        (byte) (encode >> 16),
                        (byte) (encode >> 8),
                        (byte) encode
                    ];
            CRCBytes = reader.ReadByte();
            FilterMode = reader.ReadByte();
            EncodePass2 = reader.ReadByte();
            MaxPacketSize = reader.ReadUInt();
            EmitSignal(SignalName.SessionEstablished);
        }

        private void AppendCRC(PacketWriter writer)
        {
            if (CRCBytes == 2)
            {
                var crc = CRC.CalculateCRC16(writer.ToBytes(), EncodeKey);
                writer.WriteUShort(crc);
            }
        }

        private void ProcessKeepAlive(PacketReader reader)
        {
            var writer = new PacketWriter();
            writer.WriteUShort(0x6);
            writer.WriteUShort(reader.ReadUShort());
            SendPacket(writer);
        }

        private void ProcessCombined(PacketReader reader)
        {
            while (reader.Remaining() <= 2)
            {
                var size = reader.ReadByte();
                var packet = reader.ReadBytes(size);
                var subreader = new PacketReader(packet);
                ProcessPacket(subreader, false);
            }
        }

        private void ProcessAck(PacketReader reader)
        {
            var sequence = reader.ReadUShort();
            if (SentPackets[sequence] != null)
            {
                SentPackets[sequence] = null;
            }
        }

        private void ProcessAppPacket(PacketReader reader)
        {
            var sequence = reader.ReadUShort();
            var packet = reader.ReadBytes((int)reader.Remaining());
            GD.Print($"{sequence:X04} {SequenceIn:X04} - {reader.Stream.Position}/{reader.Stream.Length} ");
            if (sequence == SequenceIn)
            {
                var writer = new PacketWriter();
                writer.WriteUShort(0x15);
                writer.WriteUShort(sequence);
                AppendCRC(writer);
                SendPacket(writer);
                SequenceIn++;
                EmitSignal(SignalName.PacketReceived, packet);
            }
        }
    }

}