using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
[GlobalClass]
public partial class Frag28PointLight : WldFragment
{
    [Export] public Frag1CLight LightReference;
    [Export] public Vector3 Position;
    [Export] public float Radius;

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