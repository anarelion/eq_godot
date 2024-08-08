using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public class WldBitmapInfoReference : WldFragment
    {
        /// <summary>
        /// The reference to the BitmapInfo
        /// </summary>
        public WldBitmapInfo BitmapInfo
        {
            get; private set;
        }

        public int Flags;

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            BitmapInfo = wld.GetFragment(Reader.ReadInt32()) as WldBitmapInfo;

            // Either 0 or 80 - unknown
            Flags = Reader.ReadInt32();
        }
    }
}
