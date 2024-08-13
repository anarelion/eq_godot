using EQGodot.resource_manager.wld_file.fragments;
using System.Collections.Generic;

namespace EQGodot.resource_manager.wld_file.data_types
{
    // Latern Extractor class
    public class SkeletonBone
    {
        public int Index;
        public string Name;
        public string FullPath;
        public string CleanedName;
        public string CleanedFullPath;
        public List<int> Children;
        public Frag13Track Track;
        public Frag2DDMSprite MeshReference;
        //public ParticleCloud ParticleCloud {
        //    get; set;
        //}
        public Dictionary<string, Frag13Track> AnimationTracks
        {
            get; set;
        }
        public SkeletonBone Parent
        {
            get; set;
        }
    }
}
