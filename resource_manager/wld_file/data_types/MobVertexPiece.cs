using Godot;

namespace EQGodot.resource_manager.wld_file.data_types;

// Lantern Extractor class
[GlobalClass]
public partial class MobVertexPiece : Resource
{
    public int Start { get; set; }

    public int Count { get; set; }

    public int Bone { get; set; }
}