using Godot;

namespace EQGodot.resource_manager.wld_file.data_types;

[GlobalClass]
public partial class ZonelineInfo : Resource
{
    [Export] public ZonelineType Type { get; set; }
    [Export] public int Index { get; set; }
    [Export] public Vector3 Position { get; set; }
    [Export] public int Heading { get; set; }
    [Export] public int ZoneIndex { get; set; }
}