using Godot;

namespace EQGodot.resource_manager.wld_file.data_types;

// Latern Extractor class
[GlobalClass]
public partial class RenderGroup : Resource
{
    public int StartPolygon { get; set; }

    public int PolygonCount { get; set; }

    public int MaterialIndex { get; set; }
}