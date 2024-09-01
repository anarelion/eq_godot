using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.godot_resources;

public partial class HierarchicalActorDefinition : Resource
{
    [Export] public int Flags;

    [Export] public string Tag;

    [Export] public Array<ActorSkeletonBone> Bones { get; set; }

    [Export] public Dictionary<string, ActorSkeletonBone> BonesByName { get; set; }

    [Export] public Dictionary<string, ArrayMesh> Meshes { get; set; }

    public Skeleton3D BuildSkeleton()
    {
        var skeleton = new Skeleton3D
        {
            Name = Tag
        };
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