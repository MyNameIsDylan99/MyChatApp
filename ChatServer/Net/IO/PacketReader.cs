using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Net.IO {
    internal class PacketReader : BinaryReader {

        NetworkStream _ns;

        public PacketReader(NetworkStream ns) : base(ns) {
            _ns = ns;
        }

        public string ReadMessage() {

            byte[] msgBuffer;
            var length = ReadInt32();
            msgBuffer = new byte[length];
            _ns.Read(msgBuffer, 0, length);
            var msg = Encoding.ASCII.GetString(msgBuffer);

            return msg;
        }

        public byte[] ReadImage() {
            using (var ms = new MemoryStream()) {
                var length = ReadInt32();
                byte[] imageFormatBuffer = new byte[length];
                _ns.Read(imageFormatBuffer, 0, length);
                var imageByteLength = ReadInt32();
                byte[] imageBuffer = new byte[imageByteLength];

                var bytesRead = _ns.Read(imageBuffer, 0, imageByteLength);
                var byteIndex = bytesRead;
                while (byteIndex < imageByteLength) {
                    bytesRead = _ns.Read(imageBuffer, byteIndex, imageByteLength-byteIndex);
                    byteIndex += bytesRead;
                }


                ms.Write(BitConverter.GetBytes(length));
                ms.Write(imageFormatBuffer);
                ms.Write(BitConverter.GetBytes(imageByteLength));
                ms.Write(imageBuffer);
                return ms.ToArray();
            }

        }


    }
}
