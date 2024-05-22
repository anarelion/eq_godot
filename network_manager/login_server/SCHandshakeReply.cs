
using System;
using System.IO;
using System.Net.Http;
using EQGodot2.network_manager.network_session;
using Godot;

namespace EQGodot2.network_manager.login_server
{
    public class SCHandshakeReply : AppPacket
    {
        public string Message;
        
        public SCHandshakeReply(BinaryReader reader) : base(reader)
        {
        }

        public override ushort Opcode()
        {
            return 0x0017;
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }

        public override void Read()
        {
            Reader.ReadUInt16();
            Reader.ReadUInt16();
            Reader.ReadUInt16();
            Reader.ReadUInt32();
            Reader.ReadUInt32();
            Reader.ReadByte();
            Message = Reader.ReadString();
        }

    }
}
