using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EQGodot.resource_manager.godot_resources;
using EQGodot.resource_manager.pack_file;
using EQGodot.resource_manager.wld_file;
using EQGodot.resource_manager.wld_file.fragments;
using Godot;

namespace EQGodot.resource_manager;

[GlobalClass]
public partial class ResourceManager : Node
{
    private Godot.Collections.Dictionary<string, BlitActorDefinition> _blitActor = [];
    private Godot.Collections.Dictionary<string, HierarchicalActorDefinition> _hierarchicalActor = [];
    private Godot.Collections.Dictionary<string, ActorSkeletonPath> _extraAnimations = [];

    // Active loaded zone
    private Frag21WorldTree _activeZone = null;
    private List<Frag28PointLight> _activeZoneLights;

    // Where instantiation of objects in the zone is going to happen
    private Node3D _sceneRoot;

    // Tasks loading stuff
    private readonly List<Task<PfsArchive>> _s3dDecompressionTasks = [];
    private readonly List<Task<PfsArchive>> _imageProcessingTasks = [];
    private readonly List<Task<PfsArchive>> _wldProcessingTasks = [];

    public override void _Ready()
    {
        GD.Print("Starting Resource Manager!");
        Name = "ResourceManager";

        _sceneRoot = new Node3D();
        _sceneRoot.RotateX((float)(-Math.PI / 2));
        AddChild(_sceneRoot);

        StartLoadingS3D("eq_files/gequip.s3d");
        StartLoadingS3D("eq_files/global_obj.s3d");
        StartLoadingS3D("eq_files/global_chr.s3d");
        StartLoadingS3D("eq_files/global2_chr.s3d");
        StartLoadingS3D("eq_files/global3_chr.s3d");
        StartLoadingS3D("eq_files/global4_chr.s3d");
        StartLoadingS3D("eq_files/global5_chr.s3d");
        StartLoadingS3D("eq_files/global6_chr.s3d");
        StartLoadingS3D("eq_files/global7_chr.s3d");
        StartLoadingS3D("eq_files/gfaydark.s3d");
    }

    public override void _Process(double delta)
    {
        ProcessS3dDecompressionTasks();
        ProcessImageProcessingTasks();
        ProcessConvertedWldFiles();
    }

    private void StartLoadingS3D(string path)
    {
        _s3dDecompressionTasks.Add(Task.Factory.StartNew(() =>
        {
            GD.Print($"Loading {path} in a thread");
            return PackFileParser.Load(path);
        }));
    }

    private void ProcessS3dDecompressionTasks()
    {
        foreach (var pfsTask in _s3dDecompressionTasks.ToList().Where(task => task.IsCompleted))
        {
            var archive = pfsTask.Result;
            GD.Print($"Completed PFS Decompression {archive.LoadedPath} in a thread");
            _s3dDecompressionTasks.Remove(pfsTask);
            _imageProcessingTasks.Add(Task.Factory.StartNew(() =>
            {
                archive.ProcessImages();
                return archive;
            }));
        }
    }

    private void ProcessImageProcessingTasks()
    {
        foreach (var imageTask in _imageProcessingTasks.ToList().Where(task => task.IsCompleted))
        {
            var archive = imageTask.Result;
            GD.Print($"Completed Image Processing {archive.LoadedPath} in a thread");
            _imageProcessingTasks.Remove(imageTask);
            _wldProcessingTasks.Add(Task.Factory.StartNew(() =>
            {
                archive.ProcessWldFiles();
                return archive;
            }));
        }
    }

