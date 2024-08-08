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
    public class WldLightInstance : WldFragment
    {
        public WldLightSourceReference LightReference { get; private set; }

        /// <summary>
        /// The position of the light
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// The radius of the light
        /// </summary>
        public float Radius { get; private set; }

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            LightReference = wld.GetFragment(Reader.ReadInt32()) as WldLightSourceReference;
            int flags = Reader.ReadInt32();
            Position = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
            Radius = Reader.ReadSingle();
        }
    }
}