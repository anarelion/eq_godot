using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EQGodot.resource_manager.godot_resources;
using EQGodot.resource_manager.pack_file;
using Godot;

namespace EQGodot.resource_manager;

[GlobalClass]
public partial class EqResourceLoader : Node
{
    [Export] public string FileName;
    [Export] public bool Loaded;
    [Export] public int AgeCounter;

    [Export] public Godot.Collections.Dictionary<string, Image> Images = [];
    [Export] public Godot.Collections.Dictionary<int, Material> Materials = [];
    [Export] public Godot.Collections.Dictionary<int, ArrayMesh> Meshes = [];
    [Export] public Godot.Collections.Dictionary<string, Resource> ActorDefs = [];
    [Export] public Godot.Collections.Dictionary<int, ActorSkeletonPath> ExtraAnimations = [];


    private Task<bool> _task;

    public override void _Ready()
    {
        _task = Task.Run(async () => await LoadFile(Name));
    }

    public override void _Process(double delta)
    {
        if (_task == null || Loaded || !_task.IsCompleted) return;

        GD.Print($"EqResourceLoader: completed processing {Name} age {AgeCounter}");
        Loaded = true;
        _task = null;
    }

    public Image GetImage(string imageName)
    {
        return Images.GetValueOrDefault(imageName);
    }

    public Resource GetActor(string tag)
    {
        return ActorDefs.GetValueOrDefault(tag);
    }

    public Dictionary<(string, string), ActorSkeletonPath> GetAnimationsFor(string tag)
    {
        Dictionary<(string, string), ActorSkeletonPath> result = [];
        foreach (var animation in ExtraAnimations.Values)
        {
            if (animation.ActorName != tag) continue;
            result[(animation.AnimationName, animation.BoneName)] = animation;
        }

        return result;
    }

    private async Task<bool> LoadFile(string name)
    {
        GD.Print($"EqResourceLoader: requesting {name} at age {AgeCounter}");
        FileName = await TestFiles([$"eq_files/{name}", $"eq_files/{name}.eqg", $"eq_files/{name}.s3d"]);
        if (FileName == null)
        {
            GD.PrintErr($"EqResourceLoader: {name} doesn't exist!");
            return false;
        }

        if (FileName.EndsWith(".s3d"))
        {
            return await ProcessS3DFile(FileName);
        }

        GD.PrintErr($"EqResourceLoader: {name} is an eqg and unsupported!");
        return false;
    }

    private static async Task<string> TestFiles(string[] names)
    {
        List<Task<string>> tasks = [];
        tasks.AddRange(names.Select(name => Task.Run(() => File.Exists(name) ? name : null)));

        var results = await Task.WhenAll([..tasks]);
        return results.FirstOrDefault(result => result != null);
    }

    private async Task<bool> ProcessS3DFile(string filename)
    {
        GD.Print($"EqResourceLoader: starting to load {filename}");
        var archive = await PackFileParser.Load(filename);
        GD.Print($"EqResourceLoader: processing images {filename}");
        Images = await archive.ProcessImages();
        GD.Print($"EqResourceLoader: loaded images {filename} - count {Images.Count}");
        var wldFiles = archive.ProcessWldFiles(this);
        if (wldFiles.TryGetValue("objects.wld", out var objectsWld))
        {
            GD.PrintErr($"EqResourceLoader: {Name} contains objects.wld but is unsupported: {objectsWld}");
        }

        if (wldFiles.TryGetValue("lights.wld", out var lightsWld))
        {
            GD.PrintErr($"EqResourceLoader: {Name} contains lights.wld but is unsupported: {lightsWld}");
        }

        if (wldFiles.TryGetValue($"{Name}.wld", out var mainWld))
        {
            ActorDefs = mainWld.ActorDefs;
            ExtraAnimations = mainWld.ExtraAnimations;
        }

        return true;
    }
}