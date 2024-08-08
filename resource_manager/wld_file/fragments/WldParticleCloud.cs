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
    public class WldParticleCloud : WldFragment
    {
        private WldParticleSprite _particleSprite;

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());

            //File.WriteAllBytes("ParticleClouds/" + Name, data);

            int flags = Reader.ReadInt32(); // always 4
            int value08 = Reader.ReadInt32(); // always 3
            int value12 = Reader.ReadInt32(); // Values are 1, 3, or 4
            byte value16 = Reader.ReadByte();
            byte value17 = Reader.ReadByte();
            byte value18 = Reader.ReadByte();
            byte value19 = Reader.ReadByte();
            int value20 = Reader.ReadInt32(); // 200, 30, particle count? 
            int value24 = Reader.ReadInt32(); // always 0
            int value28 = Reader.ReadInt32(); // always 0
            int value32 = Reader.ReadInt32(); // always 0
            int value36 = Reader.ReadInt32(); // always 0
            int value40 = Reader.ReadInt32(); // always 0
            float value44 = Reader.ReadSingle(); // confirmed float
            float value48 = Reader.ReadSingle(); // looks like a float
            int value52 = Reader.ReadInt32(); // looks like an int. numbers like 1000, 100, 750, 500, 1600, 2500.
            float value56 = Reader.ReadSingle(); // looks like a float. low numbers. 4, 5, 8, 10, 0
            float value60 = Reader.ReadSingle(); // float 0 or 1
            float value64 = Reader.ReadSingle(); // float 0 or -1
            float value68 = Reader.ReadSingle(); // float 0 or -1
            int value72 = Reader.ReadInt32(); // probably int 13, 15, 20, 600, 83? or bytes
            float value76 = Reader.ReadSingle(); // confirmed float 0.4, 0.5, 1.5, 0.1
            float value80 = Reader.ReadSingle(); // float 0.4, 1.9
            _particleSprite = wld.GetFragment(Reader.ReadInt32()) as WldParticleSprite;
        }
    }
}