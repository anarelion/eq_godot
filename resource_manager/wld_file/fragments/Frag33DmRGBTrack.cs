namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public partial class Frag33DmRGBTrack : WldFragment
{
    public Frag32DmRGBTrackDef VertexColors { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        VertexColors = wld.GetFragment(Reader.ReadInt32()) as Frag32DmRGBTrackDef;
    }
}