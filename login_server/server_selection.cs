using EQGodot2.network_manager.login_server;
using Godot;
using System;
using System.Collections.Generic;

public partial class server_selection : Control
{
    private int SelectedServer;
    private SCGetServerListReply Packet;
    private Dictionary<int, int> ListToPacketMap;

    public void LoadServers(SCGetServerListReply packet)
    {
        Packet = packet;
        ListToPacketMap = [];
        var list = GetNode<ItemList>("%ServerList");
        list.ItemSelected += OnServerSelected;
        for (int i = 0; i < packet.LongName.Length; i++)
        {
            var index = list.AddItem($"{packet.Address[i]} - {packet.LongName[i]} - {packet.Players[i]}");
            ListToPacketMap[index] = i;
        }
    }

    private void OnServerSelected(long index)
    {
        SelectedServer = ListToPacketMap[(int)index];
        GD.Print($"Selected server {Packet.Address[SelectedServer]}");
    }

    private void OnServerAccepted()
    {
        GetNode<LoginSession>("/root/LoginSession").JoinServer(SelectedServer, Packet);
    }
}
