﻿using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.godot_resources;

public partial class ActorSkeletonBone : Resource
{
    [Export] public ActorSkeletonPath BasePosition;

    [Export] public string CleanedFullPath;

    [Export] public string CleanedName;

    [Export] public string FullPath;

    [Export] public int Index;

    [Export] public string Name;

    [Export] public ActorSkeletonBone Parent;

    [Export] public Mesh ReferencedMesh;

    [Export] public Dictionary<string, ActorSkeletonPath> AnimationTracks { get; set; }

    //public ParticleCloud ParticleCloud {
    //    get; set;
    //}
}