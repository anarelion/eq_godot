using EQGodot.helpers;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public class WldTrackDefFragment : WldFragment
    {
        /// <summary>
        /// A list of bone positions for each frame
        /// </summary>
        public List<BoneTransform> Frames
        {
            get; set;
        }

        public bool IsAssigned;
        public int Flags;

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);

            Reader = new BinaryReader(new MemoryStream(data));
            Name = wld.GetName(Reader.ReadInt32());

            int Flags = Reader.ReadInt32();

            // Flags are always 8 when dealing with object animations
            if (Flags != 8)
            {

            }

            BitAnalyzer bitAnalyzer = new(Flags);

            bool isS3dTrack2 = bitAnalyzer.IsBitSet(3);

            int frameCount = Reader.ReadInt32();

            Frames = [];

            if (isS3dTrack2)
            {
                for (int i = 0; i < frameCount; ++i)
                {
                    Int16 rotW = Reader.ReadInt16();
                    Int16 rotX = Reader.ReadInt16();
                    Int16 rotY = Reader.ReadInt16();
                    Int16 rotZ = Reader.ReadInt16();
                    Int16 shiftX = Reader.ReadInt16();
                    Int16 shiftY = Reader.ReadInt16();
                    Int16 shiftZ = Reader.ReadInt16();
                    Int16 shiftDenominator = Reader.ReadInt16();

                    BoneTransform frameTransform = new BoneTransform();

                    if (shiftDenominator != 0)
                    {
                        float x = shiftX / 256f;
                        float y = shiftY / 256f;
                        float z = shiftZ / 256f;

                        frameTransform.Scale = shiftDenominator / 256f;
                        frameTransform.Translation = new Vector3(x, y, z);
                    }
                    else
                    {
                        frameTransform.Translation = Vector3.Zero;
                    }

                    frameTransform.Rotation = new Quaternion(rotX, rotY, rotZ, rotW).Normalized();
                    Frames.Add(frameTransform);
                }
            }
            else
            {
                for (int i = 0; i < frameCount; ++i)
                {
                    var shiftDenominator = Reader.ReadSingle();
                    var shiftX = Reader.ReadSingle();
                    var shiftY = Reader.ReadSingle();
                    var shiftZ = Reader.ReadSingle();
                    var rotW = Reader.ReadSingle();
                    var rotX = Reader.ReadSingle();
                    var rotY = Reader.ReadSingle();
                    var rotZ = Reader.ReadSingle();

                    var frameTransform = new BoneTransform()
                    {
                        Scale = shiftDenominator,
                        Translation = new Vector3(shiftX, shiftY, shiftZ),
                        Rotation = new Quaternion(rotX, rotY, rotZ, rotW).Normalized(),
                    };

                    Frames.Add(frameTransform);
                }
            }
        }
    }
}
