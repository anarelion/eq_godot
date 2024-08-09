using EQGodot.resource_manager.godot_resources;
using EQGodot.resource_manager.pack_file;
using Godot;
using System;

namespace EQGodot.resource_manager
{
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
                "res://eq_files/airplane_chr.s3d",
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
                if (boneName == "")
                {
                    boneName = "root";
                }

                if (CharacterActor.TryGetValue(actorName, out ActorDefinition actor))
                {
                    if (actor.BonesByName.TryGetValue(boneName, out ActorSkeletonBone bone))
                    {
                        bone.AnimationTracks[animationName] = animation;
                    }
                }
            }

            AddChild(InstantiateCharacter("avi"));
        }

        public ActorInstance InstantiateCharacter(string tagName)
        {
            var actorDef = CharacterActor[tagName];
            var node = new ActorInstance
            {
                Name = actorDef.ResourceName
            };

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
                Name = tagName + "_anim",
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
                        if (animationTag == "P01")
                        {
                            a.LoopMode = Animation.LoopModeEnum.Linear;
                        }
                        animationDict.Add(boneAnim.AnimationName, a);
                        animationLibrary.AddAnimation(boneAnim.AnimationName, a);
                    }
                    var gdAnimation = animationDict[boneAnim.AnimationName];
                    var bonePath = new NodePath($":{boneAnim.PieceName}");
                    var posIdx = gdAnimation.AddTrack(Animation.TrackType.Position3D);
                    gdAnimation.TrackSetPath(posIdx, bonePath);

                    var rotIdx = gdAnimation.AddTrack(Animation.TrackType.Rotation3D);
                    gdAnimation.TrackSetPath(rotIdx, bonePath);
                    gdAnimation.TrackSetInterpolationType(rotIdx, Animation.InterpolationType.LinearAngle);
                    for (int frame = 0; frame < boneAnim.Translation.Count; frame++)
                    {
                        gdAnimation.PositionTrackInsertKey(posIdx, frame * 0.001f * boneAnim.FrameMs, boneAnim.Translation[frame]);
                        gdAnimation.RotationTrackInsertKey(rotIdx, frame * 0.001f * boneAnim.FrameMs, boneAnim.Rotation[frame]);
                    }
                    gdAnimation.Length = Math.Max(gdAnimation.Length, boneAnim.Translation.Count * 0.001f * boneAnim.FrameMs);
                }
            }

            animationPlayer.AddAnimationLibrary(tagName, animationLibrary);
            skeleton.AddChild(animationPlayer);
            animationPlayer.Play($"{tagName}/P01");
            return node;
        }

        private string convertAnimationTag(string tagName)
        {
            return tagName;
        }
    }

}