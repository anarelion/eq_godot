using EQGodot.resource_manager.wld_file.helpers;
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

        public Frag07Sprite2D Sprite2D;
        public Frag09Sprite3D Sprite3D { get; private set; }
        public Frag11HierarchicalSprite HierarchicalSprite { get; private set; }
        public Frag27BlitSprite BlitSprite { get; private set; }
        public Frag2DDMSprite DMSprite { get; private set; }

        public ActorType ActorType;

        public string ReferenceName;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            Flags = Reader.ReadInt32();  // 0x04

            BitAnalyzer ba = new(Flags);

            bool params1Exist = ba.IsBitSet(0);
            bool params2Exist = ba.IsBitSet(1);
            bool fragment2MustContainZero = ba.IsBitSet(7);

            CallbackName = wld.GetName(Reader.ReadInt32());  // 0x08

            // 1 for both static and animated objects
            int size1 = Reader.ReadInt32();  // 0x0c

            // The number of components (meshes, skeletons, camera references) the actor has
            // In all Trilogy files, there is only ever 1
            int componentCount = Reader.ReadInt32();  // 0x10

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

                Sprite2D = fragment as Frag07Sprite2D;
                if (Sprite2D != null)
                {
                    break;
                }

                Sprite3D = fragment as Frag09Sprite3D;
                if (Sprite3D != null)
                {
                    break;
                }

                HierarchicalSprite = fragment as Frag11HierarchicalSprite;
                if (HierarchicalSprite != null)
                {
                    HierarchicalSprite.HierarchicalSpriteDef.IsAssigned = true;
                    break;
                }

                BlitSprite = fragment as Frag27BlitSprite;
                if (BlitSprite != null)
                {
                    break;
                }

                DMSprite = fragment as Frag2DDMSprite;
                if (DMSprite != null && DMSprite.Mesh != null)
                {
                    DMSprite.Mesh.IsHandled = true;
                    break;
                }

                GD.PrintErr($"Actor: Cannot link fragment with index {fragment.Index} of type {fragment.Type}");
            }

            // Always 0 in qeynos2 objects
            int name3Bytes = Reader.ReadInt32();

            CalculateActorType();
        }

        private void CalculateActorType()
        {
            if (Sprite3D != null)
            {
                ActorType = ActorType.Camera;
                ReferenceName = Sprite3D.Name;
            }
            else if (HierarchicalSprite != null)
            {
                ActorType = ActorType.Skeletal;
                ReferenceName = HierarchicalSprite.Name;
            }
            else if (DMSprite != null)
            {
                ActorType = ActorType.Static;
                ReferenceName = DMSprite.Name;
            }
            else if (BlitSprite != null)
            {
                ActorType = ActorType.Particle;
                ReferenceName = BlitSprite.Name;
            }
            else if (Sprite2D != null)
            {
                ActorType = ActorType.Sprite;
                ReferenceName = Sprite2D.Name;
            }
            else
            {
                GD.PrintErr("Cannot determine actor type!");
            }
        }

        public void AssignSkeletonReference(Frag10HierarchicalSpriteDef skeleton)
        {
            HierarchicalSprite = new Frag11HierarchicalSprite
            {
                HierarchicalSpriteDef = skeleton
            };

            CalculateActorType();
            skeleton.IsAssigned = true;
        }
    }
}
