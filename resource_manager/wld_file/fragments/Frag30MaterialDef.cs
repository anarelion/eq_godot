using System.Collections.Generic;
using System.Linq;
using EQGodot.resource_manager.pack_file;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
[GlobalClass]
public partial class Frag30MaterialDef : WldFragment
{
    [Export] public int Flags;
    [Export] public Frag05SimpleSprite SimpleSprite;
    [Export] public ShaderTypeEnumType ShaderType;
    [Export] public float Brightness;
    [Export] public float ScaledAmbient;
    [Export] public bool IsHandled;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        Flags = Reader.ReadInt32();
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
                ShaderType = ShaderTypeEnumType.Boundary;
                break;
            case MaterialType.InvisibleUnknown:
            case MaterialType.InvisibleUnknown2:
            case MaterialType.InvisibleUnknown3:
                ShaderType = ShaderTypeEnumType.Invisible;
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
                ShaderType = ShaderTypeEnumType.Diffuse;
                break;
            case MaterialType.Transparent25:
                ShaderType = ShaderTypeEnumType.Transparent25;
                break;
            case MaterialType.Transparent50:
                ShaderType = ShaderTypeEnumType.Transparent50;
                break;
            case MaterialType.Transparent75:
                ShaderType = ShaderTypeEnumType.Transparent75;
                break;
            case MaterialType.TransparentAdditive:
                ShaderType = ShaderTypeEnumType.TransparentAdditive;
                break;
            case MaterialType.TransparentAdditiveUnlit:
                ShaderType = ShaderTypeEnumType.TransparentAdditiveUnlit;
                break;
            case MaterialType.TransparentMasked:
            case MaterialType.Diffuse5:
                ShaderType = ShaderTypeEnumType.TransparentMasked;
                break;
            case MaterialType.DiffuseSkydome:
                ShaderType = ShaderTypeEnumType.DiffuseSkydome;
                break;
            case MaterialType.TransparentSkydome:
                ShaderType = ShaderTypeEnumType.TransparentSkydome;
                break;
            case MaterialType.TransparentAdditiveUnlitSkydome:
                ShaderType = ShaderTypeEnumType.TransparentAdditiveUnlitSkydome;
                break;
            default:
                ShaderType = SimpleSprite == null ? ShaderTypeEnumType.Invisible : ShaderTypeEnumType.Diffuse;
                break;
        }
    }

    public Material ToGodotMaterial(PfsArchive archive)
    {
        if (ShaderType is ShaderTypeEnumType.Boundary or ShaderTypeEnumType.Invisible)
        {
            return new StandardMaterial3D
            {
                ResourceName = Name,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                AlbedoColor = new Color(1, 1, 1, 0),
            };
        }

        if (SimpleSprite != null)
        {
            var bitmapNames = SimpleSprite.GetAllBitmapNames();
            var firstImage = (Image)archive.FilesByName[bitmapNames[0]];
            if (ShaderType is ShaderTypeEnumType.TransparentMasked)
            {
                var transparentMasked = new StandardMaterial3D
                {
                    ResourceName = Name,
                    Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass,
                    AlbedoTexture = ImageTexture.CreateFromImage(firstImage),
                    CullMode = (Flags & 0x1) != 0
                        ? BaseMaterial3D.CullModeEnum.Disabled
                        : BaseMaterial3D.CullModeEnum.Back,
                };
                transparentMasked.SetMeta("pfs_file_name", firstImage.GetMeta("pfs_file_name"));
                transparentMasked.SetMeta("original_file_name", firstImage.GetMeta("original_file_name"));
                transparentMasked.SetMeta("original_file_type", firstImage.GetMeta("original_file_type"));
                return transparentMasked;
            }

            if (SimpleSprite.SimpleSpriteDef.Animated)
            {
                Godot.Collections.Array<Image> a = [];
                foreach (var image in bitmapNames.Select(name => (archive.FilesByName[name] as Image)))
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
                    SimpleSprite.SimpleSpriteDef.AnimationDelayMs * bitmapNames.Count);

                return material;
            }

            var fallbackMaterial = new StandardMaterial3D()
            {
                ResourceName = Name,
                AlbedoTexture = ImageTexture.CreateFromImage(firstImage),
                CullMode = (Flags & 0x1) != 0 ? BaseMaterial3D.CullModeEnum.Disabled : BaseMaterial3D.CullModeEnum.Back,
            };
            fallbackMaterial.SetMeta("ShaderType", ShaderType.ToString());
            fallbackMaterial.SetMeta("pfs_file_name", firstImage.GetMeta("pfs_file_name"));
            fallbackMaterial.SetMeta("original_file_name", firstImage.GetMeta("original_file_name"));
            fallbackMaterial.SetMeta("original_file_type", firstImage.GetMeta("original_file_type"));
            return fallbackMaterial;
        }

        GD.PrintErr($"Material: {Name} doesn't have a texture");
        return new StandardMaterial3D()
        {
            ResourceName = Name
        };
    }
}