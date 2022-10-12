using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net.IO {
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

        public string ReadAndSaveImage() {
            string imagePath = "";


            var length = ReadInt32();
            byte[] imageFormatBuffer = new byte[length];
            _ns.Read(imageFormatBuffer, 0, length);
            var imageByteLength = ReadInt32();
            byte[] imageBuffer = new byte[imageByteLength];
            var bytesRead = 0;
            while (bytesRead < imageByteLength) {
                bytesRead += _ns.Read(imageBuffer, 0, imageByteLength);
            }


            var imgFormatString = Encoding.ASCII.GetString(imageFormatBuffer);

            imagePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ProfilePictures\" + DateTime.Now.Ticks.ToString().Replace(":", "_") + pictureCount.ToString() + "." + imgFormatString;


            using (MemoryStream stream = new MemoryStream(imageBuffer)) {

                Image.FromStream(stream).Save(imagePath);

            }

            pictureCount++;

            return imagePath;
        }

        public void ReadImageAndDoNothingWithIt() {

            var length = ReadInt32();
            byte[] imageFormatBuffer = new byte[length];
            _ns.Read(imageFormatBuffer, 0, length);
            var imageByteLength = ReadInt32();
            byte[] imageBuffer = new byte[imageByteLength];
            var bytesRead = 0;
            while (bytesRead < imageByteLength) {
                bytesRead += _ns.Read(imageBuffer, 0, imageByteLength);
            }
        }

    }
}
