using EQGodot.resource_manager.wld_file;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class WldVertexColorsReference : WldFragment
    {
        public WldVertexColors VertexColors { get; private set; }

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            VertexColors = wld.GetFragment(Reader.ReadInt32()) as WldVertexColors;
        }
    }
}