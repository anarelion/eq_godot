using System.Collections.Generic;
using EQGodot.resource_manager.pack_file;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
[GlobalClass]
public partial class Frag05SimpleSprite : WldFragment
{
    [Export] public int Flags;
    [Export] public Frag04SimpleSpriteDef SimpleSpriteDef { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        SimpleSpriteDef = wld.GetFragment(Reader.ReadInt32()) as Frag04SimpleSpriteDef;

        // Either 0 or 80 - unknown
        Flags = Reader.ReadInt32();
    }
    
    public List<string> GetAllBitmapNames()
    {
        var bitmapNames = new List<string>();

        foreach (var bitmapName in SimpleSpriteDef.BitmapNames)
        {
            var filename = bitmapName.Filename;
            bitmapNames.Add(filename);
        }

        return bitmapNames;
    }
}