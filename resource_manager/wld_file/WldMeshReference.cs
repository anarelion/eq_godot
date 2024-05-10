using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public class WldMeshReference : WldFragment {
        public WldMesh Mesh {
            get; private set;
        }

        //public LegacyMesh LegacyMesh {
        //    get; private set;
        //}

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];
            int reference = Reader.ReadInt32() - 1;
            Mesh = fragments[reference] as WldMesh;

            if (Mesh != null) {
                return;
            }

            //LegacyMesh = fragments[reference] as LegacyMesh;

            //if (LegacyMesh != null) {
            //    return;
            //}

            GD.PrintErr("No mesh reference found for id: " + reference);
        }

        public override void OutputInfo()
        {
            base.OutputInfo();

            if (Mesh != null) {
                GD.Print("-----");
                GD.Print("0x2D: Mesh reference: " + Mesh.Index);
            }
        }
    }
}
