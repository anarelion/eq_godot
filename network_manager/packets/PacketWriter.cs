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

        public void WriteShortBE(short value)
        {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteShortLE(short value)
        {
            WriteByte((byte)value);
            WriteByte((byte)(value >> 8));
        }
        public void WriteUShortBE(ushort value)
        {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteIntBE(int value)
        {
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteUIntBE(uint value)
        {
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }

        public void WriteUIntLE(uint value)
        {
            WriteByte((byte)value);
            WriteByte((byte)(value >> 8));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 24));
        }

        public void WriteBytes(byte[] value)
        {
            Writer.Write(value);
        }
    }
}