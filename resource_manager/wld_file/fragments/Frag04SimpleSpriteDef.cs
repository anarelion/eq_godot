using System.Collections.Generic;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
[GlobalClass]
public partial class Frag04SimpleSpriteDef : WldFragment
{
    [Export] public bool IsAnimated;
    [Export] public int AnimationDelayMs;
    [Export] public Godot.Collections.Array<Frag03BMInfo> BitmapNames;
    

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        var flags = Reader.ReadInt32();
        var bitAnalyzer = new BitAnalyzer(flags);
        IsAnimated = bitAnalyzer.IsBitSet(3);
        var bitmapCount = Reader.ReadInt32();

        BitmapNames = [];

        if (IsAnimated) AnimationDelayMs = Reader.ReadInt32();

        for (var i = 0; i < bitmapCount; ++i) BitmapNames.Add(wld.GetFragment(Reader.ReadInt32()) as Frag03BMInfo);
    }
}