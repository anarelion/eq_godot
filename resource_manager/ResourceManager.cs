using Godot;
using System;
using EQGodot2.resource_manager.pack_file;

[GlobalClass]
public partial class ResourceManager : Node {
    private ResourceManager()
    {
        GD.Print("Starting Resource Manager!");

        string[] paths = [
            "res://eq_files/globalogm_chr.s3d",
            "res://eq_files/globalelm_chr.s3d",
            "res://eq_files/global_chr.s3d",
        ];
        var i = 0;

        foreach (var path in paths) {
            var pfsArchive = GD.Load<PFSArchive>(path);
            foreach (var wldFile in pfsArchive.WldFiles) {
                foreach (var actorDef in wldFile.Value.ActorDefs) {
                    var node = new Node3D();
                    node.Translate(new Vector3(-i * 8, 0, 0));
                    node.Name = actorDef.Value.ResourceName + "_Node";

                    var skeleton = actorDef.Value.BuildSkeleton();
                    node.AddChild(skeleton);
                    foreach (var mesh in actorDef.Value.Meshes) {
                        var inst = new MeshInstance3D {
                            Name = mesh.Key,
                            Mesh = mesh.Value,
                        };
                        skeleton.AddChild(inst);

                    }
                    AddChild(node);
                    i++;
                }
            }
        }
    }
}
