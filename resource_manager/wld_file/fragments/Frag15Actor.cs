using Godot;
using EQGodot.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EQGodot.resource_manager.wld_file.fragments;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public class Frag15Actor : WldFragment
    {
        /// <summary>
        /// The name of the object model
        /// </summary>
        public string ObjectName { get; private set; }

        public string Unknown2;

        /// <summary>
        /// The instance position in the world
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// The instance rotation in the world
        /// </summary>
        public Vector3 Rotation { get; private set; }

        /// <summary>
        /// The instance scale in the world
        /// </summary>
        public Vector3 Scale { get; private set; }

        /// <summary>
        /// The vertex colors lighting data for this instance
        /// </summary>
        public Frag32DmRGBTrackDef Colors;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());

            // in main zone, points to 0x16, in object wld, it contains the object name
            ObjectName = wld.GetName(Reader.ReadInt32());

            // Main zone: 0x2E, Objects: 0x32E
            int flags = Reader.ReadInt32();

            // Fragment reference
            // In main zone, it points to a 0x16 fragment
            // In objects.wld, it is 0
            Unknown2 = wld.GetName(Reader.ReadInt32());

            Position = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());

            // Rotation is strange. There is never any x rotation (roll)
            // The z rotation is negated
            float value0 = Reader.ReadSingle();
            float value1 = Reader.ReadSingle();
            float value2 = Reader.ReadSingle();

            float modifier = 1.0f / 512.0f * 360.0f;

            Rotation = new Vector3(0f, value1 * modifier, -(value0 * modifier));

            // Only scale y is used
            float scaleX, scaleY, scaleZ;
            scaleX = Reader.ReadSingle();
            scaleY = Reader.ReadSingle();
            scaleZ = Reader.ReadSingle();

            Scale = new Vector3(scaleX, scaleY, scaleZ);

            int colorFragment = Reader.ReadInt32();

            if (colorFragment != 0)
            {
                Colors = (wld.GetFragment(colorFragment) as Frag33DmRGBTrack)?.VertexColors;
            }
        }
    }
}
