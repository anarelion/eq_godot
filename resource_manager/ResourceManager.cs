using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using EQGodot.resource_manager.godot_resources;
using EQGodot.resource_manager.pack_file;
using Godot;

namespace EQGodot.resource_manager;

[GlobalClass]
public partial class ResourceManager : Node
{
    private Godot.Collections.Dictionary<string, BlitActorDefinition> _blitActor = [];
    private Godot.Collections.Dictionary<string, ActorSkeletonPath> _extraAnimations = [];
    private Godot.Collections.Dictionary<string, HierarchicalActorDefinition> _hierarchicalActor = [];

    private ResourcePreparer _preparer;

    private ResourceManager()
    {
        GD.Print("Starting Resource Manager!");
        Name = "ResourceManager";

        _preparer = new ResourcePreparer();
        _preparer.PfsArchiveLoaded += OnPFSArchiveLoaded;
        AddChild(_preparer);

        // StartLoading("res://eq_files/gequip.s3d");
        StartLoading("res://eq_files/global_chr.s3d");
        // StartLoading("res://eq_files/load2_obj.s3d");
        // StartLoading("res://eq_files/load2.s3d");
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

    private static string ConvertAnimationTag(string tagName)
    {
        return tagName;
    }

    public void InstantiateActor(string tag)
    {
        if (_hierarchicalActor.TryGetValue(tag, out var actor))
        {
            GD.Print($"Instantiating Hierarchical Actor {actor.ResourceName}");
            AddChild(actor.InstantiateCharacter(this));
        }
    }

    public List<ActorSkeletonPath> GetAnimationsFor(string actorName)
    {
        GD.Print($"Getting animations for {actorName}");
        List<ActorSkeletonPath> result = [];
        result.AddRange(_extraAnimations.Values.Where(animation => animation.ActorName == actorName));
        GD.Print($"Got {result.Count} out of {_extraAnimations.Count} animations for {actorName}");
        return result;
    }
}