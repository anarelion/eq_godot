
using System;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;

namespace EQGodot2.network_manager.login_server
{
    public class SCJoinServerReply(PacketReader reader) : AppPacket(reader)
    {
        public uint AccountId;
        public byte IsApproved;
        public ushort EQLSStr;
        public ushort ServerId;

        public override void Write()
        {
            throw new NotImplementedException();
        }

        public override void Read()
        {
            AccountId = Reader.ReadUIntLE();
            Reader.ReadUIntLE();
            Reader.ReadUShortLE();
            IsApproved = Reader.ReadByte();
            EQLSStr = Reader.ReadUShortLE();
            Reader.ReadByte();
            Reader.ReadUShortLE();
            ServerId = Reader.ReadUShortLE();
        }

    }
}
