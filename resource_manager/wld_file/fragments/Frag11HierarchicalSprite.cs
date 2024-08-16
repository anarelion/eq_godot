using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    class Frag11HierarchicalSprite : WldFragment
    {
        public Frag10HierarchicalSpriteDef HierarchicalSpriteDef
        {
            get; set;
        }

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);

            var reader = new BinaryReader(new MemoryStream(data));

            // Reference is usually 0
            // Confirmed
            Name = wld.GetName(Reader.ReadInt32());
            HierarchicalSpriteDef = wld.GetFragment(Reader.ReadInt32()) as Frag10HierarchicalSpriteDef;

            if (HierarchicalSpriteDef == null)
            {
                GD.PrintErr("Bad skeleton hierarchy reference");
            }

            int params1 = reader.ReadInt32();

            // Params are 0
            // Confirmed
            if (params1 != 0)
            {
                GD.Print($"Frag11HierarchicalSprite {index} -> {HierarchicalSpriteDef.Index} {wld.Name} {HierarchicalSpriteDef.Name}: has params1 {params1:X}");
            }

            // Confirmed end
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                // GD.Print($"Frag11HierarchicalSprite {index} -> {HierarchicalSpriteDef.Index} {wld.Name} {HierarchicalSpriteDef.Name}: has a remainder {reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)).HexEncode()}");
            }
        }
    }
}
