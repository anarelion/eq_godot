using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
public partial class Frag28PointLight : WldFragment
{
    public Frag1CLight LightReference { get; private set; }

    /// <summary>
    ///     The position of the light
    /// </summary>
    public Vector3 Position { get; private set; }

    /// <summary>
    ///     The radius of the light
    /// </summary>
    public float Radius { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        Name = wld.GetName(Reader.ReadInt32());
        LightReference = wld.GetFragment(Reader.ReadInt32()) as Frag1CLight;
        var flags = Reader.ReadInt32();
        Position = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
        Radius = Reader.ReadSingle();
    }
}