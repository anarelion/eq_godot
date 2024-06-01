using System;
using EQGodot2.login_server;
using EQGodot2.network_manager.login_server;
using Godot;

namespace EQGodot2.GameController
{
    public partial class GameController : Node
    {
        private Node ActiveScene;
        private LoginSession NetworkLoginSession;
        private EQServerDescription ActiveServer;
        private uint PlayerLSID;
        private byte[] PlayerKey;

        public override void _Ready()
        {
            var packed = ResourceLoader.Load<PackedScene>("res://login_server/login_screen.tscn");
            ActiveScene = packed.Instantiate<login_screen>();
            (ActiveScene as login_screen).DoLogin += OnLoginScreenDoLogin;
            AddChild(ActiveScene);
        }

        public override void _Process(double delta)
        {

        }

        private void OnLoginScreenDoLogin(string username, string password)
        {
            NetworkLoginSession = new LoginSession(username, password);
            NetworkLoginSession.MessageUpdate += (ActiveScene as login_screen).OnMessageUpdate;
            NetworkLoginSession.ServerListReceived += OnServerListReceived;
            NetworkLoginSession.LoggedIn += OnLoggedIn;
            AddChild(NetworkLoginSession);
        }

        private void OnLoggedIn(uint lsid, byte[] key)
        {
            PlayerLSID = lsid;
            PlayerKey = key;
        }

        private void OnServerListReceived(EQServerDescription[] servers)
        {
            ActiveScene.QueueFree();
            PackedScene serverSelection = ResourceLoader.Load<PackedScene>("res://login_server/server_selection.tscn");
            ActiveScene = serverSelection.Instantiate<server_selection>();
            (ActiveScene as server_selection).LoadServers(servers);
            (ActiveScene as server_selection).ServerJoinStart += OnServerJoinStart;
            AddChild(ActiveScene);
        }

        private void OnServerJoinStart(EQServerDescription server)
        {
            ActiveServer = server;
            NetworkLoginSession.ServerJoinAccepted += OnServerJoinAccepted;
            NetworkLoginSession.JoinServer(ActiveServer);
        }

        private void OnServerJoinAccepted()
        {
            ActiveScene.QueueFree();
            ActiveScene = null;
            GD.Print($"Accepted on server {ActiveServer.LongName}, proceeding to join world server");
        }

    }

}