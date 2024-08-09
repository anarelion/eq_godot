using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EQGodot.resource_manager.wld_file;
using EQGodot.resource_manager.wld_file.data_types;

namespace EQGodot.resource_manager.godot_resources
{
    public partial class ActorSkeletonBone : Resource
    {
        [Export]
        public int Index;
        [Export]
        public string Name;
        [Export]
        public string FullPath;
        [Export]
        public string CleanedName;
        [Export]
        public string CleanedFullPath;
        [Export]
        public Mesh ReferencedMesh;
        [Export]
        public ActorSkeletonBone Parent;
        [Export]
        public ActorSkeletonPath BasePosition;
        [Export]
        public Godot.Collections.Dictionary<string, ActorSkeletonPath> AnimationTracks
        {
            get; set;
        }

        //public ParticleCloud ParticleCloud {
        //    get; set;
        //}
    }
}
