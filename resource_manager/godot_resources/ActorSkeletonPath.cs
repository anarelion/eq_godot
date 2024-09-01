using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.godot_resources;

public partial class ActorSkeletonPath : Resource
{
    [Export] public string AnimationName;

    [Export] public int DefFlags;

    [Export] public int Flags;

    [Export] public string ModelName;

    [Export] public string Name;

    [Export] public string PieceName;

    [Export] public int FrameMs { get; set; }

    [Export] public Array<Vector3> Translation { get; set; }

    [Export] public Array<Quaternion> Rotation { get; set; }
}