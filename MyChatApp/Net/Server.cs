using ChatClient.Net.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using static MyChatApp.MVVM.ViewModel.LoginViewModel;

namespace ChatClient.Net {
    internal class Server {

        public PacketReader PacketReader;

        TcpClient tcpClient;

        static UdpClient udpClient = new UdpClient();

        const int port = 11000;


        public string Guid;
        public bool GuidReceived => !string.IsNullOrEmpty(Guid);

        public event Action ConnectedEvent;
        public event Action UserDisconnectedEvent;
        public event Action MessageReceivedEvent;
        public event Action ServerShutdownEvent;
        public event Action<string> FoundServerInSubnetEvent;

        public enum OpCode : byte {
            NewClientConnected = 1,
            Guid = 2,
            Message = 5,
            ClientDisconnected = 10,
            ServerShutdown = 11
        }

        public Server() {

            tcpClient = new TcpClient();
            tcpClient.ReceiveBufferSize = 16384000;
            tcpClient.SendBufferSize = 16384000;
            udpClient.EnableBroadcast = true;

        }

        public void SearchForServersInWlanSubnet() {

            if(!udpClient.Client.IsBound)
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

            var from = new IPEndPoint(0, 0);
            Task.Run(() => {

                while (!tcpClient.Connected) {
                    var receiveBuffer = udpClient.Receive(ref from);
                    string receivedMessage = Encoding.ASCII.GetString(receiveBuffer);
                    if (receivedMessage.StartsWith("Apple")) {
                        string serverIP = from.Address.ToString();
                        FoundServerInSubnetEvent.Invoke(serverIP);
                        }
                    }
            });

        }

        System.Timers.Timer StartTimedMethod(int intervall, ElapsedEventHandler timedMethod) {
            System.Timers.Timer timer = new System.Timers.Timer(intervall);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += timedMethod;
            return timer;
        }

        public void ConnectToServer(string username,string profilePictureSource, ConnectionMethods connectionMethod, string selectedServerIp) {
            if (!tcpClient.Connected) {
                var ip="";
                switch (connectionMethod) {
                    case ConnectionMethods.Localhost:

                        ip = "127.0.0.1";

                        break;
                    case ConnectionMethods.SearchInLan:
                        ip = selectedServerIp;
                        break;
                }
                tcpClient.Connect(ip, port);
                PacketReader = new PacketReader(tcpClient.GetStream());

                if (!string.IsNullOrEmpty(username)) {

                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    connectPacket.WriteImage(profilePictureSource);
                    tcpClient.Client.Send(connectPacket.GetPacketBytes());
                }

                ReadPackets();

            }
        }

        void ReadPackets() {
            Task.Run(() => {
                bool readPackets = true;
                while (readPackets) {

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
                        case 11:
                            ServerShutdownEvent?.Invoke();
                            readPackets = false;
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
            messagePacket.WriteOpCode(OpCode.Message);
            messagePacket.WriteMessage(receiverGuid);
            messagePacket.WriteMessage(message);
            tcpClient.Client.Send(messagePacket.GetPacketBytes());
        }

        public string GetLocalIPAddress() {
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

