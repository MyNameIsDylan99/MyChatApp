using ChatClient.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Documents;
using System.Windows.Markup;
using static MyChatApp.MVVM.ViewModel.MainViewModel;

namespace ChatClient.Net {
    internal class Server {

        public PacketReader PacketReader;

        TcpClient tcpClient;
        UdpClient udpClient;

        int port = 36;


        public string Guid;
        public bool GuidReceived => !string.IsNullOrEmpty(Guid);

        public event Action ConnectedEvent;
        public event Action UserDisconnectedEvent;
        public event Action MessageReceivedEvent;

        public Server() {

            tcpClient = new TcpClient();
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

        }
        void BroadcastServerRequestInLan(object? sender, ElapsedEventArgs e) {
            byte[] serverRequestMessage = Encoding.ASCII.GetBytes("Apple");
            udpClient.Send(serverRequestMessage, serverRequestMessage.Length, "255.255.255.255", port);
        }

        System.Timers.Timer StartTimedMethod(int intervall, ElapsedEventHandler timedMethod) {
                System.Timers.Timer timer = new System.Timers.Timer(intervall);
                timer.AutoReset = true;
                timer.Enabled = true;
                timer.Elapsed += timedMethod;
            return timer;
        }

        public void SearchForServersInLan(List<string> serverIPsInLan) {
            if (!udpClient.Client.IsBound) {
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port+1));
            }
            var timedUdpRequests=StartTimedMethod(2000, BroadcastServerRequestInLan);
            var from = new IPEndPoint(0, 0);
            var task = Task.Run(() => {
                while (!tcpClient.Connected) {
                    var receiveBuffer = udpClient.Receive(ref from);
                    string receivedMessage = Encoding.ASCII.GetString(receiveBuffer);
                    if (receivedMessage.StartsWith("Banana")) {
                        string serverIP = from.Address.ToString();
                        if (!serverIPsInLan.Contains(serverIP))
                            serverIPsInLan.Add(serverIP);
                    }
                }
                udpClient.Close();
                timedUdpRequests.Stop();
            });
        }

        public void ConnectToServer(string username, ConnectionMethods connectionMethod, string selectedServerIp) {
            if (!tcpClient.Connected) {

                switch (connectionMethod) {
                    case ConnectionMethods.Localhost:

                        tcpClient.Connect("127.0.0.1", port);

                        break;
                    case ConnectionMethods.SearchInLan:
                        tcpClient.Connect(selectedServerIp, port);
                        break;
                }
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

        public void SendMessageToServer(string message, string receiverGuid) {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(receiverGuid);
            messagePacket.WriteMessage(message);
            tcpClient.Client.Send(messagePacket.GetPacketBytes());
        }

        public  string GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

    }
}

