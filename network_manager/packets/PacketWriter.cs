using System.Data.SqlTypes;
using System.IO;

namespace EQGodot2.network_manager.packets
{
    public class PacketWriter
    {
        private BinaryWriter Writer;
        private MemoryStream Stream;

        public PacketWriter()
        {
            Stream = new MemoryStream();
            Writer = new BinaryWriter(Stream);
        }

        public byte[] ToBytes()
        {
            return Stream.ToArray();
        }

        public void WriteByte(byte value)
        {
            Writer.Write(value);
        }

        public void WriteShort(short value)
        {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteUShort(ushort value)
        {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteInt(int value)
        {
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteUInt(uint value)
        {
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteBytes(byte[] value)
        {
            Writer.Write(value);
        }
    }
}