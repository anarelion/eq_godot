using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EQGodot.resource_manager.godot_resources;
using EQGodot.resource_manager.godot_resources.converters;
using EQGodot.resource_manager.pack_file;
using EQGodot.resource_manager.wld_file.fragments;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.wld_file;

// Latern Extractor class
public partial class WldFile : Resource
{
    private const int WldFileIdentifier = 0x54503D02;
    private const int WldFormatOldIdentifier = 0x00015500;
    private const int WldFormatNewIdentifier = 0x1000C800;

    [Export] public Godot.Collections.Dictionary<int, Resource> ActorDefs = [];
    private PfsArchive Archive;
    [Export] public Godot.Collections.Dictionary<int, ActorSkeletonPath> ExtraAnimations = [];
    [Export] public Godot.Collections.Dictionary<int, byte[]> FragmentContents = [];
    [Export] private Godot.Collections.Dictionary<string, WldFragment> FragmentNameDictionary = [];
    private readonly List<WldFragment> _fragments = [];
    private readonly System.Collections.Generic.Dictionary<Type, List<WldFragment>> _fragmentTypeDictionary = [];
    [Export] public Godot.Collections.Dictionary<int, int> FragmentTypes = [];
    [Export] public bool IsNewWldFormat;
    [Export] public Godot.Collections.Dictionary<int, Material> Materials = [];
    [Export] public Godot.Collections.Dictionary<int, ArrayMesh> Meshes = [];
    [Export] public Godot.Collections.Dictionary<string, Array<Resource>> Resources = [];
    [Export] public Godot.Collections.Dictionary<int, string> Strings = [];
    [Export] public Frag21WorldTree WorldTree = null;
    public List<Frag28PointLight> ZoneLights;

    public WldFile()
        : this(null, null)
    {
    }

    public WldFile(PFSFile pfsFile, PfsArchive archive)
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

        var identifier = reader.ReadInt32();

        if (identifier != WldFileIdentifier)
        {
            GD.PrintErr("Not a valid WLD file!");
            return;
        }

        _fragments =
        [
            new fragments.FragXXFallback()
        ];

        var version = reader.ReadInt32();

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

        var fragmentCount = reader.ReadUInt32();
        var bspRegionCount = reader.ReadUInt32();
        var maxObjectBytes = reader.ReadInt32();
        var stringHashSize = reader.ReadUInt32();
        var stringCount = reader.ReadInt32();
        // GD.Print("fragmentCount: ", fragmentCount);
        // GD.Print("bspRegionCount: ", bspRegionCount);
        // GD.Print("maxObjectBytes: ", maxObjectBytes);
        // GD.Print("stringHashSize: ", stringHashSize);
        // GD.Print("stringCount: ", stringCount);

        var strings = reader.ReadBytes((int)stringHashSize);
        var decoded = WldStringDecoder.Decode(strings);

        Strings = [];
        var index = 0;
        var splitHash = decoded.Split('\0');

        foreach (var hashString in splitHash)
        {
            Strings[index] = hashString;
            index += hashString.Length + 1;
        }

        for (var i = 0; i < fragmentCount; ++i)
        {
            var fragSize = reader.ReadUInt32();
            var fragType = reader.ReadInt32();
            var fragmentContents = reader.ReadBytes((int)fragSize);

            FragmentTypes[i + 1] = fragType;
            FragmentContents[i + 1] = fragmentContents;

            var newFragment = !WldFragmentBuilder.Fragments.TryGetValue(
                fragType,
                out var value
            )
                ? new fragments.FragXXFallback()
                : value();

            if (newFragment is fragments.FragXXFallback)
            {
                GD.PrintErr($"WldFile {Name}: Unhandled fragment type: {fragType:x}");
                break;
            }

            newFragment.Initialize(i, fragType, (int)fragSize, fragmentContents, this);

            _fragments.Add(newFragment);
            if (!_fragmentTypeDictionary.ContainsKey(newFragment.GetType()))
                _fragmentTypeDictionary[newFragment.GetType()] = [];

            if (
                !string.IsNullOrEmpty(newFragment.Name)
            )
                FragmentNameDictionary.TryAdd(newFragment.Name, newFragment);

            _fragmentTypeDictionary[newFragment.GetType()].Add(newFragment);
        }

