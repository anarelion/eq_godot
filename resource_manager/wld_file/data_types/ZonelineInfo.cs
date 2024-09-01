using Godot;

namespace EQGodot.resource_manager.wld_file.data_types;

public class ZonelineInfo
{
    public ZonelineType Type { get; set; }
    public int Index { get; set; }
    public Vector3 Position { get; set; }
    public int Heading { get; set; }
    public int ZoneIndex { get; set; }
}