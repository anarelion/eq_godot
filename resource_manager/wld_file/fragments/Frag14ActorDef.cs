﻿using EQGodot.resource_manager.wld_file.helpers;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;
using System;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    class Frag14ActorDef : WldFragment
    {
        public int Flags;
        public String CallbackName;
        public Frag2DDMSprite MeshReference { get; private set; }
        public Frag11HierarchicalSprite SkeletonReference { get; private set; }
        public Frag09Sprite3D CameraReference { get; private set; }
        public Frag27BlitSprite ParticleSpriteReference { get; private set; }

        //public Fragment07 Fragment07;

        public ActorType ActorType;

        public string ReferenceName;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            Flags = Reader.ReadInt32();

            BitAnalyzer ba = new(Flags);

            bool params1Exist = ba.IsBitSet(0);
            bool params2Exist = ba.IsBitSet(1);
            bool fragment2MustContainZero = ba.IsBitSet(7);

            CallbackName = wld.GetName(Reader.ReadInt32());

            // 1 for both static and animated objects
            int size1 = Reader.ReadInt32();

            // The number of components (meshes, skeletons, camera references) the actor has
            // In all Trilogy files, there is only ever 1
            int componentCount = Reader.ReadInt32();

            // 0 for both static and animated objects
            int fragment2 = Reader.ReadInt32();

            if (params1Exist)
            {
                int params1 = Reader.ReadInt32();
            }

            if (params2Exist)
            {
                Reader.BaseStream.Position += 7 * sizeof(int);
            }

            // Size 1 entries
            for (int i = 0; i < size1; ++i)
            {
                // Always 1
                int dataPairCount = Reader.ReadInt32();

                // Unknown purpose
                // Always 0 and 1.00000002E+30
                for (int j = 0; j < dataPairCount; ++j)
                {
                    int value = Reader.ReadInt32();
                    int value2 = Reader.ReadInt16();
                    int value3 = Reader.ReadInt16();
                }
            }

            if (componentCount > 1)
            {
                GD.PrintErr("Actor: More than one component references");
            }

            // Can contain either a skeleton reference (animated), mesh reference (static) or a camera reference
            for (int i = 0; i < componentCount; ++i)
            {
                var fragment = wld.GetFragment(Reader.ReadInt32());

                SkeletonReference = fragment as Frag11HierarchicalSprite;

                if (SkeletonReference != null)
                {
                    SkeletonReference.SkeletonHierarchy.IsAssigned = true;
                    break;
                }

                MeshReference = fragment as Frag2DDMSprite;

                if (MeshReference != null && MeshReference.Mesh != null)
                {
                    MeshReference.Mesh.IsHandled = true;
                    break;
                }

                //if (MeshReference != null && MeshReference.LegacyMesh != null) {
                //    break;
                //}

                // This only exists in the main zone WLD
                CameraReference = fragment as Frag09Sprite3D;

                if (CameraReference != null)
                {
                    break;
                }

                ParticleSpriteReference = fragment as Frag27BlitSprite;

                if (ParticleSpriteReference != null)
                {
                    break;
                }

                //Fragment07 = fragment as Fragment07;

                //if (Fragment07 != null) {
                //    break;
                //}

                GD.PrintErr($"Actor: Cannot link fragment with index {fragment.Index} of type {fragment.Type}");
            }

            // Always 0 in qeynos2 objects
            int name3Bytes = Reader.ReadInt32();

            CalculateActorType();
        }

        private void CalculateActorType()
        {
            if (CameraReference != null)
            {
                ActorType = ActorType.Camera;
                ReferenceName = CameraReference.Name;
            }
            else if (SkeletonReference != null)
            {
                ActorType = ActorType.Skeletal;
            }
            else if (MeshReference != null)
            {
                ActorType = ActorType.Static;
                ReferenceName = MeshReference.Name;
            }
            else if (ParticleSpriteReference != null)
            {
                ActorType = ActorType.Particle;
                ReferenceName = ParticleSpriteReference.Name;

                // } else if (Fragment07 != null) {
                //    ActorType = ActorType.Sprite;
                //    ReferenceName = Fragment07.Name;
            }
            else
            {
                GD.PrintErr("Cannot determine actor type!");
            }
        }

        public void AssignSkeletonReference(Frag10HierarchicalSpriteDef skeleton)
        {
            SkeletonReference = new Frag11HierarchicalSprite
            {
                SkeletonHierarchy = skeleton
            };

            CalculateActorType();
            skeleton.IsAssigned = true;
        }
    }
}