        BuildMaterials();
        BuildMeshes();
        BuildActorDefs();
        BuildAnimations();
        BuildWorldTree();
        BuildLights();
        GD.Print($"WldFile {Name}: completed.");
    }

    [Export] public string Name { get; set; }

    private List<T> GetFragmentsOfType<T>()
        where T : WldFragment
    {
        return !_fragmentTypeDictionary.ContainsKey(typeof(T))
            ? []
            : _fragmentTypeDictionary[typeof(T)].Cast<T>().ToList();
    }

    public string GetName(int reference)
    {
        switch (reference)
        {
            case < 0:
            {
                if (!Strings.ContainsKey(-reference)) GD.PrintErr($"WldFile {Name}: String not found at {-reference}");

                return Strings[-reference];
            }
            case 0:
                return string.Empty;
            default:
                return _fragments[reference].Name;
        }
    }

    public WldFragment GetFragmentByName(string name)
    {
        return FragmentNameDictionary.GetValueOrDefault(name);
    }

    public WldFragment GetFragment(int reference)
    {
        return reference switch
        {
            < 0 => FragmentNameDictionary[Strings[-reference]],
            0 => null,
            _ => _fragments[reference]
        };
    }

    private void AddResource(string name, Resource resource)
    {
        if (!Resources.TryGetValue(name, out var value)) Resources[name] = [];

        Resources[name].Add(resource);
    }

    private void BuildMaterials()
    {
        var materials = GetFragmentsOfType<Frag30MaterialDef>();
        foreach (var material in materials)
        {
            var godotMaterial = material.ToGodotMaterial(Archive);
            if (godotMaterial == null) continue;

            Materials.Add(material.Index, godotMaterial);
            AddResource(material.Name, godotMaterial);
        }
    }

    private void BuildMeshes()
    {
        var meshes = GetFragmentsOfType<Frag36DmSpriteDef2>();
        foreach (var mesh in meshes)
        {
            var godotMesh = mesh.ToGodotMesh(this);
            if (godotMesh == null) continue;
            Meshes.Add(mesh.Index, godotMesh);
            AddResource(mesh.Name, godotMesh);
        }
    }

    private void BuildActorDefs()
    {
        var actorDefs = GetFragmentsOfType<Frag14ActorDef>();
        foreach (var actorDef in actorDefs)
        {
            GD.Print($@"WldFile {Name}: {actorDef.Name}");
            var skeleton = actorDef.HierarchicalSprite?.HierarchicalSpriteDef;
            if (skeleton != null)
            {
                var godotHierarchicalActor = HierarchicalActorBuilder.Convert(actorDef, skeleton, Meshes);
                ActorDefs.Add(actorDef.Index, godotHierarchicalActor);
                AddResource(actorDef.Name, godotHierarchicalActor);
                continue;
            }

            var blit = actorDef.BlitSprite;
            if (blit != null)
            {
                var def = blit.BlitSpriteDef;
                var sprite = def?.SimpleSprite;
                if (sprite != null)
                {
                    // var godotBlitActor = new BlitActorDefinition
                    // {
                    //     ResourceName = actorDef.Name, Texture = sprite.ToGodotTexture(Archive),
                    //     Flags = blit.Flags,
                    //     Tag = actorDef.Name
                    // };
                    // ActorDefs.Add(actorDef.Index, godotBlitActor);
                    // AddResource(actorDef.Name, godotBlitActor);
                }
                else
                {
                    GD.PrintErr($"WldFile {Name} - {actorDef.Name}: No sprite found");
                }

                continue;
            }

            var dmMesh = actorDef.DMSprite?.Mesh;
            if (dmMesh != null)
            {
                // actor.Meshes.Add(dmMesh.Name, Meshes[dmMesh.Index]);
                // ActorDefs.Add(actorDef.Index, actor);
                continue;
            }

            GD.PrintErr(
                $"WldFile {Name}: Skeleton is null for {actorDef.Name} - Mesh: {actorDef.DMSprite} - Sprite2D: {actorDef.Sprite2D}"
            );
        }
    }

    private void BuildAnimations()
    {
        var animations = GetFragmentsOfType<Frag13Track>();
        foreach (var animation in animations.Where(animation => !animation.IsProcessed))
            ExtraAnimations.Add(
                animation.Index,
                ActorSkeletonPath.FromFrag13Track(animation)
            );
    }

    private void BuildWorldTree()
    {
        GD.Print("WldFile: Building world tree");
        var worlds = GetFragmentsOfType<Frag21WorldTree>();
        switch (worlds.Count)
        {
            case 0:
                return;
            case > 1:
                GD.PrintErr($"More than one world tree found for {Name}.");
                break;
        }

        WorldTree = worlds[0];
        var regions = GetFragmentsOfType<Frag22Region>();
        WorldTree.LinkBspRegions(regions);
    }

    private void BuildLights()
    {
        ZoneLights = GetFragmentsOfType<Frag28PointLight>();
    }
}