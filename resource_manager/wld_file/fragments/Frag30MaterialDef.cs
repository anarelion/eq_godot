﻿using System.Diagnostics;
using System.Linq;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
[GlobalClass]
public partial class Frag30MaterialDef : WldFragment
{
    [Export] public int Flags;
    [Export] public Frag05SimpleSprite SimpleSprite;
    [Export] public uint RenderMethod;
    [Export] public ShaderTypeEnumType ShaderType;
    [Export] public float Brightness;
    [Export] public float ScaledAmbient;
    [Export] public bool IsHandled;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        Flags = Reader.ReadInt32();
        RenderMethod = Reader.ReadUInt32();

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
        var materialType = (MaterialType)(RenderMethod & ~0x80000000);

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

    public Material ToGodotMaterial(EqResourceLoader loader)
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

            if (SimpleSprite.SimpleSpriteDef.Animated)
            {
                Godot.Collections.Array<Image> a = [];
                foreach (var image in bitmapNames.Select(loader.GetImage))
                {
                    a.Add(ApplyBmpTransparency(image));
                }

                var texture2DArray = new Texture2DArray();
                texture2DArray.CreateFromImages(ExpandTextureArray(a));

                var code = "shader_type spatial;\n\n";

                if (ShaderType is ShaderTypeEnumType.TransparentAdditive)
                {
                    code += "render_mode blend_add;\n";
                }

                code += @"
                    uniform sampler2DArray textures;
                    uniform int step_time;
                    uniform int total_time;

                    void fragment() {
	                    int texture_number = (int(TIME * 1000.0) % total_time) / step_time;
	                    vec4 texture_color = texture(textures, vec3(UV, float(texture_number)));
	                    ALBEDO.rgb = texture_color.rgb;
                    }
                ";

                var shader = new Shader()
                {
                    Code = code,
                };

                var animatedMaterial = new ShaderMaterial()
                {
                    ResourceName = Name,
                    Shader = shader,
                };
                animatedMaterial.SetShaderParameter("textures", texture2DArray);
                animatedMaterial.SetShaderParameter("step_time", SimpleSprite.SimpleSpriteDef.AnimationDelayMs);
                animatedMaterial.SetShaderParameter("total_time",
                    SimpleSprite.SimpleSpriteDef.AnimationDelayMs * bitmapNames.Count);
                animatedMaterial.SetShaderParameter("render_method", RenderMethod);
                animatedMaterial.SetMeta("render_method", $"0x{RenderMethod:x}");

                return animatedMaterial;
            }

            var firstImage = loader.GetImage(bitmapNames[0]);
            var transparentMasked = new StandardMaterial3D
            {
                ResourceName = Name,
                Transparency = ShouldApplyTransparency()
                    ? BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass
                    : BaseMaterial3D.TransparencyEnum.Disabled,
                BlendMode = ShaderType is ShaderTypeEnumType.TransparentAdditive
                    ? BaseMaterial3D.BlendModeEnum.Add
                    : BaseMaterial3D.BlendModeEnum.Mix,
                AlbedoTexture = ImageToTexture(ApplyBmpTransparency(firstImage)),
                CullMode = (Flags & 0x1) != 0
                    ? BaseMaterial3D.CullModeEnum.Disabled
                    : BaseMaterial3D.CullModeEnum.Back,
            };
            transparentMasked.SetMeta("pfs_file_name", firstImage.GetMeta("pfs_file_name"));
            transparentMasked.SetMeta("original_file_name", firstImage.GetMeta("original_file_name"));
            transparentMasked.SetMeta("original_file_type", firstImage.GetMeta("original_file_type"));
            transparentMasked.SetMeta("render_method", $"0x{RenderMethod:x}");
            return transparentMasked;
        }

        GD.PrintErr($"Material: {Name} doesn't have a texture");
        return new StandardMaterial3D()
        {
            ResourceName = Name
        };
    }

    private bool ShouldApplyTransparency()
    {
        return ShaderType is ShaderTypeEnumType.TransparentMasked or ShaderTypeEnumType.TransparentAdditive;
    }

    private Image ApplyBmpTransparency(Image image)
    {
        if (!ShouldApplyTransparency() ||
            (image.HasMeta("palette_present") && (bool)image.GetMeta("palette_present") == false))
            return image;

        if ((string)image.GetMeta("original_file_type") != "BMP") return image;
        var a = (int)image.GetMeta("transparent_a");
        var r = (int)image.GetMeta("transparent_r");
        var g = (int)image.GetMeta("transparent_g");
        var b = (int)image.GetMeta("transparent_b");

        var data = image.GetData();
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i] != r || data[i + 1] != g || data[i + 2] != b || data[i + 3] != a) continue;
            data[i + 0] = 0;
            data[i + 1] = 0;
            data[i + 2] = 0;
            data[i + 3] = 0;
        }

        var result = Image.CreateFromData(image.GetWidth(), image.GetHeight(), false, Image.Format.Rgba8, data);
        result.ResourceName = image.ResourceName;
        foreach (var metaName in image.GetMetaList())
        {
            result.SetMeta(metaName, image.GetMeta(metaName));
        }

        return result;
    }

    private static ImageTexture ImageToTexture(Image image)
    {
        var texture = ImageTexture.CreateFromImage(image);
        texture.ResourceName = image.ResourceName;
        foreach (var metaName in image.GetMetaList())
        {
            texture.SetMeta(metaName, image.GetMeta(metaName));
        }

        return texture;
    }

    private static Godot.Collections.Array<Image> ExpandTextureArray(Godot.Collections.Array<Image> list)
    {
        var maxWidth = 0;
        var maxHeight = 0;
        foreach (var image in list)
        {
            if (image.GetHeight() > maxHeight) maxHeight = image.GetHeight();
            if (image.GetWidth() > maxWidth) maxWidth = image.GetWidth();
        }

        foreach (var image in list)
        {
            var originalWidth = image.GetWidth();
            if (image.GetWidth() < maxWidth)
            {
                image.Crop(maxWidth, image.GetHeight());
                for (var i = 1;
                     i < (maxWidth / originalWidth) + 1;
                     i++)
                {
                    image.BlitRect(image, new Rect2I(0, 0, originalWidth, image.GetHeight()),
                        new Vector2I(i * originalWidth, 0));
                }
            }

            var originalHeight = image.GetHeight();
            if (image.GetHeight() >= maxHeight) continue;
            {
                image.Crop(image.GetWidth(), maxHeight);
                for (var i = 1;
                     i < (maxHeight / originalHeight) + 1;
                     i++)
                {
                    image.BlitRect(image, new Rect2I(0, 0, image.GetWidth(), originalHeight),
                        new Vector2I(0, i * originalHeight));
                }
            }
        }

        return list;
    }
}