using Godot;

namespace EQGodot.resource_manager.pack_file;

public partial class PFSFile : Resource
{
    public PFSFile(uint crc, uint size, uint offset, byte[] fileBytes)
    {
        Crc = crc;
        Size = size;
        Offset = offset;
        FileBytes = fileBytes;
    }

    public PFSFile()
    {
    }

    public uint Crc { get; }

    public uint Size { get; }

    public uint Offset { get; }

    public byte[] FileBytes { get; }

    public string Name { get; set; }
}