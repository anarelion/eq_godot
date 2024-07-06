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
        private OpcodeManager OpcodeManager;
        private uint LsId;
        private byte[] Key;

        public WorldSession(uint lsid, byte[] key, EQServerDescription server)
        {
            LsId = lsid;
            Key = key;
            OpcodeManager = new OpcodeManager();
            OpcodeManager.Register<CSWorldAuth>(0x7a09);
            OpcodeManager.Register<SCGuildList>(0x507a);
            OpcodeManager.Register<SCLogServer>(0x7ceb);
            Network = new NetworkSession();
            Network.SessionEstablished += OnConnectionEstablished;
            Network.ConnectToHost(server.Address, 9000);
            Name = "WorldSession";
        }

        public override void _Process(double delta)
        {
            Network.Process();
        }

        private void OnConnectionEstablished()
        {
            Network.PacketReceived += OnPacketReceived;
            Network.SendAppPacket(new CSWorldAuth(LsId, Key), OpcodeManager);
        }

        private void OnPacketReceived(byte[] packet)
        {
            var reader = new PacketReader(packet);
            var decoded = OpcodeManager.Decode(reader);
            _ = decoded switch
            {
                SCGuildList p => ProcessPacket(p),
                SCLogServer p => ProcessPacket(p),
                _ => throw new NotImplementedException(),
            };
        }

        private bool ProcessPacket(SCGuildList packet)
        {
            GD.Print($"Guild List {packet.GuildNames.Length}");
            return true;
        }

        private bool ProcessPacket(SCLogServer packet)
        {
            GD.Print("LogServer");
            return true;
        }
    }
}