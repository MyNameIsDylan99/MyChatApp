using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace ChatServer.Net.IO {
    internal class PacketBuilder {

        MemoryStream _ms;

        public PacketBuilder() {
            _ms = new MemoryStream();
            _ms.Capacity = 5000000;
        }

        public void WriteOpCode(OpCode opCode) {
            var opCodeAsByte = (byte)opCode;
            _ms.WriteByte(opCodeAsByte);
        }

        public void WriteMessage(string msg) {
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }

        public void AddBytesToPacket(byte[] bytes) {
            _ms.Write(bytes);
        }

        public byte[] GetPacketBytes() {
            return _ms.ToArray();
        }

    }
}
