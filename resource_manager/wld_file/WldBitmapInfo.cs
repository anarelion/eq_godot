using Godot;
using EQGodot2.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public class WldBitmapInfo : WldFragment {
        /// <summary>
        /// Is the texture animated?
        /// </summary>
        public bool IsAnimated {
            get; private set;
        }

        /// <summary>
        /// The bitmap names referenced. 
        /// </summary>
        public List<WldBitmapName> BitmapNames {
            get; private set;
        }

        /// <summary>
        /// The number of milliseconds before the next texture is swapped.
        /// </summary>
        public int AnimationDelayMs {
            get; private set;
        }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];
            int flags = Reader.ReadInt32();
            var bitAnalyzer = new BitAnalyzer(flags);
            IsAnimated = bitAnalyzer.IsBitSet(3);
            int bitmapCount = Reader.ReadInt32();

            BitmapNames = new List<WldBitmapName>();

            if (IsAnimated) {
                AnimationDelayMs = Reader.ReadInt32();
            }

            for (int i = 0; i < bitmapCount; ++i) {
                BitmapNames.Add(fragments[Reader.ReadInt32() - 1] as WldBitmapName);
            }
        }

        public override void OutputInfo()
        {
            base.OutputInfo();
            GD.Print("-----");
            GD.Print("BitmapInfo: Animated: " + IsAnimated);

            if (IsAnimated) {
                GD.Print("BitmapInfo: Animation delay: " + AnimationDelayMs + "ms");
            }

            string references = string.Empty;

            for (var i = 0; i < BitmapNames.Count; i++) {
                if (i != 0) {
                    references += ", ";
                }

                WldBitmapName bitmapName = BitmapNames[i];
                references += bitmapName.Index + 1;
            }

            GD.Print("BitmapInfo: Reference(s): " + references);
        }
    }
}
