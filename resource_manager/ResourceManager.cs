using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using EQGodot.resource_manager.godot_resources;
using EQGodot.resource_manager.pack_file;
using EQGodot.resource_manager.wld_file.fragments;
using Godot;

namespace EQGodot.resource_manager;

[GlobalClass]
public partial class ResourceManager : Node
{
    private Godot.Collections.Dictionary<string, BlitActorDefinition> _blitActor = [];
    private Godot.Collections.Dictionary<string, ActorSkeletonPath> _extraAnimations = [];
    private Godot.Collections.Dictionary<string, HierarchicalActorDefinition> _hierarchicalActor = [];
    private Frag21WorldTree _activeZone = null;

    private ResourcePreparer _preparer;
    private Node3D _sceneRoot;

    private ResourceManager()
    {
        GD.Print("Starting Resource Manager!");
        Name = "ResourceManager";

        _preparer = new ResourcePreparer();
        _preparer.PfsArchiveLoaded += OnPFSArchiveLoaded;
        _preparer.AllFilesLoaded += OnAllFilesLoaded;
        AddChild(_preparer);
        
        _sceneRoot = new Node3D();
        _sceneRoot.RotateX((float)(-Math.PI / 2));
        AddChild(_sceneRoot);

        StartLoading("res://eq_files/gequip.s3d");
        StartLoading("res://eq_files/global_chr.s3d");
        StartLoading("res://eq_files/load2_obj.s3d");
        StartLoading("res://eq_files/load2.s3d");
    }



    private void StartLoading(string path)
    {
        _preparer.StartLoading(path);
    }

    private void OnPFSArchiveLoaded(string path, PFSArchive pfs)
    {
        foreach (var wldFile in pfs.WldFiles)
        {
            GD.Print($"OnPFSArchiveLoaded: Loaded {wldFile.Key}");
            if (wldFile.Value.WorldTree != null)
            {
                GD.Print($"OnPFSArchiveLoaded: activating zone {wldFile.Key}");
                _activeZone = wldFile.Value.WorldTree;
            }

            foreach (var actorDef in wldFile.Value.ActorDefs)
            {
                var name = actorDef.Value.ResourceName;
                switch (actorDef.Value)
                {
                    case HierarchicalActorDefinition hierarchicalActor:
                        _hierarchicalActor[name] = hierarchicalActor;
                        GD.Print($"OnPFSArchiveLoaded: Loaded HierarchicalActorDefinition {name}");
                        break;
                    case BlitActorDefinition blitActor:
                        _blitActor[name] = blitActor;
                        GD.Print($"OnPFSArchiveLoaded: Loaded BlitActorDefinition {name}");
                        break;
                }
            }

            foreach (var animation in wldFile.Value.ExtraAnimations.Values)
            {
                // GD.Print($"Loading Animation {animation.Name}");
                if (_extraAnimations.TryAdd(animation.Name, animation)) continue;
                GD.Print($"OnPFSArchiveLoaded: {animation.Name} already exists");
            }
        }
    }
    
    private void OnAllFilesLoaded()
    {
        throw new NotImplementedException();
    }

    private static string ConvertAnimationTag(string tagName)
    {
        return tagName;
    }

    public void InstantiateActor(string tag)
    {
        if (_hierarchicalActor.TryGetValue(tag, out var actor))
        {
            GD.Print($"Instantiating Hierarchical Actor {actor.ResourceName}");
            _sceneRoot.AddChild(actor.InstantiateCharacter(this));
        }
    }

    public void InstantiateZone()
    {
        if (_activeZone == null) return;
        GD.Print($"Instantiating zone {_activeZone}");
        _sceneRoot.AddChild(_activeZone.ToGodotZone());
    }

    public List<ActorSkeletonPath> GetAnimationsFor(string actorName)
    {
        GD.Print($"Getting animations for {actorName}");
        List<ActorSkeletonPath> result = [];
        result.AddRange(_extraAnimations.Values.Where(animation => animation.ActorName == actorName));
        GD.Print($"Got {result.Count} out of {_extraAnimations.Count} animations for {actorName}");
        return result;
    }
    
    // Zone loading orchestration
    // Notes, the order in which the original client loads files is
    // - %s_environmentEmitters.txt -> load whatever this points to
    // - %s_%2s_obj2 -> with second argument being country code for asian countries or us
    // - %s_obj2 -> load item definitions
    // - %s_%2s_obj -> with second argument being country code for asian countries or us
    // - %s_obj -> load item definitions
    // - %s_%2s_2_obj -> with second argument being country code for asian countries or us
    // - %s_2_obj -> load item definitions
    // - %s_chr2 -> load character definitions
    // - %s2_chr -> load character definitions
    // - %s_chr -> load character definitions
    // - %s_chr.txt -> load whatever this points to
    // - %s_assets.txt -> load whatever this points to
    // - load main
    // - process objects.wld
    // - process lights.wld
    // - process %s.wld
    
    
    public void LoadZone(string zoneName)
    {
        
    }
}