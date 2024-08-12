using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    class WldSkeletonHierarchyReference : WldFragment
    {
        public WldSkeletonHierarchy SkeletonHierarchy
        {
            get; set;
        }

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);

            var reader = new BinaryReader(new MemoryStream(data));

            // Reference is usually 0
            // Confirmed
            Name = wld.GetName(Reader.ReadInt32());
            SkeletonHierarchy = wld.GetFragment(Reader.ReadInt32()) as WldSkeletonHierarchy;

            if (SkeletonHierarchy == null)
            {
                GD.PrintErr("Bad skeleton hierarchy reference");
            }

            int params1 = reader.ReadInt32();

            // Params are 0
            // Confirmed
            if (params1 != 0)
            {

            }

            // Confirmed end
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {

            }
        }
    }
}
