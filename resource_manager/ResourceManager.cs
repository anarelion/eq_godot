using Godot;
using System;
using EQGodot2.resource_manager.pack_file;
using EQGodot2.resource_manager;
using EQGodot2.helpers;

[GlobalClass]
public partial class ResourceManager : Node
{
    private Godot.Collections.Dictionary<string, ActorDefinition> CharacterActor;
    private Godot.Collections.Dictionary<string, ActorSkeletonPath> ExtraAnimations;

    private ResourceManager()
    {
        GD.Print("Starting Resource Manager!");

        CharacterActor = [];
        ExtraAnimations = [];

        string[] paths = [
            "res://eq_files/gequip.s3d",
            "res://eq_files/global_chr.s3d",
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
                    CharacterActor[name] = actorDef.Value;
                    GD.Print($"Loaded {name}");
                }
                foreach (var animation in wldFile.Value.ExtraAnimations.Values)
                {
                    if (ExtraAnimations.ContainsKey(animation.Name))
                    {
                        GD.Print($"{animation.Name} already exists");
                        continue;
                    }
                    ExtraAnimations.Add(animation.Name, animation);
                }
            }
        }

        foreach (var animation in ExtraAnimations.Values)
        {
            var animationName = animation.Name.Substr(0, 3);
            var actorName = animation.Name.Substr(3, 3);
            var boneName = animation.Name.Substring(6).Replace("_track", "");
            if (boneName == "") {
                boneName = "root";
            }

            // GD.Print($"{animationName} - {actorName} - {boneName}");

            if (CharacterActor.ContainsKey(actorName))
            {
                var actor = CharacterActor[actorName];
                if (actor.BonesByName.ContainsKey(boneName))
                {
                    var bone = actor.BonesByName[boneName];
                    bone.AnimationTracks[animationName] = animation;
                }
            }
        }

        AddChild(InstantiateCharacter("dwf"));
    }

    public Node3D InstantiateCharacter(string tagName)
    {
        var actorDef = CharacterActor[tagName];
        var node = new Node3D();
        node.Name = actorDef.ResourceName;

        var skeleton = actorDef.BuildSkeleton();
        // swap Y and Z to get a godot coordinate system
        skeleton.RotateX((float)(-Math.PI / 2));
        node.AddChild(skeleton);
        foreach (var mesh in actorDef.Meshes)
        {
            var inst = new MeshInstance3D
            {
                Name = mesh.Key,
                Mesh = mesh.Value,
            };
            skeleton.AddChild(inst);
        }

        var animationTag = convertAnimationTag(tagName);

        var animationPlayer = new AnimationPlayer
        {
            Name = tagName
        };

        var animationLibrary = new AnimationLibrary();

        Godot.Collections.Dictionary<string, Animation> animationDict = [];

        foreach (var bone in actorDef.Bones)
        {
            foreach (var boneAnim in bone.AnimationTracks.Values)
            {
                if (!animationDict.ContainsKey(boneAnim.AnimationName))
                {
                    var a = new Animation();
                    animationDict.Add(boneAnim.AnimationName, a);
                    animationLibrary.AddAnimation(animationTag, a);
                }
                var gdAnimation = animationDict[boneAnim.AnimationName];
                var bonePath = new NodePath($":{boneAnim.PieceName}");
                var posIdx = gdAnimation.AddTrack(Animation.TrackType.Position3D);
                gdAnimation.TrackSetPath(posIdx, bonePath);

                var rotIdx = gdAnimation.AddTrack(Animation.TrackType.Rotation3D);
                gdAnimation.TrackSetPath(rotIdx, bonePath);
                gdAnimation.TrackSetInterpolationType(rotIdx, Animation.InterpolationType.LinearAngle);
                for (int frame = 0; frame < boneAnim.Rotation.Count; frame++)
                {
                    gdAnimation.PositionTrackInsertKey(posIdx, frame * 0.001 * boneAnim.FrameMs, boneAnim.Translation[frame]);
                    gdAnimation.RotationTrackInsertKey(rotIdx, frame * 0.001 * boneAnim.FrameMs, boneAnim.Rotation[frame]);
                }
            }
        }

        foreach (var anim in animationDict) {
            animationLibrary.AddAnimation(anim.Key, anim.Value);
        }

        animationPlayer.AddAnimationLibrary(tagName, animationLibrary);
        skeleton.AddChild(animationPlayer);
        return node;
    }

    private string convertAnimationTag(string tagName)
    {
        return tagName;
    }
}

