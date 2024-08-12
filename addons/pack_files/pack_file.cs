using Godot;
using System;
using System.IO;
using Ionic.Zlib;
using System.Collections.Generic;
using EQGodot.resource_manager.pack_file;

#if TOOLS
[Tool]
public partial class pack_file : EditorPlugin
{
    private PFSArchiveImport Import;

    public override void _EnterTree()
    {
        GD.Print("Entering tree!");
        Import = new PFSArchiveImport();
        AddImportPlugin(Import, false);
    }

    public override void _ExitTree()
    {
        GD.Print("Exiting tree!");
        if (Import != null)
        {
            RemoveImportPlugin(Import);
            Import = null;
        }
    }
}


public partial class PFSArchiveImport : EditorImportPlugin
{

    public override string _GetImporterName()
    {
        return "pfs.import.plugin";
    }

    public override string _GetVisibleName()
    {
        return "PFS Archive Import";
    }

    public override string[] _GetRecognizedExtensions()
    {
        return ["s3d", "eqg"];
    }

    public override string _GetSaveExtension()
    {
        return "tres";
    }

    public override string _GetResourceType()
    {
        return "PFSArchive";
    }

    public override int _GetPresetCount()
    {
        return 1;
    }

    public override string _GetPresetName(int presetIndex)
    {
        return "Default";
    }

    public override float _GetPriority()
    {
        return 1.0F;
    }

    public override int _GetImportOrder()
    {
        return 1;
    }

    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        return [];
    }

    public override Error _Import(string sourceFile, string savePath, Godot.Collections.Dictionary options, Godot.Collections.Array<string> platformVariants, Godot.Collections.Array<string> genFiles)
    {
        try
        {
            GD.Print($"PfsArchive: Finished post-processing archive: {sourceFile}");
            string destFile = $"{savePath}.{_GetSaveExtension()}";
            var archive = PackFileParser.Load(sourceFile);
            var result = ResourceSaver.Save(archive, destFile);
            GD.Print($"PfsArchive: Finished writing archive: {sourceFile} to {destFile}");
            return result;
        }
        catch (Exception e) { GD.PrintErr(e); return Error.Failed; }
    }
}

#endif

