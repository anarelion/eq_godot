﻿using EQGodot2.helpers;
using EQGodot2.resource_manager.pack_file;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file
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
            GD.Print("Extracting WLD archive of length ", content.Length);
            var reader = new BinaryReader(new MemoryStream(content));

            int identifier = reader.ReadInt32();

            if (identifier != WldFileIdentifier)
            {
                GD.PrintErr("Not a valid WLD file!");
                return;
            }

            Fragments = [];
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
            //GD.Print("fragmentCount: ", fragmentCount);
            //GD.Print("bspRegionCount: ", bspRegionCount);
            //GD.Print("maxObjectBytes: ", maxObjectBytes);
            //GD.Print("stringHashSize: ", stringHashSize);
            //GD.Print("stringCount: ", stringCount);

            var strings = reader.ReadBytes((int)stringHashSize);
            var decoded = WldStringDecoder.Decode(strings);

            Strings = new Godot.Collections.Dictionary<int, string>();
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
                if (i % 500 == 0)
                {
                    GD.Print($"WldFile {Name}: Fragment {i} type: {fragType:x} size {fragSize}");
                }
                var fragmentContents = reader.ReadBytes((int)fragSize);

                var newFragment = !WldFragmentBuilder.Fragments.ContainsKey(fragType)
                    ? new WldGeneric()
                    : WldFragmentBuilder.Fragments[fragType]();

                if (newFragment is WldGeneric)
                {
                    GD.PrintErr($"WldFile {Name}: Unhandled fragment type: {fragType:x}");
                    break;
                }

                newFragment.Initialize(i, (int)fragSize, fragmentContents, Fragments, Strings,
                    IsNewWldFormat);
                // newFragment.OutputInfo();

                Fragments.Add(newFragment);
                if (!FragmentTypeDictionary.ContainsKey(newFragment.GetType()))
                {
                    FragmentTypeDictionary[newFragment.GetType()] = new List<WldFragment>();
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
        }

        public List<T> GetFragmentsOfType<T>() where T : WldFragment
        {
            if (!FragmentTypeDictionary.ContainsKey(typeof(T)))
            {
                return new List<T>();
            }

            return FragmentTypeDictionary[typeof(T)].Cast<T>().ToList();
        }

        public void BuildMaterials()
        {
            var materials = GetFragmentsOfType<WldMaterial>();
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
            var meshes = GetFragmentsOfType<WldMesh>();
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
            var actordefs = GetFragmentsOfType<WldActorDef>();
            foreach (var actordef in actordefs)
            {
                var name = FragmentNameCleaner.CleanName(actordef, true);
                ActorDefinition actor = new ActorDefinition
                {
                    ResourceName = name,
                    Tag = name,
                    Flags = actordef.Flags,
                    Bones = [],
                    BonesByName = [],
                    Meshes = [],
                };
                GD.Print(actor.Tag);

                var skeleton = actordef.SkeletonReference?.SkeletonHierarchy;
                if (skeleton != null)
                {
                    skeleton.BuildSkeletonData(false);
                    foreach (var mesh in skeleton.Meshes)
                    {
                        actor.Meshes.Add(mesh.Name, Meshes[mesh.Index]);
                    }

                    foreach (var bone in skeleton.Skeleton)
                    {
                        var meshref = bone.MeshReference;
                        var mesh = meshref != null && meshref.Mesh != null ? Meshes[meshref.Mesh.Index] : null;
                        var boneName = bone.Name.Substring(3).ToLower().Replace("_dag", "");
                        if (boneName == "") {
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
                            ReferencedMesh = mesh,
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
                }
                else
                {
                    GD.PrintErr($"Skeleton is null for {actor.Tag}");
                }
                ActorDefs.Add(actordef.Index, actor);
            }
        }

        public ActorSkeletonPath ConvertTrack(WldTrackFragment track)
        {
            string animationName = null;
            string pieceName = null;
            if (Regex.IsMatch(track.Name, @"^[A-Z][0-9][0-9]")) {
                animationName = track.Name.Substr(0,3);
                pieceName = track.Name.Substring(6).ToLower().Replace("_track", "");
                if (pieceName == "") {
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
            var animations = GetFragmentsOfType<WldTrackFragment>();
            foreach (var animation in animations)
            {
                if (animation.IsProcessed)
                {
                    continue;
                }
                ExtraAnimations.Add(animation.Index, ConvertTrack(animation));
            }
        }
    }
}
