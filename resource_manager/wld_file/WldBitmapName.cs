using EQGodot2.helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public class WldBitmapName : WldFragment {

        public string Filename {
            get; set;
        }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];

            // The client supports more than one bitmap reference but is never used
            int bitmapCount = Reader.ReadInt32();

            if (bitmapCount > 1) {
                GD.PrintErr("BitmapName: Bitmap count exceeds 1.");
            }

            int nameLength = Reader.ReadInt16();

            // Decode the bitmap name and trim the null character (c style strings)
            byte[] nameBytes = Reader.ReadBytes(nameLength);
            Filename = WldStringDecoder.Decode(nameBytes);
            Filename = Filename.ToLower().Substring(0, Filename.Length - 1);
        }

        public override void OutputInfo()
        {
            base.OutputInfo();
            GD.Print("-----");
            GD.Print("BitmapName: Filename: " + Filename);
        }
    }
}

