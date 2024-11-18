using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using EQGodot.resource_manager.wld_file;
using Godot;
using Godot.Collections;
using Pfim;
using Bitmap = System.Drawing.Bitmap;

namespace EQGodot.resource_manager.pack_file;

[GlobalClass]
public partial class PfsArchive : Resource
{
    [Export] public string LoadedPath;
    [Export] public Array<Resource> Files = [];
    [Export] public Godot.Collections.Dictionary<string, Resource> FilesByName = [];
    [Export] public Godot.Collections.Dictionary<string, WldFile> WldFiles = [];
    [Export] public bool IsWldArchive;
    [Export] public PfsArchiveType Type;

    public void ProcessImages()
    {
        List<Task> tasks = [];
        for (var i = 0; i < Files.Count; i++)
        {
            if (Files[i] is not PFSFile file)
            {
                GD.PrintErr($"File is not PFSFile on index {i}");
                continue;
            }

            if (file.FileBytes[0] == 'D' &&
                file.FileBytes[1] == 'D' && file.FileBytes[2] == 'S')
            {
                var localI = i;
                var imageTask = Task.Factory.StartNew(() => { ProcessDdsImage(file, localI); });
                tasks.Add(imageTask);
            }

            if (file.FileBytes[0] == 'B' && file.FileBytes[1] == 'M')
            {
                var localI = i;
                var imageTask = Task.Factory.StartNew(() => { ProcessBmpImage(file, localI); });
                tasks.Add(imageTask);
            }
        }

        Task.WaitAll([..tasks]);
    }

    public void ProcessWldFiles()
    {
        List<Task> wldHandles = [];
        for (var i = 0; i < Files.Count; i++)
            if (Files[i] is PFSFile pfsFile)
                if (pfsFile.Name.EndsWith(".wld"))
                    try
                    {
                        IsWldArchive = true;
                        var index = i;
                        var pfs = pfsFile;
                        wldHandles.Add(Task.Run(() => ProcessWldResource(pfs, index)));
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr("Exception while processing ", pfsFile.Name, " ", ex);
                    }

        Task.WaitAll([.. wldHandles]);

        if (WldFiles.ContainsKey("lights.wld"))
        {
            Type = PfsArchiveType.Zone;
            return;
        }

        if (LoadedPath.Contains("_chr"))
        {
            Type = PfsArchiveType.Character;
            return;
        }

        Type = PfsArchiveType.Equipment;
    }

    private void ProcessDdsImage(PFSFile pfsFile, int index)
    {
        try
        {
            var dds = Dds.Create(pfsFile.FileBytes, new PfimConfig());
            var data = dds.Data;
            Debug.Assert(data.Length % 4 == 0);
            for (var j = 0; j < data.Length / 4; j++)
            {
                var b = dds.Data[j * 4 + 0];
                var g = dds.Data[j * 4 + 1];
                var r = dds.Data[j * 4 + 2];
                var a = dds.Data[j * 4 + 3];
                dds.Data[j * 4 + 0] = r;
                dds.Data[j * 4 + 1] = g;
                dds.Data[j * 4 + 2] = b;
                dds.Data[j * 4 + 3] = a;
            }

            try
            {
                var image = Image.CreateFromData(dds.Width, dds.Height, dds.MipMaps.Length > 1, Image.Format.Rgba8,
                    dds.Data);
                image.SetMeta("pfs_file_name", pfsFile.ArchiveName);
                image.SetMeta("original_file_name", pfsFile.Name);
                image.SetMeta("original_file_type", "DDS");
                // image.FlipY();
                Files[index] = image;
                FilesByName[pfsFile.Name] = image;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"ProcessDDSImage: Exception while creating image from {pfsFile.Name}: {ex}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ProcessDDSImage: Exception while processing image from {pfsFile.Name}: {ex}");
        }
    }

    private void ProcessBmpImage(PFSFile pfsFile, int index)
    {
        try
        {
            //GD.Print("Need to process texture: ", pfsFile.Name);
            // GD.Print(pfsFile.FileBytes.HexEncode());
            var bitmap = new Bitmap(new MemoryStream(pfsFile.FileBytes));
            var data = new byte[bitmap.Width * bitmap.Height * 4];
            var offset = 0;
            for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);

                data[offset + 0] = color.R;
                data[offset + 1] = color.G;
                data[offset + 2] = color.B;
                data[offset + 3] = color.A;
                offset += 4;
            }

            try
            {
                var image = Image.CreateFromData(bitmap.Width, bitmap.Height, false, Image.Format.Rgba8, data);
                image.FlipY();
                image.SetMeta("pfs_file_name", pfsFile.ArchiveName);
                image.SetMeta("original_file_name", pfsFile.Name);
                image.SetMeta("original_file_type", "BMP");
                if (image != null)
                {
                    Files[index] = image;
                    FilesByName[pfsFile.Name] = image;
                }
                else
                {
                    GD.PrintErr($"No image could be created properly from {pfsFile.Name}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"ProcessBMPImage: Exception while creating image from {pfsFile.Name}: {ex}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ProcessBMPImage: Exception while processing image from {pfsFile.Name}: {ex}");
        }
    }

    private void ProcessWldResource(PFSFile pfsFile, int index)
    {
        var wld = new WldFile(pfsFile, this);
        Files[index] = wld;
        FilesByName[pfsFile.Name] = wld;
        WldFiles[pfsFile.Name] = wld;
    }
}