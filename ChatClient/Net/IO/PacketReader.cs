using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace MyChatApp {
    internal class PacketReader : BinaryReader {

        NetworkStream _ns;
        int pictureCount = 0;
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

        public string ReadAndSaveImage(string path) {
            string imagePath = "";


            var length = ReadInt32();
            byte[] imageFormatBuffer = new byte[length];
            _ns.Read(imageFormatBuffer, 0, length);
            var imageByteLength = ReadInt32();
            byte[] imageBuffer = new byte[imageByteLength];


            var bytesRead = _ns.Read(imageBuffer, 0, imageByteLength);
            var byteIndex = bytesRead;
            while (byteIndex < imageByteLength) {
                bytesRead = _ns.Read(imageBuffer, byteIndex, imageByteLength - byteIndex);
                byteIndex += bytesRead;
            }

            var imgFormatString = Encoding.ASCII.GetString(imageFormatBuffer);

            imagePath = path + DateTime.Now.Ticks.ToString().Replace(":", "_") + pictureCount.ToString() + "." + imgFormatString;


            using (MemoryStream stream = new MemoryStream(imageBuffer)) {

                Image.FromStream(stream).Save(imagePath);

            }

            pictureCount++;

            return imagePath;
        }

        public byte[] ReadImage() {

            var length = ReadInt32();
            byte[] imageFormatBuffer = new byte[length];
            _ns.Read(imageFormatBuffer, 0, length);
            var imageByteLength = ReadInt32();
            byte[] imageBuffer = new byte[imageByteLength];
            var bytesRead = _ns.Read(imageBuffer, 0, imageByteLength);
            var byteIndex = bytesRead;
            while (byteIndex < imageByteLength) {
                bytesRead = _ns.Read(imageBuffer, byteIndex, imageByteLength - byteIndex);
                byteIndex += bytesRead;
            }
            return imageBuffer;
        }

    }
}