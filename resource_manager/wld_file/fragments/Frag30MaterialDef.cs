﻿using System.Collections.Generic;
using System.Linq;
using EQGodot.resource_manager.pack_file;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
[GlobalClass]
public partial class Frag30MaterialDef : WldFragment
{
    [Export] public Frag05SimpleSprite SimpleSprite;
    [Export] public ShaderType ShaderType;
    [Export] public float Brightness;
    [Export] public float ScaledAmbient;
    [Export] public bool IsHandled;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        var flags = Reader.ReadInt32();
        var parameters = Reader.ReadInt32();

        // Unsure what this color is used for
        // Referred to as the RGB pen
        var colorR = Reader.ReadByte();
        var colorG = Reader.ReadByte();
        var colorB = Reader.ReadByte();
        var colorA = Reader.ReadByte();

        Brightness = Reader.ReadSingle();
        ScaledAmbient = Reader.ReadSingle();
        SimpleSprite = wld.GetFragment(Reader.ReadInt32()) as Frag05SimpleSprite;

        // Thanks to PixelBound for figuring this out
        var materialType = (MaterialType)(parameters & ~0x80000000);

        switch (materialType)
        {
            case MaterialType.Boundary:
                ShaderType = ShaderType.Boundary;
                break;
            case MaterialType.InvisibleUnknown:
            case MaterialType.InvisibleUnknown2:
            case MaterialType.InvisibleUnknown3:
                ShaderType = ShaderType.Invisible;
                break;
            case MaterialType.Diffuse:
            case MaterialType.Diffuse3:
            case MaterialType.Diffuse4:
            case MaterialType.Diffuse6:
            case MaterialType.Diffuse7:
            case MaterialType.Diffuse8:
            case MaterialType.Diffuse2:
            case MaterialType.CompleteUnknown:
            case MaterialType.TransparentMaskedPassable:
                ShaderType = ShaderType.Diffuse;
                break;
            case MaterialType.Transparent25:
                ShaderType = ShaderType.Transparent25;
                break;
            case MaterialType.Transparent50:
                ShaderType = ShaderType.Transparent50;
                break;
            case MaterialType.Transparent75:
                ShaderType = ShaderType.Transparent75;
                break;
            case MaterialType.TransparentAdditive:
                ShaderType = ShaderType.TransparentAdditive;
                break;
            case MaterialType.TransparentAdditiveUnlit:
                ShaderType = ShaderType.TransparentAdditiveUnlit;
                break;
            case MaterialType.TransparentMasked:
            case MaterialType.Diffuse5:
                ShaderType = ShaderType.TransparentMasked;
                break;
            case MaterialType.DiffuseSkydome:
                ShaderType = ShaderType.DiffuseSkydome;
                break;
            case MaterialType.TransparentSkydome:
                ShaderType = ShaderType.TransparentSkydome;
                break;
            case MaterialType.TransparentAdditiveUnlitSkydome:
                ShaderType = ShaderType.TransparentAdditiveUnlitSkydome;
                break;
            default:
                ShaderType = SimpleSprite == null ? ShaderType.Invisible : ShaderType.Diffuse;
                break;
        }
    }

    public Material ToGodotMaterial(PFSArchive archive)
    {
        if (SimpleSprite != null)
        {
            var names = SimpleSprite.GetAllBitmapNames();
            if (SimpleSprite.SimpleSpriteDef.Animated)
            {
                Godot.Collections.Array<Image> a = [];
                foreach (var image in names.Select(name => (archive.FilesByName[name] as Image)))
                {
                    a.Add(image);
                }

                var texture2DArray = new Texture2DArray();
                texture2DArray.CreateFromImages(a);

                var material = new ShaderMaterial()
                {
                    ResourceName = Name,
                    Shader = ResourceLoader.Load<Shader>("res://shaders/animated_texture.gdshader"),
                };
                material.SetShaderParameter("textures", texture2DArray);
                material.SetShaderParameter("step_time", SimpleSprite.SimpleSpriteDef.AnimationDelayMs);
                material.SetShaderParameter("total_time",
                    SimpleSprite.SimpleSpriteDef.AnimationDelayMs * names.Count);

                return material;
            }

            return new StandardMaterial3D()
            {
                ResourceName = Name,
                AlbedoTexture = ImageTexture.CreateFromImage((Image)archive.FilesByName[names[0]]),
            };
        }

        GD.PrintErr($"Material: {Name} doesn't have a texture");
        return new StandardMaterial3D()
        {
            ResourceName = Name
        };
    }
}