using EQGodot2.helpers;
using EQGodot2.resource_manager.wld_file.data_types;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public class WldMaterialList : WldFragment {
        public List<WldMaterial> Materials {
            get; private set;
        }

        /// <summary>
        /// A mapping of slot names to alternate skins
        /// </summary>
        public Dictionary<string, Dictionary<int, WldMaterial>> Slots {
            get; private set;
        }

        /// <summary>
        /// The number of alternate skins
        /// </summary>
        public int VariantCount {
            get; set;
        }

        public List<WldMaterial> AdditionalMaterials {
            get; set;
        }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];

            Materials = new List<WldMaterial>();

            int flags = Reader.ReadInt32();
            int materialCount = Reader.ReadInt32();

            for (int i = 0; i < materialCount; ++i) {
                int reference = Reader.ReadInt32() - 1;
                WldMaterial material = fragments[reference] as WldMaterial;

                if (material == null) {
                    GD.PrintErr("Unable to get material reference for fragment id: " + reference);
                    continue;
                }

                Materials.Add(material);

                // Materials that are referenced in the MaterialList are already handled
                material.IsHandled = true;
            }
        }

        public void BuildSlotMapping()
        {
            Slots = new Dictionary<string, Dictionary<int, WldMaterial>>();

            if (Materials == null || Materials.Count == 0) {
                return;
            }

            foreach (var material in Materials) {
                string character = string.Empty;
                string skinId = string.Empty;
                string partName = string.Empty;

                ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

                string key = character + "_" + partName;
                Slots[key] = new Dictionary<int, WldMaterial>();
            }

            AdditionalMaterials = new List<WldMaterial>();
        }

        public override void OutputInfo()
        {
            base.OutputInfo();
            GD.Print("-----");
            GD.Print("0x30: Material count: " + Materials.Count);

            string references = string.Empty;

            for (var i = 0; i < Materials.Count; i++) {
                if (i != 0) {
                    references += ", ";
                }

                references += (Materials[i].Index + 1);
            }

            GD.Print("0x30: References: " + references);
        }

        private static void ParseCharacterSkin(string materialName, out string character, out string skinId,
            out string partName)
        {
            if (materialName.Length != 9) {
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
            switch (shaderType) {
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

        public void AddVariant(WldMaterial material)
        {
            string character = string.Empty;
            string skinId = string.Empty;
            string partName = string.Empty;
            ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

            string key = character + "_" + partName;

            if (!Slots.ContainsKey(key)) {
                Slots[key] = new Dictionary<int, WldMaterial>();
            }

            int skinIdNumber = Convert.ToInt32(skinId);
            Slots[key][skinIdNumber] = material;

            if (skinIdNumber > VariantCount) {
                VariantCount = skinIdNumber;
            }

            AdditionalMaterials.Add(material);
        }

        public List<WldMaterial> GetMaterialVariants(WldMaterial material)
        {
            List<WldMaterial> additionalSkins = new List<WldMaterial>();

            if (Slots == null) {
                return additionalSkins;
            }

            string character = string.Empty;
            string skinId = string.Empty;
            string partName = string.Empty;
            ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

            string key = character + "_" + partName;

            if (!Slots.ContainsKey(key)) {
                return additionalSkins;
            }

            var variants = Slots[key];
            for (int i = 0; i < VariantCount; ++i) {
                if (!variants.ContainsKey(i + 1)) {
                    additionalSkins.Add(null);
                    continue;
                }

                additionalSkins.Add(variants[i + 1]);
            }

            return additionalSkins;
        }
    }
}
