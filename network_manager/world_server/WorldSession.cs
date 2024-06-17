using System;
using System.IO;
using EQGodot2.network_manager.login_server;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;
using Godot;

namespace EQGodot2.network_manager.world_server
{
    public partial class WorldSession : Node
    {
        NetworkSession Network;
        private uint LsId;
        private byte[] Key;

        public WorldSession(uint lsid, byte[] key, EQServerDescription server)
        {
            LsId = lsid;
            Key = key;
            Network = new NetworkSession();
            Network.SessionEstablished += OnConnectionEstablished;
            Network.ConnectToHost(server.Address, 9000);
            Name = "WorldSession";
        }

        private void OnConnectionEstablished()
        {
            throw new NotImplementedException();
        }
    }
}