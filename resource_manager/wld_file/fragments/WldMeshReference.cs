using Godot;

namespace EQGodot.resource_manager.wld_file.fragments
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

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
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
