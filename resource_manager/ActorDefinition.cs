using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EQGodot2.resource_manager.wld_file;
using EQGodot2.resource_manager.wld_file.data_types;
using Godot;

namespace EQGodot2.resource_manager {
    public partial class ActorDefinition : Resource {
        [Export]
        public string Tag;

        [Export]
        public int Flags;

        [Export]
        public Godot.Collections.Array<ActorSkeletonBone> Bones {
            get; set;
        }

        [Export]
        public Godot.Collections.Dictionary<string, ActorSkeletonBone> BonesByName {
            get; set;
        }

        [Export]
        public Godot.Collections.Dictionary<string, ArrayMesh> Meshes {
            get; set;
        }

        public Skeleton3D BuildSkeleton()
        {
            var skeleton = new Skeleton3D();
            skeleton.Name = Tag;
            foreach (var bone in Bones) {
                skeleton.AddBone(bone.Name);
                if (bone.Parent != null) {
                    skeleton.SetBoneParent(bone.Index, bone.Parent.Index);
                    skeleton.SetBonePosePosition(bone.Index, bone.BasePosition.Translation[0]);
                    skeleton.SetBonePoseRotation(bone.Index, bone.BasePosition.Rotation[0]);
                }
            }
            return skeleton;
        }
    }
}