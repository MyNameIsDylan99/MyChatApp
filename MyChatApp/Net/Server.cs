using ChatClient.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net {
    internal class Server {

        public PacketReader PacketReader;

        TcpClient tcpClient;

        public string Guid;
        public bool GuidReceived => !string.IsNullOrEmpty(Guid);

        public event Action ConnectedEvent;
        public event Action UserDisconnectedEvent;
        public event Action MessageReceivedEvent;

        public Server() {

            tcpClient = new TcpClient();

        }

        public void ConnectToServer(string username) {
            if (!tcpClient.Connected) {

                    tcpClient.Connect("127.0.0.1", 36);

                PacketReader = new PacketReader(tcpClient.GetStream());

                if (!string.IsNullOrEmpty(username)) {

                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    tcpClient.Client.Send(connectPacket.GetPacketBytes());
                }

                ReadPackets();

            }
        }

        void ReadPackets() {
            Task.Run(() => {
                while (true) {
                    var opcode = PacketReader.ReadByte();

                    switch (opcode) {
                        case 1:
                            ConnectedEvent?.Invoke();
                            break;
                            case 2:
                            Guid = PacketReader.ReadMessage();
                            break;
                        case 5:
                            MessageReceivedEvent?.Invoke();
                            break;
                        case 10:
                            UserDisconnectedEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("ah yes..");
                            break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message,string receiverGuid) {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(receiverGuid);
            messagePacket.WriteMessage(message);
            tcpClient.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}

