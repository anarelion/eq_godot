namespace EQGodot.resource_manager.wld_file.data_types;

// Latern Extractor class
public class Polygon
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