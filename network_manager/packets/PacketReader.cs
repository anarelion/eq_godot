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

        public uint ReadUIntBE()
        {
            return (uint)(ReadByte() << 24 | ReadByte() << 16 | ReadByte() << 8 | ReadByte());
        }

        public uint ReadUIntLE()
        {
            return (uint)(ReadByte() | ReadByte() << 8 | ReadByte() << 16 | ReadByte() << 24);
        }

        public byte[] ReadBytes(long amount)
        {
            return Reader.ReadBytes((int)amount);
        }

        public string ReadString()
        {
            string s = "";
            byte c;
            while ((c = Reader.ReadByte()) != 0)
            {
                s += (char)c;
            }
            return s;
        }
    }
}