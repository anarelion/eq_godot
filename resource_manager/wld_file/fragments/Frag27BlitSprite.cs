using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Lantern Extractor class
public class Frag27BlitSprite : WldFragment
{
    public Frag26BlitSpriteDef BlitSpriteDef;
    public int Flags;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        BlitSpriteDef = wld.GetFragment(Reader.ReadInt32()) as Frag26BlitSpriteDef;
        Flags = Reader.ReadInt32();
        if (Flags != 0) GD.PrintErr($"Frag27BlitSprite flags: {Flags}");
    }
}