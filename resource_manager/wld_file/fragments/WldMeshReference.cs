using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public class WldMeshReference : WldFragment
    {
        public WldMesh Mesh
        {
            get; private set;
        }

        public WldLegacyMesh LegacyMesh
        {
            get; private set;
        }

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            WldFragment fragment = wld.GetFragment(Reader.ReadInt32());

            Mesh = fragment as WldMesh;
            if (Mesh != null)
            {
                return;
            }

            LegacyMesh = fragment as WldLegacyMesh;
            if (LegacyMesh != null)
            {
                return;
            }

            GD.PrintErr($"No mesh reference found for fragment {Index} pointing to {fragment.Index}");
        }
    }
}
