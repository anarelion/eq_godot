using System.Diagnostics;
using System.IO;
using EQGodot.resource_manager.wld_file.fragments;
using Godot;

namespace EQGodot.resource_manager;

public partial class EqGlobalResources : EqResources
{
    
    public override void _Ready()
    {
        Debugger.Break();
        using var reader = new StreamReader("eq_files/Resources/GlobalLoad.txt");
        while (reader.ReadLine() is { } line)
        {
            var values = line.Split(',');
            StartEqResourceLoad(values[3]);
        }
    }

    protected override void OnLoadCompleted()
    {
        GD.Print($"EqGlobalResources finished in {Time.GetTicksMsec()}ms since game started");
    }
}