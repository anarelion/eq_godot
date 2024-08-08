using Godot;
using EQGodot.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public class WldBitmapInfo : WldFragment
    {
        /// <summary>
        /// Is the texture animated?
        /// </summary>
        public bool IsAnimated
        {
            get; private set;
        }

        /// <summary>
        /// The bitmap names referenced. 
        /// </summary>
        public List<WldBitmapName> BitmapNames
        {
            get; private set;
        }

        /// <summary>
        /// The number of milliseconds before the next texture is swapped.
        /// </summary>
        public int AnimationDelayMs
        {
            get; private set;
        }

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            int flags = Reader.ReadInt32();
            var bitAnalyzer = new BitAnalyzer(flags);
            IsAnimated = bitAnalyzer.IsBitSet(3);
            int bitmapCount = Reader.ReadInt32();

            BitmapNames = [];

            if (IsAnimated)
            {
                AnimationDelayMs = Reader.ReadInt32();
            }

            for (int i = 0; i < bitmapCount; ++i)
            {
                BitmapNames.Add(wld.GetFragment(Reader.ReadInt32()) as WldBitmapName);
            }
        }
    }
}
