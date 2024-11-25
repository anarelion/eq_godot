using Godot;

namespace EQGodot.resource_manager.godot_resources;

[GlobalClass]
public partial class HierarchicalActorInstance : Node3D
{
    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void AttachItem(string tag, string bone, ResourceManager resourceManager)
    {
        resourceManager.InstantiateHierarchicalInto(tag, this.GetNode($"{Name}_skeleton/{bone}_attachment"));
    }
}