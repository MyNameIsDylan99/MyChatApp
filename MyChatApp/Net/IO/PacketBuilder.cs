using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interop;


namespace ChatClient.Net.IO {
    internal class PacketBuilder {

        MemoryStream _ms;

        public PacketBuilder() {
            _ms = new MemoryStream();
        }

        public void WriteOpCode(byte opcode) {
            _ms.WriteByte(opcode);
        }

        public void WriteMessage(string msg) {
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }

        public void WriteImage(string imgSource) {
            byte[] imgData = System.IO.File.ReadAllBytes(imgSource);
            var imgByteLength = imgData.Length;
            var imgTypeAsString = imgSource.Substring(imgSource.IndexOf('.') + 1);
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
