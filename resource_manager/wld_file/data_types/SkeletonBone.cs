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
        public WldTrackFragment Track;
        public WldMeshReference MeshReference;
        //public ParticleCloud ParticleCloud {
        //    get; set;
        //}
        public Dictionary<string, WldTrackFragment> AnimationTracks
        {
            get; set;
        }
        public SkeletonBone Parent
        {
            get; set;
        }
    }
}
