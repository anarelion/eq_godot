using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EQGodot2.helpers;
using EQGodot2.resource_manager.pack_file;
using EQGodot2.resource_manager.wld_file.data_types;
using Godot;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public class WldMaterial : WldFragment {
        public WldBitmapInfoReference BitmapInfoReference {
            get; private set;
        }

        /// <summary>
        /// The shader type that this material uses when rendering
        /// </summary>
        public ShaderType ShaderType {
            get; set;
        }

        public float Brightness {
            get; set;
        }
        public float ScaledAmbient {
            get; set;
        }

        /// <summary>
        /// If a material has not been handled, we still need to find the corresponding material list
        /// Used for alternate character skins
        /// </summary>
        public bool IsHandled {
            get; set;
        }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];
            int flags = Reader.ReadInt32();
            int parameters = Reader.ReadInt32();

            // Unsure what this color is used for
            // Referred to as the RGB pen
            byte colorR = Reader.ReadByte();
            byte colorG = Reader.ReadByte();
            byte colorB = Reader.ReadByte();
            byte colorA = Reader.ReadByte();

            Brightness = Reader.ReadSingle();
            ScaledAmbient = Reader.ReadSingle();

            int fragmentReference = Reader.ReadInt32();

            if (fragmentReference != 0) {
                BitmapInfoReference = fragments[fragmentReference - 1] as WldBitmapInfoReference;
            }

            // Thanks to PixelBound for figuring this out
            MaterialType materialType = (MaterialType)(parameters & ~0x80000000);

            switch (materialType) {
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
                    ShaderType = BitmapInfoReference == null ? ShaderType.Invisible : ShaderType.Diffuse;
                    break;
            }
        }

        public override void OutputInfo()
        {
            base.OutputInfo();
            GD.Print("-----");
            GD.Print("Material: Shader type: " + ShaderType);

            if (ShaderType != ShaderType.Invisible && BitmapInfoReference != null) {
                GD.Print("Material: Reference: " + (BitmapInfoReference.Index + 1));
            }
        }

        public List<string> GetAllBitmapNames()
        {
            var bitmapNames = new List<string>();

            if (BitmapInfoReference == null) {
                return bitmapNames;
            }

            foreach (WldBitmapName bitmapName in BitmapInfoReference.BitmapInfo.BitmapNames) {
                string filename = bitmapName.Filename;
                bitmapNames.Add(filename);
            }

            return bitmapNames;
        }

        public Material ToGodotMaterial(PFSArchive archive)
        {
            var names = GetAllBitmapNames();
            if (names.Count > 1) {
                GD.PrintErr("TODO: need to create an animated texture ", Name);
                return null;
            }
            var texture = archive.FilesByName[names[0]] as ImageTexture;
            var result = new StandardMaterial3D();
            result.AlbedoTexture = texture;
            return result;
        }
    }
}
