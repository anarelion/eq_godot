using EQGodot.resource_manager.wld_file.data_types;
using Godot;
using System;
using System.Collections.Generic;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class adapted for Godot
    public class WldMesh : WldFragment
    {
        public Vector3 Center
        {
            get; private set;
        }

        /// <summary>
        /// The maximum distance between the center and any vertex - bounding radius
        /// </summary>
        public float MaxDistance
        {
            get; private set;
        }

        /// <summary>
        /// The minimum vertex positions in the model - used for bounding box
        /// </summary>
        public Vector3 MinPosition
        {
            get; private set;
        }

        /// <summary>
        /// The maximum vertex positions in the model - used for bounding box
        /// </summary>
        public Vector3 MaxPosition
        {
            get; private set;
        }

        /// <summary>
        /// The texture list used to render this mesh
        /// In zone meshes, it's always the same one
        /// In object meshes, it can be unique
        /// </summary>
        public WldMaterialList MaterialList
        {
            get; private set;
        }

        /// <summary>
        /// The vertices of the mesh
        /// </summary>
        public Vector3[] Vertices
        {
            get; set;
        }

        /// <summary>
        /// The normals of the mesh
        /// </summary>
        public Vector3[] Normals
        {
            get; private set;
        }

        /// <summary>
        /// The polygon indices of the mesh
        /// </summary>
        public List<Polygon> Indices
        {
            get; private set;
        }

        public Color[] Colors
        {
            get; set;
        }

        /// <summary>
        /// The UV texture coordinates of the vertex
        /// </summary>
        public Vector2[] TextureUvCoordinates
        {
            get; private set;
        }

        /// <summary>
        /// The mesh render groups
        /// Defines which texture index corresponds with groups of vertices
        /// </summary>
        public List<RenderGroup> MaterialGroups
        {
            get; private set;
        }

        /// <summary>
        /// The animated vertex fragment (0x37) reference
        /// </summary>
        // public MeshAnimatedVerticesReference AnimatedVerticesReference {
        //     get; private set;
        // }

        /// <summary>
        /// Set to true if there are non solid polygons in the mesh
        /// This means we export collision separately (e.g. trees, fire)
        /// </summary>
        public bool ExportSeparateCollision
        {
            get; private set;
        }

        public bool IsHandled = false;

        public int StartTextureIndex = 0;

        /// <summary>
        /// The render components of a mob skeleton
        /// </summary>
        public List<MobVertexPiece> MobPieces
        {
            get; private set;
        }

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());

            // Zone: 0x00018003, Objects: 0x00014003
            int flags = Reader.ReadInt32();

            MaterialList = wld.GetFragment(Reader.ReadInt32()) as WldMaterialList;
            int meshAnimation = Reader.ReadInt32();

            // Vertex animation only
            // if (meshAnimation != 0) {
            //    AnimatedVerticesReference = fragments[meshAnimation - 1] as MeshAnimatedVerticesReference;
            // }

            int unknown1 = Reader.ReadInt32();

            // maybe references the first 0x03 in the WLD - unknown
            int unknown2 = Reader.ReadInt32();

            Center = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());

            // 3 unknown dwords
            int unknownDword1 = Reader.ReadInt32();
            int unknownDword2 = Reader.ReadInt32();
            int unknownDword3 = Reader.ReadInt32();

            // Seems to be related to lighting models? (torches, etc.)
            if (unknownDword1 != 0 || unknownDword2 != 0 || unknownDword3 != 0)
            {
                GD.PrintErr($"WldMesh: unknown1 {unknownDword1} unknown2 {unknownDword2} unknown3 {unknownDword3}");
            }

            MaxDistance = Reader.ReadSingle();
            MinPosition = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
            MaxPosition = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());

            short vertexCount = Reader.ReadInt16();
            short textureCoordinateCount = Reader.ReadInt16();
            short normalsCount = Reader.ReadInt16();
            short colorsCount = Reader.ReadInt16();
            short polygonCount = Reader.ReadInt16();
            short vertexPieceCount = Reader.ReadInt16();
            short polygonTextureCount = Reader.ReadInt16();
            short vertexTextureCount = Reader.ReadInt16();
            short meshOpCount = Reader.ReadInt16();
            float scale = 1.0f / (1 << Reader.ReadInt16());

            Vertices = new Vector3[vertexCount];
            Normals = new Vector3[normalsCount];
            Colors = new Color[colorsCount];
            TextureUvCoordinates = new Vector2[textureCoordinateCount];

            for (int i = 0; i < vertexCount; ++i)
            {
                float x = Reader.ReadInt16() * scale;
                float y = Reader.ReadInt16() * scale;
                float z = Reader.ReadInt16() * scale;
                Vertices[i] = new Vector3(x, y, z);
            }

            for (int i = 0; i < textureCoordinateCount; ++i)
            {
                if (wld.IsNewWldFormat)
                {
                    TextureUvCoordinates[i] = new Vector2(Reader.ReadSingle(), Reader.ReadSingle());
                }
                else
                {
                    TextureUvCoordinates[i] = new Vector2(Reader.ReadInt16() / 256.0f, Reader.ReadInt16() / 256.0f);
                }
            }

            for (int i = 0; i < normalsCount; ++i)
            {
                float x = Reader.ReadSByte() / 128.0f;
                float y = Reader.ReadSByte() / 128.0f;
                float z = Reader.ReadSByte() / 128.0f;
                Normals[i] = new Vector3(x, y, z);
            }

            for (int i = 0; i < colorsCount; ++i)
            {
                var colorBytes = BitConverter.GetBytes(Reader.ReadInt32());
                int b = colorBytes[0];
                int g = colorBytes[1];
                int r = colorBytes[2];
                int a = colorBytes[3];

                Colors[i] = new Color(r, g, b, a);
            }

            Indices = [];

            for (int i = 0; i < polygonCount; ++i)
            {
                bool isSolid = Reader.ReadInt16() == 0;

                if (!isSolid)
                {
                    ExportSeparateCollision = true;
                }

                Indices.Add(new Polygon()
                {
                    IsSolid = isSolid,
                    Vertex1 = Reader.ReadInt16(),
                    Vertex2 = Reader.ReadInt16(),
                    Vertex3 = Reader.ReadInt16(),
                });
            }

            MobPieces = [];
            int mobStart = 0;

            for (int i = 0; i < vertexPieceCount; ++i)
            {
                int count = Reader.ReadInt16();
                int index1 = Reader.ReadInt16();
                var mobVertexPiece = new MobVertexPiece
                {
                    Count = count,
                    Start = mobStart,
                    Bone = index1
                };

                mobStart += count;

                MobPieces.Add(mobVertexPiece);
            }

            MaterialGroups = [];

            StartTextureIndex = int.MaxValue;
            var startPolygon = 0;

            for (int i = 0; i < polygonTextureCount; ++i)
            {
                var group = new RenderGroup
                {
                    StartPolygon = startPolygon,
                    PolygonCount = Reader.ReadUInt16(),
                    MaterialIndex = Reader.ReadUInt16()
                };
                MaterialGroups.Add(group);

                startPolygon += group.PolygonCount;

                if (group.MaterialIndex < StartTextureIndex)
                {
                    StartTextureIndex = group.MaterialIndex;
                }
            }

            for (int i = 0; i < vertexTextureCount; ++i)
            {
                Reader.BaseStream.Position += 4;
            }

            for (int i = 0; i < meshOpCount; ++i)
            {
                Reader.BaseStream.Position += 12;
            }

            // In some rare cases, the number of uvs does not match the number of vertices
            if (Vertices.Length != TextureUvCoordinates.Length)
            {
                int difference = Vertices.Length - TextureUvCoordinates.Length;

                for (int i = 0; i < difference; ++i)
                {
                    TextureUvCoordinates[TextureUvCoordinates.Length + i] = new Vector2(0.0f, 0.0f);
                }
            }
        }

        public void ClearCollision()
        {
            foreach (var poly in Indices)
            {
                poly.IsSolid = false;
            }

            ExportSeparateCollision = true;
        }

        public ArrayMesh ToGodotMesh(WldFile wld)
        {
            var arrays = new Godot.Collections.Array();
            arrays.Resize((int)Mesh.ArrayType.Max);

            //GD.Print("vertices ", Vertices.Count);
            arrays[(int)Mesh.ArrayType.Vertex] = Vertices;

            //GD.Print("normals ", Normals.Count);
            arrays[(int)Mesh.ArrayType.Normal] = Normals;

            if (Colors.Length > 0)
            {
                //GD.Print("colors ", Colors.Count);
                arrays[(int)Mesh.ArrayType.Color] = Colors;
            }

            //GD.Print("texture ", TextureUvCoordinates.Count);
            arrays[(int)Mesh.ArrayType.TexUV] = TextureUvCoordinates;

            var bones = new int[Vertices.Length * 4];
            var weights = new float[Vertices.Length * 4];

            //GD.Print("bones ", MobPieces.Count);
            if (MobPieces.Count > 0)
            {
                for (int i = 0; i < MobPieces.Count; i++)
                {
                    var piece = MobPieces[i];
                    for (int j = 0; j < piece.Count; j++)
                    {
                        var startIndex = piece.Start + j;
                        bones[startIndex * 4 + 0] = MobPieces[i].Bone;
                        bones[startIndex * 4 + 1] = 0;
                        bones[startIndex * 4 + 2] = 0;
                        bones[startIndex * 4 + 3] = 0;
                        weights[startIndex * 4 + 0] = 1.0f;
                        weights[startIndex * 4 + 1] = 0.0f;
                        weights[startIndex * 4 + 2] = 0.0f;
                        weights[startIndex * 4 + 3] = 0.0f;
                    }
                }
                arrays[(int)Mesh.ArrayType.Bones] = bones;
                arrays[(int)Mesh.ArrayType.Weights] = weights;
            }

            var mesh = new ArrayMesh();
            for (int j = 0; j < MaterialGroups.Count; j++)
            {
                var group = MaterialGroups[j];
                var indices = new int[group.PolygonCount * 3];
                for (int i = 0; i < group.PolygonCount; i++)
                {
                    indices[i * 3 + 0] = Indices[group.StartPolygon + i].Vertex1;
                    indices[i * 3 + 1] = Indices[group.StartPolygon + i].Vertex2;
                    indices[i * 3 + 2] = Indices[group.StartPolygon + i].Vertex3;
                }
                arrays[(int)Mesh.ArrayType.Index] = indices;

                mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
                mesh.SurfaceSetMaterial(j, wld.Materials[MaterialList.Materials[group.MaterialIndex].Index]);
            }
            mesh.ResourceName = Name;
            return mesh;
        }
    }
}
