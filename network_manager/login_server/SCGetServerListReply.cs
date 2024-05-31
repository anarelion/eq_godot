
using System;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;
using Godot;

namespace EQGodot2.network_manager.login_server
{
    public class SCGetServerListReply(PacketReader reader) : AppPacket(reader)
    {
        public uint Count;
        public string[] Address;
        public uint[] ServerType;
        public uint[] Id;
        public string[] LongName;
        public string[] Language;
        public string[] Region;
        public uint[] Status;
        public uint[] Players;

        public override ushort Opcode()
        {
            return 0x1900;
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }

        public override void Read()
        {
            Reader.ReadUIntLE();
            Reader.ReadUIntLE();
            Reader.ReadUIntLE();
            Reader.ReadUIntLE();
            Count = Reader.ReadUIntLE();
            GD.Print($"Processing {Count} servers");
            Address = new string[Count];
            LongName = new string[Count];
            Language = new string[Count];
            Region = new string[Count];
            ServerType = new uint[Count];
            Id = new uint[Count];
            Status = new uint[Count];
            Players = new uint[Count];
            for (uint i = 0; i < Count; i++)
            {
                Address[i] = Reader.ReadString();
                ServerType[i] = Reader.ReadUIntLE();
                Id[i] = Reader.ReadUIntLE();
                LongName[i] = Reader.ReadString();
                Language[i] = Reader.ReadString();
                Region[i] = Reader.ReadString();
                Status[i] = Reader.ReadUIntLE();
                Players[i] = Reader.ReadUIntLE();
                GD.Print($"{Id[i]} {Address[i]} - {LongName[i]} - {Players[i]}");
            }
        }
    }
}
