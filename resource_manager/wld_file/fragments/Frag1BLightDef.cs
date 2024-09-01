using EQGodot.resource_manager.wld_file.helpers;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public class Frag1BLightDef : WldFragment
{
    public bool IsPlacedLightSource { get; private set; }
    public bool IsColoredLight { get; private set; }
    public Color Color { get; private set; }
    public int Attenuation { get; private set; }
    public float SomeValue { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        var flags = Reader.ReadInt32();
        var bitAnalyzer = new BitAnalyzer(flags);

        if (bitAnalyzer.IsBitSet(1)) IsPlacedLightSource = true;

        if (bitAnalyzer.IsBitSet(4)) IsColoredLight = true;

        if (!IsPlacedLightSource)
        {
            if (!IsColoredLight)
            {
                var something1 = Reader.ReadInt32();
                SomeValue = Reader.ReadSingle();
                return;
            }

            Attenuation = Reader.ReadInt32();

            var alpha = Reader.ReadSingle();
            var red = Reader.ReadSingle();
            var green = Reader.ReadSingle();
            var blue = Reader.ReadSingle();
            Color = new Color(red, green, blue, alpha);

            if (Attenuation != 1)
            {
            }

            return;
        }

        if (!IsColoredLight)
        {
            var something1 = Reader.ReadInt32();
            var something2 = Reader.ReadSingle();
            return;
        }

        // Not sure yet what the purpose of this fragment is in the main zone file
        // For now, return
        if (!IsPlacedLightSource && Name == "DEFAULT_LIGHTDEF")
        {
            var unknown = Reader.ReadInt32();
            var unknown6 = Reader.ReadSingle();
            return;
        }

        var unknown1 = Reader.ReadInt32();

        if (!IsColoredLight)
        {
            var unknown = Reader.ReadInt32();
            Color = new Color(1.0f, 1.0f, 1.0f);
            var unknown2 = Reader.ReadInt32();
            var unknown3 = Reader.ReadInt32();
        }
        else
        {
            Attenuation = Reader.ReadInt32();

            var alpha = Reader.ReadSingle();
            var red = Reader.ReadSingle();
            var green = Reader.ReadSingle();
            var blue = Reader.ReadSingle();

            Color = new Color(red, green, blue, alpha);
        }
    }
}