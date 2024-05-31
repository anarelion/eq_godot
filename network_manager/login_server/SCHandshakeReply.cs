
using System;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;

namespace EQGodot2.network_manager.login_server
{
    public class SCHandshakeReply(PacketReader reader) : AppPacket(reader)
    {
        public string Message;

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
            Reader.ReadUShortBE();
            Reader.ReadUShortBE();
            Reader.ReadUShortBE();
            Reader.ReadUIntLE();
            Reader.ReadUIntLE();
            Reader.ReadByte();
            Message = Reader.ReadString();
        }

    }
}
