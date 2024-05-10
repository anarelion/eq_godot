using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public abstract class WldFragment {
        public int Index {
            get; private set;
        }

        public int Size {
            get; private set;
        }

        public string Name {
            get; set;
        }

        public BinaryReader Reader {
            get; set;
        }

        public virtual void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            Index = index;
            Size = size;
            Reader = new BinaryReader(new MemoryStream(data));
        }

        public virtual void OutputInfo()
        {
            GD.Print("-----------------------------------");
            GD.Print("Fragment " + (Index + 1) + ": " + this.GetType().Name);
            GD.Print("-----");
            GD.Print("Size: " + Size + " bytes");
            GD.Print("Name: " + (string.IsNullOrEmpty(Name) ? "(empty)" : Name));
        }
    }
}
