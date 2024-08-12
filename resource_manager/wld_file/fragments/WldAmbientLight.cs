using EQGodot.helpers;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class WldAmbientLight : WldFragment
    {
        public WldLightSourceReference LightReference { get; private set; }

        public List<int> Regions { get; private set; }

        public int Flags;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            LightReference = wld.GetFragment(Reader.ReadInt32()) as WldLightSourceReference;
            Flags = Reader.ReadInt32();
            int regionCount = Reader.ReadInt32();

            Regions = [];
            for (int i = 0; i < regionCount; ++i)
            {
                int regionId = Reader.ReadInt32();
                Regions.Add(regionId);
            }
        }
    }
}