using System.Collections.Generic;
using EQGodot.resource_manager.wld_file.fragments;

namespace EQGodot.resource_manager.wld_file.data_types;

// Latern Extractor class
public class SkeletonBone
{
    public List<int> Children;
    public string CleanedFullPath;
    public string CleanedName;
    public string FullPath;
    public int Index;
    public Frag2DDMSprite MeshReference;
    public string Name;

    public Frag13Track Track;

    //public ParticleCloud ParticleCloud {
    //    get; set;
    //}
    public Dictionary<string, Frag13Track> AnimationTracks { get; set; }

    public SkeletonBone Parent { get; set; }
}