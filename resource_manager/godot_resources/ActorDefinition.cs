﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EQGodot.resource_manager.wld_file;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;

namespace EQGodot.resource_manager.godot_resources
{
    public partial class ActorDefinition : Resource
    {
        [Export]
        public string Tag;

        [Export]
        public int Flags;

        [Export]
        public Godot.Collections.Array<ActorSkeletonBone> Bones
        {
            get; set;
        }

        [Export]
        public Godot.Collections.Dictionary<string, ActorSkeletonBone> BonesByName
        {
            get; set;
        }

        [Export]
        public Godot.Collections.Dictionary<string, ArrayMesh> Meshes
        {
            get; set;
        }

        public Skeleton3D BuildSkeleton()
        {
            var skeleton = new Skeleton3D
            {
                Name = Tag
            };
            foreach (var bone in Bones)
            {
                skeleton.AddBone(bone.Name);
                if (bone.Parent != null)
                {
                    skeleton.SetBoneParent(bone.Index, bone.Parent.Index);
                    skeleton.SetBonePosePosition(bone.Index, bone.BasePosition.Translation[0]);
                    skeleton.SetBonePoseRotation(bone.Index, bone.BasePosition.Rotation[0]);
                }
            }
            return skeleton;
        }
    }
}