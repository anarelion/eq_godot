using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.godot_resources;

[GlobalClass]
public partial class HierarchicalActorDefinition : ActorDefinition
{
    [Export] public int Flags;

    [Export] public string Tag;

    [Export] public Array<ActorSkeletonBone> Bones { get; set; }

    [Export] public Dictionary<string, ActorSkeletonBone> BonesByName { get; set; }

    [Export] public Dictionary<string, ArrayMesh> Meshes { get; set; }

    public HierarchicalActorInstance InstantiateCharacter(ResourceManager resourceManager)
    {
        var node = new HierarchicalActorInstance { Name = ResourceName };

        var skeleton = BuildSkeleton();
        // swap Y and Z to get a godot coordinate system
        skeleton.RotateX((float)(-Math.PI / 2));
        node.AddChild(skeleton);
        foreach (var mesh in Meshes)
        {
            var inst = new MeshInstance3D { Name = mesh.Key, Mesh = mesh.Value };
            skeleton.AddChild(inst);
        }

        var animationPlayer = new AnimationPlayer { Name = Tag + "_anim_player" };
        var animationLibrary = new AnimationLibrary();
        
        var animations = resourceManager.GetAnimationsFor(Tag).GroupBy(r => r.AnimationName)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var group in animations)
        {
            var animation = new Animation();
            animation.ResourceName = group.Key;
            animation.LoopMode = Animation.LoopModeEnum.Linear;
            foreach (var bone in group.Value)
            {
                bone.ApplyToAnimation(animation);
            }

            animationLibrary.AddAnimation(group.Key, animation);
        }
        
        animationPlayer.AddAnimationLibrary($"{Tag}_library", animationLibrary);
        skeleton.AddChild(animationPlayer);
        animationPlayer.Play($"{Tag}_library/l01");
        return node;
    }

    private Skeleton3D BuildSkeleton()
    {
        var skeleton = new Skeleton3D
        {
            Name = $"{Tag}_Skeleton"
        };
        GD.Print($"Building {skeleton.Name} skeleton");
        foreach (var bone in Bones)
        {
            skeleton.AddBone(bone.Name);
            if (bone.Parent == null) continue;
            skeleton.SetBoneParent(bone.Index, bone.Parent.Index);
            skeleton.SetBonePosePosition(bone.Index, bone.BasePosition.Translation[0]);
            skeleton.SetBonePoseRotation(bone.Index, bone.BasePosition.Rotation[0]);
        }

        return skeleton;
    }
}