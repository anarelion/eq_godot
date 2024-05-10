using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    class WldSkeletonHierarchyReference : WldFragment {
        public WldSkeletonHierarchy SkeletonHierarchy {
            get; set;
        }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);

            var reader = new BinaryReader(new MemoryStream(data));

            // Reference is usually 0
            // Confirmed
            Name = stringHash[-reader.ReadInt32()];

            int reference = reader.ReadInt32();

            SkeletonHierarchy = fragments[reference - 1] as WldSkeletonHierarchy;

            if (SkeletonHierarchy == null) {
                GD.PrintErr("Bad skeleton hierarchy reference");
            }

            int params1 = reader.ReadInt32();

            // Params are 0
            // Confirmed
            if (params1 != 0) {

            }

            // Confirmed end
            if (reader.BaseStream.Position != reader.BaseStream.Length) {

            }
        }

        public override void OutputInfo()
        {
            base.OutputInfo();

            if (SkeletonHierarchy != null) {
                GD.Print("-----");
                GD.Print("0x11: Skeleton track reference: " + SkeletonHierarchy.Index + 1);
            }
        }
    }
}
