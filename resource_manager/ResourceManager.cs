using Godot;
using System;
using EQGodot2.resource_manager.pack_file;
using EQGodot2.resource_manager;
using EQGodot2.helpers;

[GlobalClass]
public partial class ResourceManager : Node {
    private Godot.Collections.Dictionary<string, ActorDefinition> CharacterActor;

    private ResourceManager()
    {
        GD.Print("Starting Resource Manager!");

        CharacterActor = [];

        string[] paths = [
            "res://eq_files/globalogm_chr.s3d",
            "res://eq_files/globalelm_chr.s3d",
            "res://eq_files/globalelm_chr2.s3d",
            "res://eq_files/global_chr.s3d",
        ];

        foreach (var path in paths) {
            var pfsArchive = GD.Load<PFSArchive>(path);
            foreach (var wldFile in pfsArchive.WldFiles) {
                GD.Print($"Loaded {wldFile.Key}");
                foreach (var actorDef in wldFile.Value.ActorDefs) {
                    var name = actorDef.Value.ResourceName;
                    CharacterActor[name] = actorDef.Value;
                    GD.Print($"Loaded {name}");
                }
                foreach (var animation in wldFile.Value.ExtraAnimations) {
                    //GD.Print($"Processing {animation.Value.Name}");
                }
            }
        }

        AddChild(InstantiateCharacter("ogm"));
    }

    public Node3D InstantiateCharacter(string tagName)
    {
        var actorDef = CharacterActor[tagName];
        var node = new Node3D();
        node.Name = actorDef.ResourceName;

        var skeleton = actorDef.BuildSkeleton();
        // swap Y and Z to get a godot coordinate system
        skeleton.RotateX((float)(-Math.PI / 2));
        node.AddChild(skeleton);
        foreach (var mesh in actorDef.Meshes) {
            var inst = new MeshInstance3D {
                Name = mesh.Key,
                Mesh = mesh.Value,
            };
            skeleton.AddChild(inst);
        }
        
        return node;
    }
}

