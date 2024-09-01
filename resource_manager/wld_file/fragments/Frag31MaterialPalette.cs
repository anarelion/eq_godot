using System;
using System.Collections.Generic;
using EQGodot.resource_manager.wld_file.data_types;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public class Frag31MaterialPalette : WldFragment
{
    public List<Frag30MaterialDef> Materials { get; private set; }

    /// <summary>
    ///     A mapping of slot names to alternate skins
    /// </summary>
    public Dictionary<string, Dictionary<int, Frag30MaterialDef>> Slots { get; private set; }

    /// <summary>
    ///     The number of alternate skins
    /// </summary>
    public int VariantCount { get; set; }

    public List<Frag30MaterialDef> AdditionalMaterials { get; set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());

        Materials = [];

        var flags = Reader.ReadInt32();
        var materialCount = Reader.ReadInt32();

        for (var i = 0; i < materialCount; ++i)
        {
            var material = wld.GetFragment(Reader.ReadInt32()) as Frag30MaterialDef;

            if (material == null)
            {
                GD.PrintErr("Unable to get material reference for fragment id");
                continue;
            }

            Materials.Add(material);

            // Materials that are referenced in the MaterialList are already handled
            material.IsHandled = true;
        }
    }

    public void BuildSlotMapping()
    {
        Slots = [];

        if (Materials == null || Materials.Count == 0) return;

        foreach (var material in Materials)
        {
            var character = string.Empty;
            var skinId = string.Empty;
            var partName = string.Empty;

            ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

            var key = character + "_" + partName;
            Slots[key] = [];
        }

        AdditionalMaterials = [];
    }

    private static void ParseCharacterSkin(string materialName, out string character, out string skinId,
        out string partName)
    {
        if (materialName.Length != 9)
        {
            character = string.Empty;
            skinId = string.Empty;
            partName = string.Empty;
            return;
        }

        character = materialName.Substring(0, 3);
        skinId = materialName.Substring(5, 2);
        partName = materialName.Substring(3, 2) + materialName.Substring(7, 2);
    }

    public static string GetMaterialPrefix(ShaderType shaderType)
    {
        switch (shaderType)
        {
            case ShaderType.Diffuse:
                return "d_";
            case ShaderType.Invisible:
                return "i_";
            case ShaderType.Boundary:
                return "b_";
            case ShaderType.Transparent25:
                return "t25_";
            case ShaderType.Transparent50:
                return "t50_";
            case ShaderType.Transparent75:
                return "t75_";
            case ShaderType.TransparentAdditive:
                return "ta_";
            case ShaderType.TransparentAdditiveUnlit:
                return "tau_";
            case ShaderType.TransparentMasked:
                return "tm_";
            case ShaderType.DiffuseSkydome:
                return "ds_";
            case ShaderType.TransparentSkydome:
                return "ts_";
            case ShaderType.TransparentAdditiveUnlitSkydome:
                return "taus_";
            default:
                return "d_";
        }
    }

    public void AddVariant(Frag30MaterialDef material)
    {
        var character = string.Empty;
        var skinId = string.Empty;
        var partName = string.Empty;
        ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

        var key = character + "_" + partName;

        if (!Slots.ContainsKey(key)) Slots[key] = [];

        var skinIdNumber = Convert.ToInt32(skinId);
        Slots[key][skinIdNumber] = material;

        if (skinIdNumber > VariantCount) VariantCount = skinIdNumber;

        AdditionalMaterials.Add(material);
    }

    public List<Frag30MaterialDef> GetMaterialVariants(Frag30MaterialDef material)
    {
        List<Frag30MaterialDef> additionalSkins = [];

        if (Slots == null) return additionalSkins;

        var character = string.Empty;
        var skinId = string.Empty;
        var partName = string.Empty;
        ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

        var key = character + "_" + partName;

        if (!Slots.ContainsKey(key)) return additionalSkins;

        var variants = Slots[key];
        for (var i = 0; i < VariantCount; ++i)
        {
            if (!variants.ContainsKey(i + 1))
            {
                additionalSkins.Add(null);
                continue;
            }

            additionalSkins.Add(variants[i + 1]);
        }

        return additionalSkins;
    }
}