    private void ProcessConvertedWldFiles()
    {
        foreach (var wldTask in _wldProcessingTasks.ToList().Where(task => task.IsCompleted))
        {
            var archive = wldTask.Result;
            GD.Print($"Completed Wld Processing {archive.LoadedPath} in a thread");
            _wldProcessingTasks.Remove(wldTask);

            switch (archive.Type)
            {
                case PfsArchiveType.Character:
                case PfsArchiveType.Equipment:
                    foreach (var wldFile in archive.WldFiles.Values)
                    {
                        GD.Print($"Processing Wld Actors {wldFile}");
                        ProcessWldActors(wldFile);
                    }

                    break;

                case PfsArchiveType.Zone:
                    var lights = archive.WldFiles.GetValueOrDefault("lights.wld");
                    if (lights == null)
                    {
                        GD.PrintErr("No lights.wld found");
                        continue;
                    }
                    GD.Print($"Processing lights");
                    _activeZoneLights = lights.ZoneLights;
                    
                    var objects = archive.WldFiles.GetValueOrDefault("objects.wld");
                    if (objects == null)
                    {
                        GD.PrintErr("No objects.wld found");
                        continue;
                    }
                    GD.Print($"Processing objects");
                    // TODO: Process objects.wld
                    
                    foreach (var wldFile in archive.WldFiles)
                    {
                        if (wldFile.Value.WorldTree != null)
                        {
                            GD.Print($"OnPFSArchiveLoaded: activating zone {wldFile.Key}");
                            _activeZone = wldFile.Value.WorldTree;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


    private void ProcessWldActors(WldFile file)
    {
        foreach (var actorDef in file.ActorDefs)
        {
            var name = actorDef.Value.ResourceName;
            switch (actorDef.Value)
            {
                case HierarchicalActorDefinition hierarchicalActor:
                    _hierarchicalActor[name] = hierarchicalActor;
                    // GD.Print($"OnPFSArchiveLoaded: Loaded HierarchicalActorDefinition {name}");
                    break;
                case BlitActorDefinition blitActor:
                    _blitActor[name] = blitActor;
                    // GD.Print($"OnPFSArchiveLoaded: Loaded BlitActorDefinition {name}");
                    break;
            }
        }

        foreach (var animation in file.ExtraAnimations.Values)
        {
            // GD.Print($"Loading Animation {animation.Name}");
            if (_extraAnimations.TryAdd(animation.Name, animation)) continue;
            // GD.Print($"OnPFSArchiveLoaded: {animation.Name} already exists");
        }
    }

    private static string ConvertAnimationTag(string tagName)
    {
        return tagName;
    }

    public void InstantiateActor(string tag)
    {
        if (_hierarchicalActor.TryGetValue(tag, out var actor))
        {
            GD.Print($"Instantiating Hierarchical Actor {actor.ResourceName}");
            _sceneRoot.AddChild(actor.InstantiateCharacter(this));
        }
    }

    public void InstantiateZone()
    {
        if (_activeZone == null) return;
        GD.Print($"Instantiating zone {_activeZone}");
        _sceneRoot.AddChild(_activeZone.ToGodotZone());
        foreach (var l in _activeZoneLights)
        {
            _sceneRoot.AddChild(l.ToGodotLight());
        }
    }

    public List<ActorSkeletonPath> GetAnimationsFor(string actorName)
    {
        GD.Print($"Getting animations for {actorName}");
        List<ActorSkeletonPath> result = [];
        result.AddRange(_extraAnimations.Values.Where(animation => animation.ActorName == actorName));
        GD.Print($"Got {result.Count} out of {_extraAnimations.Count} animations for {actorName}");
        return result;
    }

    // Zone loading orchestration
    // Notes, the order in which the original client loads files is
    // - %s_environmentEmitters.txt -> load whatever this points to
    // - %s_%2s_obj2 -> with second argument being country code for asian countries or us
    // - %s_obj2 -> load item definitions
    // - %s_%2s_obj -> with second argument being country code for asian countries or us
    // - %s_obj -> load item definitions
    // - %s_%2s_2_obj -> with second argument being country code for asian countries or us
    // - %s_2_obj -> load item definitions
    // - %s_chr2 -> load character definitions
    // - %s2_chr -> load character definitions
    // - %s_chr -> load character definitions
    // - %s_chr.txt -> load whatever this points to
    // - %s_assets.txt -> load whatever this points to
    // - load main
    // - process objects.wld
    // - process lights.wld
    // - process %s.wld


    public void LoadZone(string zoneName)
    {
    }
}