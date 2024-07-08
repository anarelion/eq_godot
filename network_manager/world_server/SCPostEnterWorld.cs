
using System;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;
using Godot;

namespace EQGodot2.network_manager.world_server
{
    public class SCPostEnterWorld(PacketReader reader) : AppPacket(reader)
    {
        public override void Write()
        {
            throw new NotImplementedException();
        }

        public override void Read()
        {
            GD.Print($"SCPostEnterWorld {Reader.ReadBytes(Reader.Remaining()).HexEncode()}");
        }
    }
}
