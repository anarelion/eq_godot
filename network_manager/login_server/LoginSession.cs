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
            var opcode = reader.ReadUShort();
            switch (opcode)
            {
                case 0x1700: ProcessPacket(new SCHandshakeReply(reader)); break;
                case 0x1800: ProcessPacket(new SCPlayerLoginReply(reader)); break;
                case 0x3100: ProcessPacket(new SCSetGameFeatures(reader)); break;
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
        }
    }
}