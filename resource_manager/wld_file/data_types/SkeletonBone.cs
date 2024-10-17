using EQGodot.resource_manager.wld_file.fragments;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.wld_file.data_types;

// Latern Extractor class
[GlobalClass]
public partial class SkeletonBone: Resource
{
    [Export] public Array<int> Children;
    [Export] public string CleanedFullPath;
    [Export] public string CleanedName;
    [Export] public string FullPath;
    [Export] public int Index;
    [Export] public Frag2DDMSprite MeshReference;
    [Export] public string Name;

    [Export] public Frag13Track Track;

    //public ParticleCloud ParticleCloud {
    //    get; set;
    //}
    [Export] public Dictionary<string, Frag13Track> AnimationTracks;

    [Export] public SkeletonBone Parent;
}