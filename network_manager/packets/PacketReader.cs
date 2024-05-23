using System;
using System.Data.SqlTypes;
using System.IO;

namespace EQGodot2.network_manager.packets
{
    public class PacketReader
    {
        private BinaryReader Reader;
        public MemoryStream Stream;

        public PacketReader(byte[] bytes)
        {
            Stream = new MemoryStream(bytes);
            Reader = new BinaryReader(Stream);
        }

        public long Remaining()
        {
            return Stream.Length - Stream.Position;
        }

        public void Reset()
        {
            Stream.Position = 0;
        }

        public byte ReadByte()
        {
            return Reader.ReadByte();
        }

        public short ReadShort()
        {
            return (short)(ReadByte() << 8 | ReadByte());
        }

        public ushort ReadUShort()
        {
            return (ushort)(ReadByte() << 8 | ReadByte());
        }

        public int ReadInt()
        {
            return ReadByte() << 24 | ReadByte() << 16 | ReadByte() << 8 | ReadByte();
        }

        public uint ReadUInt()
        {
            return (uint)(ReadByte() << 24 | ReadByte() << 16 | ReadByte() << 8 | ReadByte());
        }

        public byte[] ReadBytes(int amount)
        {
            return Reader.ReadBytes(amount);
        }

        public string ReadString()
        {
            return Reader.ReadString();
        }
    }
}