using Godot;

namespace EQGodot.resource_manager.wld_file.data_types;

// Lantern Extractor class
[GlobalClass]
public partial class Polygon : Resource
{
    public bool IsSolid { get; set; }

    public int Vertex1 { get; set; }

    public int Vertex2 { get; set; }

    public int Vertex3 { get; set; }

    public int MaterialIndex { get; set; }

    public Polygon GetCopy()
    {
        return new Polygon
        {
            IsSolid = IsSolid,
            Vertex1 = Vertex1,
            Vertex2 = Vertex2,
            Vertex3 = Vertex3,
            MaterialIndex = MaterialIndex
        };
    }
}