using EQGodot2.resource_manager.wld_file;
using Godot;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace EQGodot2.resource_manager.pack_file
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
            Files = new Godot.Collections.Array<Resource>();
            FilesByName = new Godot.Collections.Dictionary<string, Resource>();
            WldFiles = new Godot.Collections.Dictionary<string, WldFile>();
        }

        public void ProcessFiles()
        {
            for (var i = 0; i < Files.Count; i++)
            {
                if (Files[i] is PFSFile pfsFile)
                {
                    if (pfsFile.Name.EndsWith(".dds"))
                    {
                        try
                        {
                            // GD.Print("Need to process texture: ", pfsFile.Name);
                            var texture = ProcessDDSImage(pfsFile);
                            if (texture != null)
                            {
                                Files[i] = texture;
                                FilesByName[pfsFile.Name] = texture;
                            }
                        }
                        catch (Exception ex)
                        {
                            GD.PrintErr("Exception while processing ", pfsFile.Name, " ", ex);
                        }
                    }
                }
            }
            for (var i = 0; i < Files.Count; i++)
            {
                if (Files[i] is PFSFile pfsFile)
                {
                    if (pfsFile.Name.EndsWith(".bmp"))
                    {
                        try
                        {
                            //GD.Print("Need to process texture: ", pfsFile.Name);
                            var texture = ProcessBMPImage(pfsFile);
                            if (texture != null)
                            {
                                Files[i] = texture;
                                FilesByName[pfsFile.Name] = texture;
                            }
                        }
                        catch (Exception ex)
                        {
                            GD.PrintErr("Exception while processing ", pfsFile.Name, " ", ex);
                        }
                    }
                }
            }
            for (var i = 0; i < Files.Count; i++)
            {
                if (Files[i] is PFSFile pfsFile)
                {
                    if (pfsFile.Name.EndsWith(".wld"))
                    {
                        try
                        {

                            IsWldArchive = true;
                            var wld = ProcessWldResource(pfsFile);
                            Files[i] = wld;
                            FilesByName[pfsFile.Name] = wld;
                            WldFiles[pfsFile.Name] = wld;
                        }
                        catch (Exception ex)
                        {
                            GD.PrintErr("Exception while processing ", pfsFile.Name, " ", ex);
                        }
                    }
                }
            }
        }

        private ImageTexture ProcessDDSImage(PFSFile pfsFile)
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
                return ImageTexture.CreateFromImage(image);
            }
            catch (Exception ex)
            {
                GD.PrintErr("While processing ", pfsFile.Name, " an exception happened ", ex);
                return null;
            }
        }

        private ImageTexture ProcessBMPImage(PFSFile pfsFile)
        {
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
                return ImageTexture.CreateFromImage(image);
            }
            catch (Exception ex)
            {
                GD.PrintErr("While processing ", pfsFile.Name, " an exception happened ", ex);
                return null;
            }
        }

        private WldFile ProcessWldResource(PFSFile pfsFile)
        {
            return new WldFile(pfsFile, this);
        }

    }
}