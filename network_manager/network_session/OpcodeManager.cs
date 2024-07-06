using EQGodot2.network_manager.packets;
using Godot;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Xml.XPath;

namespace EQGodot2.network_manager.network_session
{
    public class OpcodeManager
    {
        private Dictionary<ushort, Type> OpcodeToClass = [];
        private Dictionary<Type, ushort> ClassToOpcode = [];

        public void Register<T>(ushort opcode) where T : AppPacket
        {
            Register(opcode, typeof(T));
        }

        private void Register(ushort opcode, Type type)
        {
            GD.Print($"Registered {opcode} of type {type}");
            OpcodeToClass[opcode] = type;
            ClassToOpcode[type] = opcode;
        }

        public AppPacket Decode(PacketReader reader)
        {
            var opcode = reader.ReadUShortLE();
            if (!OpcodeToClass.ContainsKey(opcode)) {
                GD.PrintErr($"Unknown opcode received {opcode:X}");
            }
            var type = OpcodeToClass[opcode];
            return (AppPacket)Activator.CreateInstance(type, new object[] { reader });
        }

        public byte[] Encode(PacketWriter writer, AppPacket packet) {
            ushort opcode = ClassToOpcode[packet.GetType()];
            writer.WriteUShortLE(opcode);
            writer.WriteBytes(packet.ToBytes());
            GD.Print($"Sending {writer.ToBytes().Length} {writer.ToBytes().HexEncode()}");
            return writer.ToBytes();
        }
    }
}