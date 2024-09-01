using System.Collections.Generic;
using EQGodot.resource_manager.pack_file;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public class Frag05SimpleSprite : WldFragment
{
    public int Flags;

    /// <summary>
    ///     The reference to the BitmapInfo
    /// </summary>
    public Frag04SimpleSpriteDef SimpleSpriteDef { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        SimpleSpriteDef = wld.GetFragment(Reader.ReadInt32()) as Frag04SimpleSpriteDef;

        // Either 0 or 80 - unknown
        Flags = Reader.ReadInt32();
    }

    public Texture2D ToGodotTexture(PFSArchive archive)
    {
        var names = GetAllBitmapNames();
        if (names.Count > 1)
        {
            var animated = new AnimatedTexture { Frames = names.Count };

            for (var i = 0; i < names.Count; i++)
            {
                var image = archive.FilesByName[names[i]] as ImageTexture;
                animated.SetFrameTexture(i, image);
                animated.SetFrameDuration(i, SimpleSpriteDef.AnimationDelayMs / 1000.0f);
            }

            return animated;
        }

        return archive.FilesByName[names[0]] as ImageTexture;
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