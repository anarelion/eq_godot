using EQGodot.helpers;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public class WldParticleSprite : WldFragment
    {
        private WldBitmapInfoReference _bitmapReference;

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            int value04 = Reader.ReadInt32(); // flags? always 0
            _bitmapReference = wld.GetFragment(Reader.ReadInt32()) as WldBitmapInfoReference;
            int value12 = Reader.ReadInt32(); // always the same value. unlikely a float, or bytes. Not color.

        }
    }
}