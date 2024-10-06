using System.IO;
using Godot;

namespace EQGodot.resource_manager.wld_file;

// Latern Extractor class
[GlobalClass]
public abstract partial class WldFragment : Resource
{ 
    [Export] public int Index;
    [Export] public int Type;
    [Export] public int Size;
    [Export] public string Name;

    protected BinaryReader Reader { get; set; }

    public virtual void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        Index = index;
        Size = size;
        Type = type;
        Reader = new BinaryReader(new MemoryStream(data));
    }
}