﻿using ChatServer.Net.IO;
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
            packetReader.Dispose();
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
                            var receiverGuid = packetReader.ReadMessage();
                            var msg = packetReader.ReadMessage();
                            packetReader.Dispose();
                            var receiver = Program.clients.Where(x => x.Guid.ToString() == receiverGuid).FirstOrDefault();
                            Console.WriteLine($"{DateTime.Now} | {Username} sent: <<{msg}>> to {receiver.Username}");
                            Program.SendMessage(msg, Guid.ToString(),receiverGuid);
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

    }
}
