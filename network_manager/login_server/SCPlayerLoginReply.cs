
using System;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;
using Godot;

namespace EQGodot2.network_manager.login_server
{
    public class SCPlayerLoginReply(PacketReader reader) : AppPacket(reader)
    {
        public uint LSID;
        public byte[] KeyComponents;
        public uint FailedAttempts;
        public byte Message;
        public byte EQLSStr;

        public override ushort Opcode()
        {
            return 0x1800;
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }

        public override void Read()
        {
            Reader.ReadUIntBE();
            Reader.ReadUIntBE();
            Reader.ReadUShortBE();
            var contents = Reader.ReadBytes(Reader.Remaining());
            var empty = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var des = new DESCrypto(empty, empty);
            var subReader = new PacketReader(des.Decrypt(contents));
            Message = subReader.ReadByte();
            EQLSStr = subReader.ReadByte();
            subReader.ReadUShortBE();
            subReader.ReadUIntLE();
            LSID = subReader.ReadUIntLE();
            KeyComponents = subReader.ReadBytes(10);
            FailedAttempts = subReader.ReadUIntLE();
        }

    }
}
