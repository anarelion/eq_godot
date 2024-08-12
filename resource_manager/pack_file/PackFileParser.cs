
using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Ionic.Zlib;

namespace EQGodot.resource_manager.pack_file
{
    public class PackFileParser
    {
        static public PFSArchive Load(string path)
        {
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file.GetError() != Error.Ok)
            {
                GD.PrintErr("Failed to open: ", path);
                throw new Exception($"Failed to open {path}");
            }
            file.BigEndian = false;

            ulong fileLength = file.GetLength();

            var directoryOffset = file.Get32();
            var magicNumber = file.Get32();
            var version = file.Get32();

            file.Seek(directoryOffset);

            var fileCount = file.Get32();
            var fileNames = new List<string>();

            var files = new List<PFSFile>();

            for (int i = 0; i < fileCount; i++)
            {
                var crc = file.Get32();
                var offset = file.Get32();
                var size = file.Get32();

                if (offset > fileLength)
                {
                    GD.PrintErr("PfsArchive: Corrupted PFS length detected!");
                    throw new Exception("PfsArchive: Corrupted PFS length detected!");
                }

                ulong cachedOffset = file.GetPosition();
                file.Seek(offset);

                var fileBytes = new byte[size];
                uint uncompressedTotal = 0;

                while (uncompressedTotal != size)
                {
                    uint compressedSize = file.Get32();
                    uint uncompressedSize = file.Get32();

                    if (compressedSize >= fileLength)
                    {
                        GD.PrintErr("PfsArchive: Corrupted file length detected! ", compressedSize, " >= ", fileLength);
                        throw new Exception("PfsArchive: Corrupted file length detected!");
                    }

                    byte[] compressedBytes = file.GetBuffer(compressedSize);
                    byte[] uncompressedBytes;

                    if (!InflateBlock(compressedBytes, (int)uncompressedSize, out uncompressedBytes))
                    {
                        GD.PrintErr("PfsArchive: Error occured inflating data");
                        throw new Exception("PfsArchive: Error occured inflating data");
                    }

                    uncompressedBytes.CopyTo(fileBytes, uncompressedTotal);
                    uncompressedTotal += uncompressedSize;
                }

                if (crc == 0x61580AC9 || (crc == 0xFFFFFFFFU && fileNames.Count == 0))
                {
                    var dictionaryStream = new MemoryStream(fileBytes);
                    var dictionary = new BinaryReader(dictionaryStream);
                    uint filenameCount = dictionary.ReadUInt32();

                    for (uint j = 0; j < filenameCount; ++j)
                    {
                        uint fileNameLength = dictionary.ReadUInt32();
                        var filename = new string(dictionary.ReadChars((int)fileNameLength));
                        fileNames.Add(filename.Substring(0, filename.Length - 1));
                    }

                    file.Seek(cachedOffset);
                    continue;
                }

                files.Add(new PFSFile(crc, size, offset, fileBytes));
                file.Seek(cachedOffset);
            }

            files.Sort((x, y) => x.Offset.CompareTo(y.Offset));

            var archive = new PFSArchive();
            foreach (PFSFile x in files)
            {
                archive.Files.Add(x);
            }

            for (int i = 0; i < files.Count; ++i)
            {
                switch (version)
                {
                    case 0x10000:
                        // PFS version 1 files do not appear to contain the filenames
                        if (files[i] is PFSFile pfsFile)
                        {
                            pfsFile.Name = $"{pfsFile.Crc:X8}.bin";
                        }
                        break;
                    case 0x20000:
                        files[i].Name = fileNames[i];
                        archive.FilesByName[fileNames[i]] = files[i];

                        if (fileNames[i].EndsWith(".wld"))
                        {
                            archive.IsWldArchive = true;
                        }
                        break;
                    default:
                        GD.PrintErr("PfsArchive: Unexpected pfs version: ", fileNames[i]);
                        throw new Exception("PfsArchive: Unexpected pfs version:");
                }
                files[i].ResourceName = files[i].Name;
            }
            file.Close();

            GD.Print($"PfsArchive: Finished initialization of archive: {path}");
            archive.ProcessFiles();
            return archive;
        }

        private static bool InflateBlock(byte[] deflatedBytes, int inflatedSize, out byte[] inflatedBytes)
        {
            var output = new byte[inflatedSize];

            using (var memoryStream = new MemoryStream())
            {
                var zlibCodec = new ZlibCodec();
                zlibCodec.InitializeInflate(true);

                zlibCodec.InputBuffer = deflatedBytes;
                zlibCodec.AvailableBytesIn = deflatedBytes.Length;
                zlibCodec.NextIn = 0;
                zlibCodec.OutputBuffer = output;

                foreach (FlushType f in new[] { FlushType.None, FlushType.Finish })
                {
                    int bytesToWrite;

                    do
                    {
                        zlibCodec.AvailableBytesOut = inflatedSize;
                        zlibCodec.NextOut = 0;
                        try
                        {
                            zlibCodec.Inflate(f);
                        }
                        catch (Exception e)
                        {
                            inflatedBytes = null;
                            GD.PrintErr("PfsArchive: Exception caught while inflating bytes: " + e);
                            throw new Exception($"PfsArchive: Exception caught while inflating bytes: {e}");
                        }

                        bytesToWrite = inflatedSize - zlibCodec.AvailableBytesOut;
                        if (bytesToWrite > 0)
                            memoryStream.Write(output, 0, bytesToWrite);
                    }
                    while (f == FlushType.None &&
                             (zlibCodec.AvailableBytesIn != 0 || zlibCodec.AvailableBytesOut == 0) ||
                             f == FlushType.Finish && bytesToWrite != 0);
                }

                zlibCodec.EndInflate();

                inflatedBytes = output;
                return true;
            }
        }
    }
}