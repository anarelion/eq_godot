using System;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;

namespace EQGodot2.network_manager.world_server;

public class SCGuildList(PacketReader reader) : AppPacket(reader)
{
    private uint GuildCount;
    public string[] GuildNames;

    public override void Write()
    {
        throw new NotImplementedException();
    }

    public override void Read()
    {
        GuildCount = Reader.ReadUIntLE();
        GuildNames = new string[GuildCount];
        for (var i = 0; i < GuildCount; i++) GuildNames[i] = Reader.ReadString();
    }
}