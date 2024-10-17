using EQGodot.resource_manager.wld_file.fragments;
using Godot;

namespace EQGodot.resource_manager.wld_file.data_types;

[GlobalClass]
public partial class BspNode : Resource
{
    [Export] public float NormalX;
    [Export] public float NormalY;
    [Export] public float NormalZ;
    [Export] public float SplitDistance;
    [Export] public int RegionId;
    [Export] public int LeftNode;
    [Export] public int RightNode;
    [Export] public Frag22Region Region;
}