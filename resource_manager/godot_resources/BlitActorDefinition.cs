using Godot;

namespace EQGodot.resource_manager.godot_resources;

public partial class BlitActorDefinition : Resource
{
    [Export] public int Flags;
    [Export] public string Tag;
    [Export] public Texture2D Texture;
}