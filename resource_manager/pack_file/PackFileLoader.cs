using EQGodot.resource_manager.wld_file;
using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace EQGodot.resource_manager.pack_file
{
    [GlobalClass]
    public partial class PackFileLoader : ResourceFormatLoader
    {
        public override string[] _GetRecognizedExtensions()
        {
            GD.Print("_GetRecognizedExtensions");
            return ["s3d"];
        }

        public override string _GetResourceScriptClass(string path)
        {
            GD.Print($"_GetResourceScriptClass({path})");
            return "PFSArchive.cs";
        }

        public override string _GetResourceType(string path)
        {
            GD.Print($"_GetResourceType({path})");
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file.GetError() != Error.Ok)
            {
                GD.PrintErr("Failed to open: ", path);
                throw new Exception($"Failed to open {path}");
            }
            file.BigEndian = false;

            var directoryOffset = file.Get32();
            var magicNumber = file.Get32();
            return magicNumber == 0x20534650 || magicNumber == 0x50465320 ? "PFSArchive" : "";
        }

        public override bool _HandlesType(StringName type)
        {
            GD.Print($"_HandlesType({type})");
            return type == "PFSArchive";
        }

        public override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
        {
            GD.Print($"_Load({path}, {originalPath}, {useSubThreads}, {cacheMode})");
            return PackFileParser.Load(path);
        }
    }
}