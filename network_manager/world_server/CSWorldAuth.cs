using System;
using EQGodot2.network_manager.network_session;
using Godot;

namespace EQGodot2.network_manager.world_server
{
    class CSWorldAuth : AppPacket
    {
        private uint DbId;
        private byte[] key;

        public CSWorldAuth(uint DbId, byte[] key)
        {
            this.DbId = DbId;
            this.key = key;
        }

        public override void Write()
        {
            GD.Print($"Logging in {DbId} and key {key.HexEncode()}");
            Writer.WriteString($"{DbId}");
            Writer.WriteBytes(key);
            while (Writer.ToBytes().Length < 464)
            {
                Writer.WriteByte(0);
            }
        }

        public override void Read()
        {
            throw new NotImplementedException();
        }
    }
}