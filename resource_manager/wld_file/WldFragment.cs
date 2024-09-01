using System.IO;
using Godot;

namespace EQGodot.resource_manager.wld_file;

// Latern Extractor class
public abstract class WldFragment
{
    public int Index { get; private set; }

    public int Type { get; private set; }

    public int Size { get; private set; }

    public string Name { get; set; }

    public BinaryReader Reader { get; set; }

    public virtual void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        Index = index;
        Size = size;
        Type = type;
        Reader = new BinaryReader(new MemoryStream(data));
    }

    public virtual void OutputInfo()
    {
        GD.Print("-----------------------------------");
        GD.Print("Fragment " + (Index + 1) + ": " + GetType().Name);
        GD.Print("-----");
        GD.Print("Size: " + Size + " bytes");
        GD.Print("Name: " + (string.IsNullOrEmpty(Name) ? "(empty)" : Name));
    }
}