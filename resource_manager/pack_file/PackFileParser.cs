using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Ionic.Zlib;

namespace EQGodot.resource_manager.pack_file;

public static class PackFileParser
{
    public static async Task<PfsArchive> Load(string path)
    {
        var content = await File.ReadAllBytesAsync(path);
        var fileLength = content.Length;
        var reader = new BinaryReader(new MemoryStream(content));

        var directoryOffset = reader.ReadInt32();
        var magicNumber = reader.ReadInt32();
        var version = reader.ReadInt32();

        reader.BaseStream.Position = directoryOffset;

        var fileCount = reader.ReadInt32();
        var fileNames = new List<string>();

        var files = new List<PFSFile>();

        for (var i = 0; i < fileCount; i++)
        {
            var crc = reader.ReadUInt32();
            var offset = reader.ReadUInt32();
            var size = reader.ReadUInt32();

            if (offset > fileLength)
            {
                GD.PrintErr("PfsArchive: Corrupted PFS length detected!");
                throw new Exception("PfsArchive: Corrupted PFS length detected!");
            }

            var cachedOffset = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;

            var fileBytes = new byte[size];
            var uncompressedTotal = 0;

            while (uncompressedTotal != size)
            {
                var compressedSize = reader.ReadInt32();
                var uncompressedSize = reader.ReadInt32();

                if (compressedSize >= fileLength)
                {
                    GD.PrintErr("PfsArchive: Corrupted file length detected! ", compressedSize, " >= ", fileLength);
                    throw new Exception("PfsArchive: Corrupted file length detected!");
                }

                var compressedBytes = reader.ReadBytes(compressedSize);
                if (!InflateBlock(compressedBytes, uncompressedSize, out var uncompressedBytes))
                {
                    GD.PrintErr("PfsArchive: Error occured inflating data");
                    throw new Exception("PfsArchive: Error occured inflating data");
                }

                uncompressedBytes.CopyTo(fileBytes, uncompressedTotal);
                uncompressedTotal += uncompressedSize;
            }

            if (crc == 0x61580AC9 || (crc == 0xFFFFFFFF && fileNames.Count == 0))
            {
                var dictionaryStream = new MemoryStream(fileBytes);
                var dictionary = new BinaryReader(dictionaryStream);
                var filenameCount = dictionary.ReadUInt32();

                for (uint j = 0; j < filenameCount; ++j)
                {
                    var fileNameLength = dictionary.ReadUInt32();
                    var filename = new string(dictionary.ReadChars((int)fileNameLength));
                    fileNames.Add(filename[..^1]);
                }

                reader.BaseStream.Position = cachedOffset;
                continue;
            }

            files.Add(new PFSFile(path, crc, size, offset, fileBytes));
            reader.BaseStream.Position = cachedOffset;
        }

        files.Sort((x, y) => x.Offset.CompareTo(y.Offset));

        var archive = new PfsArchive() {LoadedPath = path};
        foreach (var x in files) archive.Files.Add(x);

        for (var i = 0; i < files.Count; ++i)
        {
            switch (version)
            {
                case 0x10000:
                    // PFS version 1 files do not appear to contain the filenames
                    files[i].Name = $"{files[i].Crc:X8}.bin";
                    break;
                case 0x20000:
                    files[i].Name = fileNames[i];
                    break;
                default:
                    GD.PrintErr("PfsArchive: Unexpected pfs version: ", fileNames[i]);
                    throw new Exception("PfsArchive: Unexpected pfs version:");
            }

            files[i].ResourceName = files[i].Name;
        }
        
        return archive;
    }
    

    private static bool InflateBlock(byte[] deflatedBytes, int inflatedSize, out byte[] inflatedBytes)
    {
        var output = new byte[inflatedSize];

        using var memoryStream = new MemoryStream();
        var zlibCodec = new ZlibCodec();
        zlibCodec.InitializeInflate(true);

        zlibCodec.InputBuffer = deflatedBytes;
        zlibCodec.AvailableBytesIn = deflatedBytes.Length;
        zlibCodec.NextIn = 0;
        zlibCodec.OutputBuffer = output;

        foreach (var f in new[] { FlushType.None, FlushType.Finish })
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
            } while ((f == FlushType.None &&
                      (zlibCodec.AvailableBytesIn != 0 || zlibCodec.AvailableBytesOut == 0)) ||
                     (f == FlushType.Finish && bytesToWrite != 0));
        }

        zlibCodec.EndInflate();

        inflatedBytes = output;
        return true;
    }
}