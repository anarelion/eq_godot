using EQGodot.resource_manager.godot_resources;
using EQGodot.resource_manager.pack_file;
using EQGodot.resource_manager.wld_file.fragments;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public partial class WldFile : Resource
    {

        public const int WldFileIdentifier = 0x54503D02;
        public const int WldFormatOldIdentifier = 0x00015500;
        public const int WldFormatNewIdentifier = 0x1000C800;

        [Export]
        public string Name
        {
            get; set;
        }

        private List<WldFragment> Fragments;
        private Dictionary<Type, List<WldFragment>> FragmentTypeDictionary;
        private Dictionary<string, WldFragment> FragmentNameDictionary;
        private PFSArchive Archive;

        [Export]
        public Godot.Collections.Dictionary<int, string> Strings;

        [Export]
        public Godot.Collections.Dictionary<int, int> FragmentTypes;

        [Export]
        public Godot.Collections.Dictionary<int, byte[]> FragmentContents;

        [Export]
        public Godot.Collections.Dictionary<int, Material> Materials;

        [Export]
        public Godot.Collections.Dictionary<int, ArrayMesh> Meshes;

        [Export]
        public Godot.Collections.Dictionary<int, ActorDefinition> ActorDefs
        {
            get; set;
        }
        [Export]
        public Godot.Collections.Dictionary<int, ActorSkeletonPath> ExtraAnimations
        {
            get; set;
        }

        [Export]
        public bool IsNewWldFormat;

        public WldFile() : this(null, null) { }

        public WldFile(PFSFile pfsFile, PFSArchive archive)
        {
            if (pfsFile == null)
            {
                GD.PrintErr("Invalid file received, doing nothing");
                return;
            }
            Name = pfsFile.Name;
            var content = pfsFile.FileBytes;
            Archive = archive;

            GD.Print($"WldFile {Name}: extracting WLD archive of length {content.Length}");
            var reader = new BinaryReader(new MemoryStream(content));

            int identifier = reader.ReadInt32();

            if (identifier != WldFileIdentifier)
            {
                GD.PrintErr("Not a valid WLD file!");
                return;
            }

            Fragments = [];
            Fragments.Add(new FragXXFallback());
            FragmentTypes = [];
            FragmentContents = [];
            FragmentTypeDictionary = [];
            FragmentNameDictionary = [];
            Materials = [];
            Meshes = [];
            ActorDefs = [];
            ExtraAnimations = [];

            int version = reader.ReadInt32();

            switch (version)
            {
                case WldFormatOldIdentifier:
                    break;
                case WldFormatNewIdentifier:
                    IsNewWldFormat = true;
                    GD.Print("New WLD format not fully supported.");
                    break;
                default:
                    GD.PrintErr("Unrecognized WLD format.");
                    return;
            }

            uint fragmentCount = reader.ReadUInt32();
            uint bspRegionCount = reader.ReadUInt32();
            int maxObjectBytes = reader.ReadInt32();
            uint stringHashSize = reader.ReadUInt32();
            int stringCount = reader.ReadInt32();
            // GD.Print("fragmentCount: ", fragmentCount);
            // GD.Print("bspRegionCount: ", bspRegionCount);
            // GD.Print("maxObjectBytes: ", maxObjectBytes);
            // GD.Print("stringHashSize: ", stringHashSize);
            // GD.Print("stringCount: ", stringCount);

            var strings = reader.ReadBytes((int)stringHashSize);
            var decoded = WldStringDecoder.Decode(strings);

            Strings = [];
            int index = 0;
            string[] splitHash = decoded.Split('\0');

            foreach (var hashString in splitHash)
            {
                Strings[index] = hashString;
                index += hashString.Length + 1;
            }

            for (int i = 0; i < fragmentCount; ++i)
            {
                uint fragSize = reader.ReadUInt32();
                int fragType = reader.ReadInt32();
                // if (i % 1000 == 0)
                // {
                //     GD.Print($"WldFile {Name}: Fragment {i} type: {fragType:x} size {fragSize}");
                // }
                var fragmentContents = reader.ReadBytes((int)fragSize);

                FragmentTypes[i + 1] = fragType;
                FragmentContents[i + 1] = fragmentContents;

                var newFragment = !WldFragmentBuilder.Fragments.TryGetValue(fragType, out Func<WldFragment> value)
                    ? new FragXXFallback()
                    : value();

                if (newFragment is FragXXFallback)
                {
                    GD.PrintErr($"WldFile {Name}: Unhandled fragment type: {fragType:x}");
                    break;
                }

                newFragment.Initialize(i, fragType, (int)fragSize, fragmentContents, this);

                Fragments.Add(newFragment);
                if (!FragmentTypeDictionary.ContainsKey(newFragment.GetType()))
                {
                    FragmentTypeDictionary[newFragment.GetType()] = [];
                }

                if (!string.IsNullOrEmpty(newFragment.Name) && !FragmentNameDictionary.ContainsKey(newFragment.Name))
                {
                    FragmentNameDictionary[newFragment.Name] = newFragment;
                }

                FragmentTypeDictionary[newFragment.GetType()].Add(newFragment);
            }

            BuildMaterials();
            BuildMeshes();
            BuildActorDefs();
            BuildAnimations();
            GD.Print($"WldFile {Name}: completed.");
        }

        public List<T> GetFragmentsOfType<T>() where T : WldFragment
        {
            if (!FragmentTypeDictionary.ContainsKey(typeof(T)))
            {
                return [];
            }

            return FragmentTypeDictionary[typeof(T)].Cast<T>().ToList();
        }

        public string GetName(int reference)
        {
            if (reference < 0)
            {
                if (!Strings.ContainsKey(-reference))
                {
                    GD.PrintErr($"WldFile {Name}: String not found at {-reference}");
                }
                return Strings[-reference];
            }
            else if (reference == 0)
            {
                return string.Empty;
            }
            return Fragments[reference].Name;
        }

        public WldFragment GetFragmentByName(String name)
        {
            return FragmentNameDictionary.TryGetValue(name, out WldFragment fragment) ? fragment : null;
        }

        public WldFragment GetFragment(int reference)
        {
            if (reference < 0)
            {
                return FragmentNameDictionary[Strings[-reference]];
            }
            else if (reference == 0)
            {
                return null;
            }
            return Fragments[reference];
        }

        public void BuildMaterials()
        {
            var materials = GetFragmentsOfType<Frag30MaterialDef>();
            foreach (var material in materials)
            {
                var g = material.ToGodotMaterial(Archive);
                if (g != null)
                {
                    Materials.Add(material.Index, g);
                }
            }
        }

        public void BuildMeshes()
        {
            var meshes = GetFragmentsOfType<Frag36DmSpriteDef2>();
            foreach (var mesh in meshes)
            {
                var g = mesh.ToGodotMesh(this);
                if (g != null)
                {
                    Meshes.Add(mesh.Index, g);
                }
            }
        }

        public void BuildActorDefs()
        {
            var actordefs = GetFragmentsOfType<Frag14ActorDef>();
            foreach (var actordef in actordefs)
            {
                var name = FragmentNameCleaner.CleanName(actordef, true);
                ActorDefinition actor = new()
                {
                    ResourceName = name,
                    Tag = name,
                    Flags = actordef.Flags,
                    Bones = [],
                    BonesByName = [],
                    Meshes = [],
                };
                // GD.Print(actor.Tag);

                var skeleton = actordef.HierarchicalSprite?.HierarchicalSpriteDef;
                if (skeleton != null)
                {
                    skeleton.BuildSkeletonData(false);
                    foreach (var skel_mesh in skeleton.Meshes)
                    {
                        actor.Meshes.Add(skel_mesh.Name, Meshes[skel_mesh.Index]);
                    }

                    foreach (var bone in skeleton.Skeleton)
                    {
                        var meshref = bone.MeshReference;
                        var bone_mesh = meshref != null && meshref.Mesh != null ? Meshes[meshref.Mesh.Index] : null;
                        string boneName = bone.Name.Substring(3).ToLower().Replace("_dag", "");
                        if (boneName == "")
                        {
                            boneName = "root";
                        }
                        var rbone = new ActorSkeletonBone
                        {
                            ResourceName = bone.Name,
                            Index = bone.Index,
                            Name = boneName,
                            FullPath = bone.FullPath,
                            CleanedName = bone.CleanedName,
                            CleanedFullPath = bone.CleanedFullPath,
                            ReferencedMesh = bone_mesh,
                            Parent = bone.Parent != null ? actor.Bones[bone.Parent.Index] : null
                        };
                        var track = bone.Track;
                        if (track != null)
                        {
                            track.IsProcessed = true;
                            track.IsPoseAnimation = true;
                            rbone.BasePosition = ConvertTrack(track);
                        }

                        actor.Bones.Add(rbone);
                        if (actor.BonesByName.ContainsKey(boneName))
                        {
                            actor.BonesByName.Remove(boneName);
                        }
                        actor.BonesByName.Add(boneName, rbone);
                    }
                    ActorDefs.Add(actordef.Index, actor);
                    continue;
                }

                var blit = actordef.BlitSprite?.BlitSpriteDef?.SimpleSprite?.SimpleSpriteDef;
                if (blit != null)
                {
                    GD.Print($"WldFile {Name}: Need to process BlitSpriteDef {blit.Name}");
                    continue;
                }

                var dm_mesh = actordef.DMSprite?.Mesh;
                if (dm_mesh != null)
                {
                    actor.Meshes.Add(dm_mesh.Name, Meshes[dm_mesh.Index]);
                    ActorDefs.Add(actordef.Index, actor);
                    continue;
                }

                GD.PrintErr($"WldFile {Name}: Skeleton is null for {actor.Tag} - Sprite2D: {actordef.Sprite2D}");
            }
        }

        public ActorSkeletonPath ConvertTrack(Frag13Track track)
        {
            string animationName = null;
            string pieceName = null;
            if (AnimatedTrackRegex().IsMatch(track.Name))
            {
                animationName = track.Name.Substr(0, 3);
                pieceName = track.Name.Substring(6).ToLower().Replace("_track", "");
                if (pieceName == "")
                {
                    pieceName = "root";
                }
            }
            var skeletonPath = new ActorSkeletonPath
            {
                Name = track.Name.ToLower(),
                AnimationName = animationName,
                PieceName = pieceName,
                FrameMs = track.FrameMs,
                Flags = track.Flags,
                DefFlags = track.TrackDefFragment.Flags,
                Translation = [],
                Rotation = [],
            };
            foreach (var frame in track.TrackDefFragment.Frames)
            {
                skeletonPath.Translation.Add(frame.Translation);
                skeletonPath.Rotation.Add(frame.Rotation);
            }
            return skeletonPath;
        }

        public void BuildAnimations()
        {
            var animations = GetFragmentsOfType<Frag13Track>();
            foreach (var animation in animations)
            {
                if (animation.IsProcessed)
                {
                    continue;
                }
                ExtraAnimations.Add(animation.Index, ConvertTrack(animation));
            }
        }

        [GeneratedRegex(@"^[A-Z][0-9][0-9]")]
        private static partial Regex AnimatedTrackRegex();
    }
}
