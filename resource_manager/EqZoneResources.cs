using System.Collections.Generic;
using EQGodot.resource_manager.wld_file.fragments;
using Godot;

namespace EQGodot.resource_manager;

[GlobalClass]
public partial class EqZoneResources : EqResources
{
    private Frag21WorldTree _activeZone = null;
    private List<Frag28PointLight> _activeZoneLights;
    
    // TODO
}