using EQGodot.resource_manager;
using EQGodot2.login_server;
using EQGodot2.network_manager.login_server;
using EQGodot2.network_manager.world_server;
using Godot;

namespace EQGodot.GameController;

public partial class GameController : Node
{
    private Node ActiveScene;
    private EQServerDescription ActiveServer;
    private LoginSession NetworkLoginSession;
    private WorldSession NetworkWorldSession;
    private byte[] PlayerKey;
    private uint PlayerLSID;

    public ResourceManager Resources;


    public override void _Ready()
    {
        var packed = ResourceLoader.Load<PackedScene>("res://login_server/login_screen.tscn");
        ActiveScene = packed.Instantiate<login_screen>();
        ((login_screen)ActiveScene).DoLogin += OnLoginScreenDoLogin;
        AddChild(ActiveScene);

        Resources = (ResourceManager)ResourceLoader.Load<CSharpScript>("res://resource_manager/ResourceManager.cs")
            .New();
        AddChild(Resources);
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
        var serverSelection = ResourceLoader.Load<PackedScene>("res://login_server/server_selection.tscn");
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

        Resources = (ResourceManager)ResourceLoader.Load<CSharpScript>("res://resource_manager/ResourceManager.cs")
            .New();
        AddChild(Resources);

        var serverSelection = ResourceLoader.Load<PackedScene>("res://base_scene.tscn");
        ActiveScene = serverSelection.Instantiate<Node3D>();
        AddChild(ActiveScene);

        GD.Print($"Accepted on server {ActiveServer.LongName}, proceeding to join world server");

        NetworkWorldSession = new WorldSession(PlayerLSID, PlayerKey, ActiveServer);
        AddChild(NetworkWorldSession);
    }
}