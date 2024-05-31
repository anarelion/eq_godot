
using System;
using static System.Text.Encoding;
using EQGodot2.network_manager.network_session;
using EQGodot2.network_manager.packets;

namespace EQGodot2.network_manager.login_server
{
    public class CSPlayerLogin(string username, string password) : AppPacket
    {
        private readonly string Username = username;
        private readonly string Password = password;

        public override ushort Opcode()
        {
            return 0x0200;
        }

        public override void Write()
        {
            Writer.WriteShortBE(0);
            Writer.WriteShortBE(0);
            Writer.WriteShortBE(0);
            Writer.WriteShortBE(0);
            Writer.WriteShortBE(0);
            Writer.WriteBytes(Encrypt());
        }

        public override void Read()
        {
            throw new NotImplementedException();
        }

        byte[] Encrypt()
        {
            var tbuf = new byte[Username.Length + Password.Length + 2];
            Array.Copy(ASCII.GetBytes(Username), tbuf, Username.Length);
            Array.Copy(ASCII.GetBytes(Password), 0, tbuf, Username.Length + 1, Password.Length);
            tbuf[Username.Length] = 0;
            tbuf[Username.Length + Password.Length + 1] = 0;
            return Encrypt(tbuf);
        }

        byte[] Encrypt(byte[] buffer)
        {
            var empty = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var des = new DESCrypto(empty, empty);
            return des.Encrypt(buffer);
        }

    }
}
