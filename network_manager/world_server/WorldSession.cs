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
            OpcodeManager.Register<SCApproveWorld>(0x7499);
            OpcodeManager.Register<SCEnterWorld>(0x578f);
            OpcodeManager.Register<SCPostEnterWorld>(0x6259);

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
                SCApproveWorld p => ProcessPacket(p),
                SCEnterWorld p => ProcessPacket(p),
                SCPostEnterWorld p => ProcessPacket(p),

                _ => throw new NotImplementedException(),
            };
        }

        private bool ProcessPacket(SCGuildList packet)
        {
            GD.Print($"SCGuildList count={packet.GuildNames.Length}");
            return true;
        }

        private bool ProcessPacket(SCLogServer packet)
        {
            GD.Print("SCLogServer");
            return true;
        }

        private bool ProcessPacket(SCApproveWorld packet)
        {
            GD.Print("SCApproveWorld");
            return true;
        }

        private bool ProcessPacket(SCEnterWorld packet)
        {
            GD.Print("SCEnterWorld");
            return true;
        }

        private bool ProcessPacket(SCPostEnterWorld packet)
        {
            GD.Print("SCPostEnterWorld");
            return true;
        }
    }
}