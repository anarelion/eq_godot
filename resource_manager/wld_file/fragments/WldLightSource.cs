using EQGodot.helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public class WldLightSource : WldFragment
    {
        public bool IsPlacedLightSource { get; private set; }
        public bool IsColoredLight { get; private set; }
        public Color Color { get; private set; }
        public int Attenuation { get; private set; }
        public float SomeValue { get; private set; }

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            int flags = Reader.ReadInt32();
            var bitAnalyzer = new BitAnalyzer(flags);

            if (bitAnalyzer.IsBitSet(1))
            {
                IsPlacedLightSource = true;
            }

            if (bitAnalyzer.IsBitSet(4))
            {
                IsColoredLight = true;
            }

            if (!IsPlacedLightSource)
            {
                if (!IsColoredLight)
                {
                    int something1 = Reader.ReadInt32();
                    SomeValue = Reader.ReadSingle();
                    return;
                }

                Attenuation = Reader.ReadInt32();

                float alpha = Reader.ReadSingle();
                float red = Reader.ReadSingle();
                float green = Reader.ReadSingle();
                float blue = Reader.ReadSingle();
                Color = new Color(red, green, blue, alpha);

                if (Attenuation != 1)
                {

                }

                return;
            }

            if (!IsColoredLight)
            {
                int something1 = Reader.ReadInt32();
                float something2 = Reader.ReadSingle();
                return;
            }

            // Not sure yet what the purpose of this fragment is in the main zone file
            // For now, return
            if (!IsPlacedLightSource && Name == "DEFAULT_LIGHTDEF")
            {
                int unknown = Reader.ReadInt32();
                float unknown6 = Reader.ReadSingle();
                return;
            }

            int unknown1 = Reader.ReadInt32();

            if (!IsColoredLight)
            {
                int unknown = Reader.ReadInt32();
                Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                int unknown2 = Reader.ReadInt32();
                int unknown3 = Reader.ReadInt32();

            }
            else
            {
                Attenuation = Reader.ReadInt32();

                float alpha = Reader.ReadSingle();
                float red = Reader.ReadSingle();
                float green = Reader.ReadSingle();
                float blue = Reader.ReadSingle();

                Color = new Color(red, green, blue, alpha);
            }
        }
    }
}

