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
            Writer.Write(value);
        }

        public void WriteUShort(ushort value)
        {
            Writer.Write(value);
        }

        public void WriteInt(int value)
        {
            Writer.Write(value);
        }

        public void WriteUInt(uint value)
        {
            Writer.Write(value);
        }

        public void WriteBytes(byte[] value)
        {
            Writer.Write(value);
        }        
    }
}