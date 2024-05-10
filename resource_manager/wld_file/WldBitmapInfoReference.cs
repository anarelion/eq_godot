using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public class WldBitmapInfoReference : WldFragment {
        /// <summary>
        /// The reference to the BitmapInfo
        /// </summary>
        public WldBitmapInfo BitmapInfo {
            get; private set;
        }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];
            BitmapInfo = fragments[Reader.ReadInt32() - 1] as WldBitmapInfo;

            // Either 0 or 80 - unknown
            int flags = Reader.ReadInt32();
        }

        public override void OutputInfo()
        {
            base.OutputInfo();
            GD.Print("-----");
            GD.Print("BitmapInfoReference: Reference: " + (BitmapInfo.Index + 1));
        }
    }
}
