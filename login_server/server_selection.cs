using EQGodot2.network_manager.login_server;
using Godot;
using System;
using System.Collections.Generic;

public partial class server_selection : Control
{
    private int SelectedServer;
    private EQServerDescription[] Servers;
    private Dictionary<int, int> ListToPacketMap;

    [Signal]
    public delegate void ServerJoinStartEventHandler(EQServerDescription server);

    public void LoadServers(EQServerDescription[] servers)
    {
        ListToPacketMap = [];
        var list = GetNode<ItemList>("%ServerList");
        list.ItemSelected += OnServerSelected;
        Servers = servers;
        for (int i = 0; i < Servers.Length; i++)
        {
            var server = Servers[i];
            var index = list.AddItem($"{server.Address} - {server.LongName} - {server.Players}");
            ListToPacketMap[index] = i;
        }
    }

    private void OnServerSelected(long index)
    {
        SelectedServer = ListToPacketMap[(int)index];
    }

    private void OnServerAccepted()
    {
        EmitSignal(SignalName.ServerJoinStart, Servers[SelectedServer]);
    }
}
