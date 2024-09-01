using EQGodot.helpers;
using EQGodot.resource_manager.pack_file;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public class Frag30MaterialDef : WldFragment
{
    public Frag05SimpleSprite SimpleSprite { get; private set; }

    /// <summary>
    ///     The shader type that this material uses when rendering
    /// </summary>
    public ShaderType ShaderType { get; set; }

    public float Brightness { get; set; }
    public float ScaledAmbient { get; set; }

    /// <summary>
    ///     If a material has not been handled, we still need to find the corresponding material list
    ///     Used for alternate character skins
    /// </summary>
    public bool IsHandled { get; set; }

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
        var result = new StandardMaterial3D();
        if (SimpleSprite != null)
            result.AlbedoTexture = SimpleSprite.ToGodotTexture(archive);
        else
            GD.PrintErr($"Material: {Name} doesn't have a texture");
        return result;
    }
}