using EQGodot.resource_manager.godot_resources;
using EQGodot.resource_manager.pack_file;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EQGodot.resource_manager
{
    [GlobalClass]
    public partial class ResourcePreparer : Node
    {

        [Signal]
        public delegate void PFSArchiveLoadedEventHandler(string path, PFSArchive pfs);

        private HashSet<string> PendingLoad;

        public ResourcePreparer()
        {
            PendingLoad = [];
        }

        public override void _Ready()
        {

        }

        public override void _Process(double delta)
        {
            foreach (var path in PendingLoad)
            {
                var state = ResourceLoader.LoadThreadedGetStatus(path);
                if (state == ResourceLoader.ThreadLoadStatus.Loaded)
                {
                    PendingLoad.Remove(path);
                    EmitSignal(SignalName.PFSArchiveLoaded, [path, ResourceLoader.LoadThreadedGet(path) as PFSArchive]);
                    GD.Print($"Finished loading {path}");
                }
            }
        }

        public void StartLoading(string path)
        {
            ResourceLoader.LoadThreadedRequest(path, "PFSArchive", true, ResourceLoader.CacheMode.Reuse);
        }
    }
}