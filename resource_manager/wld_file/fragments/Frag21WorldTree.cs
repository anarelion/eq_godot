using System.Collections.Generic;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.wld_file.fragments;

// Latern Extractor class
[GlobalClass]
public partial class Frag21WorldTree : WldFragment
{
    [Export] public Array<BspNode> Nodes;
    private WldFile _wldFile;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
    {
        base.Initialize(index, type, size, data, wld);
        _wldFile = wld;
        Name = wld.GetName(Reader.ReadInt32());
        var nodeCount = Reader.ReadInt32();
        Nodes = [];

        for (var i = 0; i < nodeCount; ++i)
            Nodes.Add(new BspNode
            {
                NormalX = Reader.ReadSingle(),
                NormalY = Reader.ReadSingle(),
                NormalZ = Reader.ReadSingle(),
                SplitDistance = Reader.ReadSingle(),
                RegionId = Reader.ReadInt32(),
                LeftNode = Reader.ReadInt32() - 1,
                RightNode = Reader.ReadInt32() - 1
            });
    }

    public void LinkBspRegions(List<Frag22Region> fragments)
    {
        foreach (var node in Nodes)
        {
            if (node.RegionId == 0) continue;

            node.Region = fragments[node.RegionId - 1];
        }
    }

    public Node3D ToGodotZone()
    {
        var zone = new Node3D();
        // zone.RotateX((float)(-Math.PI / 2));

        var queue = new Queue<int>();
        queue.Enqueue(0);
        while (queue.TryDequeue(out var index))
        {
            var node = Nodes[index];
            if (node.RegionId != 0)
            {
                var mesh = node.Region.Mesh;
                if (mesh != null)
                {
                    var arrayMesh = mesh.ToGodotMesh(_wldFile);
                    var inst = new MeshInstance3D { Name = mesh.Name, Mesh = arrayMesh, Position = mesh.Centre };
                    zone.AddChild(inst);
                }
            }

            if (node.LeftNode != -1)
            {
                queue.Enqueue(node.LeftNode);
            }

            if (node.RightNode != -1)
            {
                queue.Enqueue(node.RightNode);
            }
        }

        return zone;
    }
}