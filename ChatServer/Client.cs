using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace ChatServer {
    internal class Client {

        public string Username { get; set; }

        public Guid Guid { get; set; }

        public byte[] ProfileImgData { get; set; }

        const int receiveAndSendBufferSize = 500000000;

        public TcpClient TcpClient { get; set; }

         PacketReader packetReader;

        public Client(TcpClient tcpClient) { 
            this.TcpClient = tcpClient;
            tcpClient.ReceiveBufferSize = receiveAndSendBufferSize;
            tcpClient.SendBufferSize = receiveAndSendBufferSize;
            Guid = Guid.NewGuid();
            packetReader = new PacketReader(TcpClient.GetStream());

            var opcode = packetReader.ReadByte();
            Username = packetReader.ReadMessage();
            ProfileImgData = packetReader.ReadImage();
            Console.WriteLine($"{DateTime.Now}: Client {Username} with guid: {Guid.ToString()} has connected.");

            Task.Run(Process);

        }


        void Process() {
            while (true) {
                try {
                    var opcode = packetReader.ReadByte();
                    var opCodeAsEnum = (OpCode)opcode;
                    switch (opCodeAsEnum) {
                        case OpCode.Message:
                            OnMessageReceived();
                            break;
                        case OpCode.Picture:
                            OnPictureReceived();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception) {
                    Console.WriteLine($"{Guid}: Disconnected!");
                    Program.BroadcastDisconnection(Guid.ToString());
                    TcpClient.Close();
                    break;
                }
            }
        }

        private void OnMessageReceived() {
            var receiverGuid = packetReader.ReadMessage();
            var msg = packetReader.ReadMessage();
            var receiver = Program.clients[receiverGuid];
            Console.WriteLine($"{DateTime.Now} | {Username} sent: <<{msg}>> to {receiver.Username}");
            SendMessage(msg, Guid.ToString(), receiver);
        }
          void SendMessage(string message, string senderGuid, Client receiver) {

            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(OpCode.Message);
            messagePacket.WriteMessage(senderGuid);
            messagePacket.WriteMessage(message);

            receiver.TcpClient.Client.Send(messagePacket.GetPacketBytes());


        }

        private void OnPictureReceived() {
            var receiverGuid = packetReader.ReadMessage();
            var img = packetReader.ReadImage();
            var receiver = Program.clients[receiverGuid];
            Console.WriteLine($"{DateTime.Now} | {Username} sent: <<A picture containing {img.Length} bytes>> to {receiver.Username}");
            SendImage(img, Guid.ToString(), receiver);
        }
        void SendImage(byte[] image, string senderGuid, Client receiver) {

            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(OpCode.Picture);
            messagePacket.WriteMessage(receiver.Guid.ToString());
            messagePacket.AddBytesToPacket(image);

            receiver.TcpClient.Client.Send(messagePacket.GetPacketBytes());


        }

    }
}
