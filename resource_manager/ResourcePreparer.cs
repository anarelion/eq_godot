using System.Collections.Generic;
using System.Linq;
using EQGodot.resource_manager.pack_file;
using Godot;

namespace EQGodot.resource_manager;

[GlobalClass]
public partial class ResourcePreparer : Node
{
    [Signal]
    public delegate void PfsArchiveLoadedEventHandler(string path, PFSArchive pfs);

    [Signal]
    public delegate void AllFilesLoadedEventHandler();

    private readonly HashSet<string> _pendingLoad = [];

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        foreach (var path in from path in _pendingLoad
                 let state = ResourceLoader.LoadThreadedGetStatus(path)
                 where state == ResourceLoader.ThreadLoadStatus.Loaded
                 select path)
        {
            _pendingLoad.Remove(path);
            EmitSignal(SignalName.PfsArchiveLoaded, path, ResourceLoader.LoadThreadedGet(path) as PFSArchive);
            GD.Print($"Finished loading {path}");
            if (_pendingLoad.Count == 0)
                EmitSignal(SignalName.AllFilesLoaded);
        }
    }

    public void StartLoading(string path)
    {
        ResourceLoader.LoadThreadedRequest(path, "PFSArchive", true);
        _pendingLoad.Add(path);
    }
}