using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
[GlobalClass]
public partial class Frag2DDMSprite : WldFragment
{
    [Export] public Frag36DmSpriteDef2 Mesh;

    [Export] public Frag2CDMSpriteDef LegacyMesh;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        var fragment = wld.GetFragment(Reader.ReadInt32());

        Mesh = fragment as Frag36DmSpriteDef2;
        if (Mesh != null) return;

        LegacyMesh = fragment as Frag2CDMSpriteDef;
        if (LegacyMesh != null) return;

        GD.PrintErr($"No mesh reference found for fragment {Index} pointing to {fragment.Index}");
    }
}