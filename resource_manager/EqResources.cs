using System.Collections.Generic;
using EQGodot.resource_manager.godot_resources;
using Godot;

namespace EQGodot.resource_manager;

[GlobalClass]
public partial class EqResources : Node
{
    private Godot.Collections.Dictionary<string, BlitActorDefinition> _blitActor = [];
    private Godot.Collections.Dictionary<string, HierarchicalActorDefinition> _hierarchicalActor = [];
    private Godot.Collections.Dictionary<string, ActorSkeletonPath> _extraAnimations = [];

    private int _ageCounter = 0;

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void StartEqResourceLoad(string name)
    {
        var loader = new EqResourceLoader()
        {
            Name = name.ToLower(),
            AgeCounter = _ageCounter
        };
        loader.SetProcessThreadGroup(ProcessThreadGroupEnum.SubThread);
        AddChild(loader);
        _ageCounter += 1;
    }

    public Image GetImage(string name)
    {
        var children = GetChildren();
        children.Reverse();
        foreach (var node in children)
        {
            if (node is not EqResourceLoader loader) continue;
            var image = loader.GetImage(name);
            if (image != null) return image;
        }

        return null;
    }

    public Resource GetActor(string tag)
    {
        var children = GetChildren();
        children.Reverse();
        foreach (var node in children)
        {
            if (node is not EqResourceLoader loader) continue;
            var image = loader.GetActor(tag);
            if (image != null) return image;
        }

        return null;
    }
}