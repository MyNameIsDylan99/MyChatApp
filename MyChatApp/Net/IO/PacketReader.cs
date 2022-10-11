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

        public string ReadImage() {
            string imagePath = "";


            var length = ReadInt32();
            byte[] imageFormatBuffer = new byte[length];
            _ns.Read(imageFormatBuffer, 0, length);
            var imageByteLength = ReadInt32();
            byte[] imageBuffer = new byte[imageByteLength];
            while (_ns.DataAvailable) {
                _ns.Read(imageBuffer, 0, imageByteLength);
            }

            //MemoryStream ms = new MemoryStream(imageBuffer);

            var imgFormatString = Encoding.ASCII.GetString(imageFormatBuffer);
            //var img = Image.FromStream(ms);

            //var imgFormat = ImageFormat.Png;
            //switch (imgFormatString) {
            //    case "png":
            //        imgFormat = ImageFormat.Png;
            //        break;
            //    case "jpeg":
            //        imgFormat = ImageFormat.Jpeg;
            //        break;
            //    case "gif":
            //        imgFormat = ImageFormat.Gif;
            //        break;
            //    case "jpg":
            //        imgFormat = ImageFormat.Jpeg;
            //        break;

            //}

            imagePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ProfilePictures\" + DateTime.Now.ToString().Replace(":", "_") + "." + imgFormatString;

            BinaryWriter bWriter = new BinaryWriter(File.Open(imagePath, FileMode.Create));
            bWriter.Write(imageBuffer);
            bWriter.Close();

            //img.Save(imagePath, imgFormat);
            return imagePath;
        }

    }
}
