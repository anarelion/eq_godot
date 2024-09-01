using System.Collections.Generic;
using EQGodot.resource_manager.wld_file.helpers;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public class Frag04SimpleSpriteDef : WldFragment
{
    /// <summary>
    ///     Is the texture animated?
    /// </summary>
    public bool IsAnimated { get; private set; }

    /// <summary>
    ///     The bitmap names referenced.
    /// </summary>
    public List<Frag03BMInfo> BitmapNames { get; private set; }

    /// <summary>
    ///     The number of milliseconds before the next texture is swapped.
    /// </summary>
    public int AnimationDelayMs { get; private set; }

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