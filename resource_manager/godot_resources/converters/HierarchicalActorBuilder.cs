using System.Text.RegularExpressions;
using EQGodot.resource_manager.wld_file.fragments;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.godot_resources.converters;

internal partial class HierarchicalActorBuilder
{
    [GeneratedRegex(@"^[A-Z][0-9][0-9]")]
    private static partial Regex AnimatedTrackRegex();

    public static HierarchicalActorDefinition Convert(Frag14ActorDef actordef, Frag10HierarchicalSpriteDef skeleton,
        Dictionary<int, ArrayMesh> meshes)
    {
        var name = FragmentNameCleaner.CleanName(actordef);

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
            var bone_mesh = meshref != null && meshref.Mesh != null ? meshes[meshref.Mesh.Index] : null;
            var boneName = bone.Name.Substring(3).ToLower().Replace("_dag", "");
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
                rbone.BasePosition = ConvertTrack(track);
            }

            actor.Bones.Add(rbone);
            if (actor.BonesByName.ContainsKey(boneName)) actor.BonesByName.Remove(boneName);
            actor.BonesByName.Add(boneName, rbone);
        }

        return actor;
    }

    public static ActorSkeletonPath ConvertTrack(Frag13Track track)
    {
        string animationName = null;
        string pieceName = null;
        if (AnimatedTrackRegex().IsMatch(track.Name))
        {
            animationName = track.Name.Substr(0, 3);
            pieceName = track.Name.Substring(6).ToLower().Replace("_track", "");
            if (pieceName == "") pieceName = "root";
        }

        var skeletonPath = new ActorSkeletonPath
        {
            Name = track.Name.ToLower(),
            AnimationName = animationName,
            PieceName = pieceName,
            FrameMs = track.FrameMs,
            Flags = track.Flags,
            DefFlags = track.TrackDefFragment.Flags,
            Translation = [],
            Rotation = []
        };
        foreach (var frame in track.TrackDefFragment.Frames)
        {
            skeletonPath.Translation.Add(frame.Translation);
            skeletonPath.Rotation.Add(frame.Rotation);
        }

        return skeletonPath;
    }
}