namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public class Frag1CLight : WldFragment
{
    /// <summary>
    ///     The light source (0x1B) fragment reference
    /// </summary>
    public Frag1BLightDef LightSource { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        LightSource = wld.GetFragment(Reader.ReadInt32()) as Frag1BLightDef;
    }
}