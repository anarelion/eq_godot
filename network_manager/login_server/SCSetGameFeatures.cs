
using System;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;

namespace EQGodot2.network_manager.login_server
{
    public class SCSetGameFeatures(PacketReader reader) : AppPacket(reader)
    {
        public override ushort Opcode()
        {
            return 0x3100;
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }

        public override void Read()
        {
            // TODO: not sure if we care about this packet
        }

    }
}
