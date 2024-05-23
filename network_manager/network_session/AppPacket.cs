
using System;
using System.IO;
using EQGodot2.network_manager.packets;

namespace EQGodot2.network_manager.network_session
{
    // Latern Extractor class
    public abstract class AppPacket
    {
        public PacketReader Reader;

        protected PacketWriter Writer;

        public AppPacket() {
            Writer = new PacketWriter();
        }

        public AppPacket(PacketReader reader) {
            Reader = reader;
            Read();
        }

        public byte[] ToBytes() {
            if (Writer == null) {
                throw new NotImplementedException();
            }
            Writer.WriteUShort(Opcode());
            Write();
            var result = Writer.ToBytes();
            Writer = new PacketWriter();
            return result;
        }

        public abstract void Write();
        public abstract void Read();
        public abstract ushort Opcode();

    }
}
