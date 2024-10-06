using System.Text.RegularExpressions;
using EQGodot.resource_manager.wld_file.fragments;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.godot_resources.converters;

internal partial class HierarchicalActorBuilder
{
    public static HierarchicalActorDefinition Convert(Frag14ActorDef actordef, Frag10HierarchicalSpriteDef skeleton,
        Dictionary<int, ArrayMesh> meshes)
    {
        var name = FragmentNameCleaner.CleanName(actordef);
        GD.Print($"HierarchicalActorBuilder::Convert: {name}");

        HierarchicalActorDefinition actor = new()
        {
            ResourceName = name,
            Tag = name,
            Flags = actordef.Flags,
            Bones = [],
            BonesByName = [],
            Meshes = []
        };

        skeleton.BuildSkeletonData(false);
        foreach (var skel_mesh in skeleton.Meshes) actor.Meshes.Add(skel_mesh.Name, meshes[skel_mesh.Index]);

        foreach (var bone in skeleton.Skeleton)
        {
            var meshref = bone.MeshReference;
            var bone_mesh = meshref is { Mesh: not null } ? meshes[meshref.Mesh.Index] : null;
            var boneName = bone.Name[3..].ToLower().Replace("_dag", "");
            if (boneName == "") boneName = "root";
            var rbone = new ActorSkeletonBone
            {
                ResourceName = bone.Name,
                Index = bone.Index,
                Name = boneName,
                FullPath = bone.FullPath,
                CleanedName = bone.CleanedName,
                CleanedFullPath = bone.CleanedFullPath,
                ReferencedMesh = bone_mesh,
                Parent = bone.Parent != null ? actor.Bones[bone.Parent.Index] : null
            };
            var track = bone.Track;
            if (track != null)
            {
                track.IsProcessed = true;
                track.IsPoseAnimation = true;
                rbone.BasePosition = ActorSkeletonPath.FromFrag13Track(track);
            }

            actor.Bones.Add(rbone);
            if (actor.BonesByName.ContainsKey(boneName)) actor.BonesByName.Remove(boneName);
            actor.BonesByName.Add(boneName, rbone);
        }

        return actor;
    }

 
}