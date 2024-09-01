using System.Collections.Generic;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public class Frag2AAmbientLight : WldFragment
{
    public int Flags;
    public Frag1CLight LightReference { get; private set; }

    public List<int> Regions { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        LightReference = wld.GetFragment(Reader.ReadInt32()) as Frag1CLight;
        Flags = Reader.ReadInt32();
        var regionCount = Reader.ReadInt32();

        Regions = [];
        for (var i = 0; i < regionCount; ++i)
        {
            var regionId = Reader.ReadInt32();
            Regions.Add(regionId);
        }
    }
}