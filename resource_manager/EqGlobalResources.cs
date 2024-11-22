using System.Diagnostics;
using System.IO;
using EQGodot.resource_manager.wld_file.fragments;
using Godot;

namespace EQGodot.resource_manager;

public partial class EqGlobalResources : EqResources
{
    private bool _flaggedFinished = false;
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

    public override void _Process(double delta)
    {
        var children = GetChildren();
        var allDone = true;
        foreach (var child in children)
        {
            if (child is EqResourceLoader resourceLoader)
            {
                if (resourceLoader.Loaded == false)
                {
                    allDone = false;
                }
            }
        }

        if (allDone && !_flaggedFinished)
        {
            GD.Print($"EqGlobalResources finished in {Time.GetTicksMsec()}ms since game started");
            _flaggedFinished = true;
        }
    }
}