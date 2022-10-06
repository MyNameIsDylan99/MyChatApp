using ChatClient.Net.IO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net {
    internal class Server {

        public PacketReader PacketReader;

        TcpClient tcpClient;
        UdpClient udpClient;

        public bool UseLocalhost;
        public string serverIPInLan;

        public string Guid;
        public bool GuidReceived => !string.IsNullOrEmpty(Guid);

        public event Action ConnectedEvent;
        public event Action UserDisconnectedEvent;
        public event Action MessageReceivedEvent;

        public Server() {

            tcpClient = new TcpClient();
            udpClient = new UdpClient();

        }

        public void ListenForServerInLan() {
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 36));

            var from = new IPEndPoint(0, 0);
            var task = Task.Run(() => {
                while (!tcpClient.Client.Connected) {
                    var receiveBuffer = udpClient.Receive(ref from);

                    if (serverIPInLan == null) {
                        serverIPInLan = Encoding.ASCII.GetString(receiveBuffer);
                    }
                }
            });
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

