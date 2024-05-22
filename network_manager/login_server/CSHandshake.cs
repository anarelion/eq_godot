
using System;
using EQGodot2.network_manager.network_session;

namespace EQGodot2.network_manager.login_server
{
    public class CSHandshake : AppPacket
    {
        public override ushort Opcode()
        {
            return 0x0001;
        }

        public override void Write()
        {
            Writer.WriteInt(0);
            Writer.WriteInt(0);
            Writer.WriteShort(0);
            Writer.WriteShort(0xB00);
        }

        public override void Read()
        {
            throw new NotImplementedException();
        }

    }
}
