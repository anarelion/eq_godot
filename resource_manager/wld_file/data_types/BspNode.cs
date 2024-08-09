using EQGodot.resource_manager.wld_file.fragments;

namespace EQGodot.resource_manager.wld_file.data_types
{
    public class BspNode
    {
        public float NormalX { get; set; }
        public float NormalY { get; set; }
        public float NormalZ { get; set; }
        public float SplitDistance { get; set; }
        public int RegionId { get; set; }
        public int LeftNode { get; set; }
        public int RightNode { get; set; }
        public WldBspRegion Region { get; set; }
    }
}