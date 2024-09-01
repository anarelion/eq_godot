using System;
using System.Collections.Generic;
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

        _preparer = new ResourcePreparer();
        _preparer.PfsArchiveLoaded += OnPFSArchiveLoaded;
        AddChild(_preparer);

        StartLoading("res://eq_files/gequip.s3d");
        // StartLoading("res://eq_files/global_chr.s3d");
        // StartLoading("res://eq_files/load2_obj.s3d");
        // StartLoading("res://eq_files/load2.s3d");
    }

    private void StartLoading(string path)
    {
        _preparer.StartLoading(path);
    }

    private void Remainder()
    {
        string[] paths =
        [
            "res://eq_files/gequip.s3d",
            "res://eq_files/global_chr.s3d",
            "res://eq_files/airplane_chr.s3d"
        ];

        foreach (var path in paths)
        {
            var pfsArchive = GD.Load<PFSArchive>(path);
            foreach (var wldFile in pfsArchive.WldFiles)
            {
                GD.Print($"Loaded {wldFile.Key}");
                foreach (var actorDef in wldFile.Value.ActorDefs)
                {
                    var name = actorDef.Value.ResourceName;
                    _hierarchicalActor[name] = (HierarchicalActorDefinition)actorDef.Value;
                    GD.Print($"Loaded {name}");
                }

                foreach (var animation in wldFile.Value.ExtraAnimations.Values)
                {
                    if (_extraAnimations.TryAdd(animation.Name, animation)) continue;
                    GD.Print($"{animation.Name} already exists");
                }
            }
        }

        foreach (var animation in _extraAnimations.Values)
        {
            var animationName = animation.Name.Substr(0, 3);
            var actorName = animation.Name.Substr(3, 3);
            var boneName = animation.Name[6..].Replace("_track", "");
            if (boneName == "") boneName = "root";

            if (!_hierarchicalActor.TryGetValue(actorName, out var actor)) continue;
            if (actor.BonesByName.TryGetValue(boneName, out var bone)) bone.AnimationTracks[animationName] = animation;
        }

        AddChild(InstantiateCharacter("avi"));
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
                if (_extraAnimations.TryAdd(animation.Name, animation)) continue;
                GD.Print($"OnPFSArchiveLoaded: {animation.Name} already exists");
            }
        }
    }

    public ActorInstance InstantiateCharacter(string tagName)
    {
        var actorDef = _hierarchicalActor[tagName];
        var node = new ActorInstance { Name = actorDef.ResourceName };

        var skeleton = actorDef.BuildSkeleton();
        // swap Y and Z to get a godot coordinate system
        skeleton.RotateX((float)(-Math.PI / 2));
        node.AddChild(skeleton);
        foreach (var mesh in actorDef.Meshes)
        {
            var inst = new MeshInstance3D { Name = mesh.Key, Mesh = mesh.Value };
            skeleton.AddChild(inst);
        }

        var animationTag = ConvertAnimationTag(tagName);

        var animationPlayer = new AnimationPlayer { Name = tagName + "_anim" };

        var animationLibrary = new AnimationLibrary();

        Godot.Collections.Dictionary<string, Animation> animationDict = [];

        foreach (var bone in actorDef.Bones)
        foreach (var boneAnim in bone.AnimationTracks.Values)
        {
            if (!animationDict.TryGetValue(boneAnim.AnimationName, out var value))
            {
                var a = new Animation();
                if (animationTag == "P01") a.LoopMode = Animation.LoopModeEnum.Linear;

                value = a;
                animationDict.Add(boneAnim.AnimationName, value);
                animationLibrary.AddAnimation(boneAnim.AnimationName, a);
            }

            var gdAnimation = value;
            var bonePath = new NodePath($":{boneAnim.PieceName}");
            var posIdx = gdAnimation.AddTrack(Animation.TrackType.Position3D);
            gdAnimation.TrackSetPath(posIdx, bonePath);

            var rotIdx = gdAnimation.AddTrack(Animation.TrackType.Rotation3D);
            gdAnimation.TrackSetPath(rotIdx, bonePath);
            gdAnimation.TrackSetInterpolationType(
                rotIdx,
                Animation.InterpolationType.LinearAngle
            );
            for (var frame = 0; frame < boneAnim.Translation.Count; frame++)
            {
                gdAnimation.PositionTrackInsertKey(
                    posIdx,
                    frame * 0.001f * boneAnim.FrameMs,
                    boneAnim.Translation[frame]
                );
                gdAnimation.RotationTrackInsertKey(
                    rotIdx,
                    frame * 0.001f * boneAnim.FrameMs,
                    boneAnim.Rotation[frame]
                );
            }

            gdAnimation.Length = Math.Max(
                gdAnimation.Length,
                boneAnim.Translation.Count * 0.001f * boneAnim.FrameMs
            );
        }

        animationPlayer.AddAnimationLibrary(tagName, animationLibrary);
        skeleton.AddChild(animationPlayer);
        animationPlayer.Play($"{tagName}/P01");
        return node;
    }

    private static string ConvertAnimationTag(string tagName)
    {
        return tagName;
    }
}