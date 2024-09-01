using Godot;

namespace EQGodot.resource_manager.wld_file.data_types;

// Latern Extractor class
public class BoneTransform
{
    public Quaternion ModelMatrix;

    public Vector3 Translation { get; set; }

    public Quaternion Rotation { get; set; }

    public float Scale { get; set; }
}