
using System;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;

namespace EQGodot2.network_manager.login_server
{
    public class SCHandshakeReply : AppPacket
    {
        public string Message;

        public SCHandshakeReply(PacketReader reader) : base(reader)
        {
        }

        public override ushort Opcode()
        {
            return 0x1700;
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }

        public override void Read()
        {
            Reader.ReadUShort();
            Reader.ReadUShort();
            Reader.ReadUShort();
            Reader.ReadUInt();
            Reader.ReadUInt();
            Reader.ReadByte();
            Message = Reader.ReadString();
        }

    }
}
