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
    public class WldParticleSpriteReference : WldFragment
    {
        private WldParticleSprite _reference;
        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            _reference = wld.GetFragment(Reader.ReadInt32()) as WldParticleSprite;
            int value08 = Reader.ReadInt32(); // always 0
        }
    }
}