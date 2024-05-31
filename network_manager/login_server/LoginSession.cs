using System;
using System.IO;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;
using Godot;

namespace EQGodot2.network_manager.login_server
{
    public partial class LoginSession : Node
    {
        NetworkSession Network;
        string Username;
        string Password;

        [Signal]
        public delegate void MessageUpdateEventHandler(string message);

        public LoginSession(string username, string password)
        {
            Username = username;
            Password = password;
            Network = new NetworkSession();
            Network.SessionEstablished += OnConnectionEstablished;
            Network.ConnectToHost("127.0.0.1", 5999);
            Name = "LoginSession";
        }

        public override void _Process(double delta)
        {
            Network.Process();
        }

        private void OnConnectionEstablished()
        {
            EmitSignal(SignalName.MessageUpdate, "Established connection, logging in");
            Network.PacketReceived += OnPacketReceived;
            Network.SendAppPacket(new CSHandshake());
        }

        private void OnPacketReceived(byte[] packet)
        {
            GD.Print($" LOG IN  {packet.HexEncode()}");
            var reader = new PacketReader(packet);
            var opcode = reader.ReadUShortLE();
            switch (opcode)
            {
                case 0x17: ProcessPacket(new SCHandshakeReply(reader)); break;
                case 0x18: ProcessPacket(new SCPlayerLoginReply(reader)); break;
                case 0x19: ProcessPacket(new SCGetServerListReply(reader)); break;
                case 0x22: ProcessPacket(new SCJoinServerReply(reader)); break;
                case 0x31: ProcessPacket(new SCSetGameFeatures(reader)); break;
                default:
                    GD.Print($" LOG IN  UNK {packet.HexEncode()}");
                    throw new NotImplementedException();
            };
        }

        private void ProcessPacket(SCHandshakeReply packet)
        {
            GD.Print($"Message: {packet.Message}");
            Network.SendAppPacket(new CSPlayerLogin(Username, Password));
        }

        private void ProcessPacket(SCSetGameFeatures packet)
        {
            // TODO: not sure if we care about this packet as it contains the expansions data
        }

        private void ProcessPacket(SCPlayerLoginReply packet)
        {
            if (packet.EQLSStr == 101)
            {
                EmitSignal(SignalName.MessageUpdate, "Logged in, retrieving server list");
                Network.SendAppPacket(new CSGetServerList());
            }
            else
            {
                EmitSignal(SignalName.MessageUpdate, "There was an error while logging in");
            }
        }

        private void ProcessPacket(SCGetServerListReply packet)
        {
            GetNode<Control>("/root/LoginScreen").QueueFree();
            PackedScene serverSelection = ResourceLoader.Load<PackedScene>("res://login_server/server_selection.tscn");
            server_selection newScene = serverSelection.Instantiate() as server_selection;
            newScene.LoadServers(packet);
            GetTree().Root.AddChild(newScene);
        }

        public void JoinServer(int id, SCGetServerListReply packet)
        {
            Network.SendAppPacket(new CSJoinServer(packet.Id[id]));
        }

        private void ProcessPacket(SCJoinServerReply packet)
        {
            GD.Print($"Accepted on server {packet.ServerId} isApproved {packet.IsApproved} message {packet.EQLSStr} account {packet.AccountId}");
            Network.Disconnect();
        }
    }
}