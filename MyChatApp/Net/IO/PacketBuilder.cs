using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interop;
using static ChatClient.Net.Server;

namespace ChatClient.Net.IO {
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

        public void WriteImage(string imgSource) {
            byte[] imgData = System.IO.File.ReadAllBytes(imgSource);
            var imgByteLength = imgData.Length;
            var imgTypeAsString = imgSource.Substring(imgSource.LastIndexOf('.') + 1);
            var imgTypeAsStringLength = imgTypeAsString.Length;


            _ms.Write(BitConverter.GetBytes(imgTypeAsStringLength));
            _ms.Write(Encoding.ASCII.GetBytes(imgTypeAsString));
            _ms.Write(BitConverter.GetBytes(imgByteLength));
            _ms.Write(imgData);
        }

        public byte[] GetPacketBytes() {
            return _ms.ToArray();
        }

    }
}
