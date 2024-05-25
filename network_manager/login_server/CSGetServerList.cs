
using System;
using EQGodot2.network_manager.network_session;

namespace EQGodot2.network_manager.login_server
{
    public class CSGetServerList : AppPacket
    {
        public override ushort Opcode()
        {
            return 0x0400;
        }

        public override void Write()
        {
            Writer.WriteInt(0x04);
        }

        public override void Read()
        {
            throw new NotImplementedException();
        }

    }
}
