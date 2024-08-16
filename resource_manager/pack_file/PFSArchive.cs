using EQGodot.resource_manager.wld_file;
using Godot;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Data;

namespace EQGodot.resource_manager.pack_file
{
    [GlobalClass]
    public partial class PFSArchive : Resource
    {
        [Export]
        public Godot.Collections.Array<Resource> Files;

        [Export]
        public Godot.Collections.Dictionary<string, Resource> FilesByName;

        [Export]
        public Godot.Collections.Dictionary<string, WldFile> WldFiles;

        [Export]
        public bool IsWldArchive
        {
            get; set;
        }

        public PFSArchive()
        {
            Files = [];
            FilesByName = [];
            WldFiles = [];
        }

        public void ProcessFiles()
        {
            List<Task> image_handles = [];
            for (var i = 0; i < Files.Count; i++)
            {
                if (Files[i] is PFSFile pfsFile && (pfsFile.Name.EndsWith(".dds") || (pfsFile.FileBytes[0] == 'D' && pfsFile.FileBytes[1] == 'D' && pfsFile.FileBytes[2] == 'S')))
                {
                    var index = i;
                    var pfs = pfsFile;
                    image_handles.Add(Task.Run(() => ProcessDDSImage(pfs, index)));
                }
            }

            for (var i = 0; i < Files.Count; i++)
            {
                if (Files[i] is PFSFile pfsFile)
                {
                    if (pfsFile.Name.EndsWith(".bmp"))
                    {
                        var index = i;
                        var pfs = pfsFile;
                        image_handles.Add(Task.Run(() => ProcessBMPImage(pfs, index)));
                    }
                }
            }

            Task.WaitAll([.. image_handles]);

            List<Task> wld_handles = [];
            for (var i = 0; i < Files.Count; i++)
            {
                if (Files[i] is PFSFile pfsFile)
                {
                    if (pfsFile.Name.EndsWith(".wld"))
                    {
                        try
                        {

                            IsWldArchive = true;
                            var index = i;
                            var pfs = pfsFile;
                            wld_handles.Add(Task.Run(() => ProcessWldResource(pfs, index)));

                        }
                        catch (Exception ex)
                        {
                            GD.PrintErr("Exception while processing ", pfsFile.Name, " ", ex);
                        }
                    }
                }
            }
            Task.WaitAll([.. wld_handles]);
        }

        private void ProcessDDSImage(PFSFile pfsFile, int index)
        {
            try
            {
                var dds = Pfim.Dds.Create(pfsFile.FileBytes, new Pfim.PfimConfig());
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
                    var image = Image.CreateFromData(dds.Width, dds.Height, dds.MipMaps.Length > 1, Image.Format.Rgba8, dds.Data);
                    image.FlipY();
                    ImageTexture texture = ImageTexture.CreateFromImage(image);
                    if (texture != null)
                    {
                        Files[index] = texture;
                        FilesByName[pfsFile.Name] = texture;
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"ProcessDDSImage: Exception while creating image from {pfsFile.Name}: {ex}");
                    return;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"ProcessDDSImage: Exception while processing image from {pfsFile.Name}: {ex}");
            }
        }

        private void ProcessBMPImage(PFSFile pfsFile, int index)
        {
            try
            {
                //GD.Print("Need to process texture: ", pfsFile.Name);
                // GD.Print(pfsFile.FileBytes.HexEncode());
                var bitmap = new System.Drawing.Bitmap(new MemoryStream(pfsFile.FileBytes));
                var data = new byte[bitmap.Width * bitmap.Height * 4];
                var offset = data.Length - 4;
                for (var y = 0; y < bitmap.Height; y++)
                {
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var color = bitmap.GetPixel(x, y);

                        data[offset + 0] = color.R;
                        data[offset + 1] = color.G;
                        data[offset + 2] = color.B;
                        data[offset + 3] = color.A;
                        offset -= 4;
                    }
                }

                try
                {
                    var image = Image.CreateFromData(bitmap.Width, bitmap.Height, false, Image.Format.Rgba8, data);
                    ImageTexture texture = ImageTexture.CreateFromImage(image);
                    if (texture != null)
                    {
                        Files[index] = texture;
                        FilesByName[pfsFile.Name] = texture;
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"ProcessBMPImage: Exception while creating image from {pfsFile.Name}: {ex}");
                    return;
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
}