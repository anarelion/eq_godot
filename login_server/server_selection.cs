using EQGodot2.network_manager.login_server;
using Godot;
using System;

public partial class server_selection : Control
{
    public void LoadServers(SCGetServerListReply packet)
    {
        var list = GetNode<ItemList>("%ServerList");
        list.ItemSelected += OnServerSelected;
        for (int i = 0; i < packet.LongName.Length; i++)
        {
            list.AddItem($"{packet.Address[i]} - {packet.LongName[i]} - {packet.Players[i]}");
        }
    }

    private void OnServerSelected(long index)
    {
        throw new NotImplementedException();
    }

    private void OnServerAccepted()
    {
        GD.Print("Go!");
    }
}
