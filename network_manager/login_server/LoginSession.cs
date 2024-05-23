using System;
using System.IO;
using EQGodot2.network_manager.network_session;
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
            GD.Print($" APP Layer got {packet.HexEncode()}");
            var stream = new MemoryStream(packet);
            var reader = new BinaryReader(stream);
            var opcode = reader.ReadUInt16();
            var decoded = opcode switch
            {
                0x0017 => new SCHandshakeReply(reader),
                _ => throw new NotImplementedException(),
            };
            ProcessPacket(decoded);
        }

        private void ProcessPacket(SCHandshakeReply packet)
        {
            GD.Print($"Message: {packet.Message}");
            Network.SendAppPacket(new CSPlayerLogin(Username, Password));
        }
    }
}