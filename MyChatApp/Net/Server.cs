using MyChatApp;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public enum OpCode : byte {
    NewClientConnected = 1,
    Guid = 2,
    Message = 5,
    Picture = 6,
    ClientDisconnected = 10,
    ServerShutdown = 11
}
namespace MyChatApp { 
internal class Server {

        public PacketReader PacketReader;

        TcpClient tcpClient;

        static UdpClient udpClient = new UdpClient();

        const int port = 11000;

        const int receiveAndSendBufferSize = 500000000;

        public string Guid;
        public bool GuidReceived => !string.IsNullOrEmpty(Guid);

        public event Action ConnectedEvent;
        public event Action UserDisconnectedEvent;
        public event Action MessageReceivedEvent;
        public event Action PictureReceivedEvent;
        public event Action ServerShutdownEvent;
        public event Action<string> FoundServerInSubnetEvent;

        public Server() {

            tcpClient = new TcpClient();
            tcpClient.ReceiveBufferSize = receiveAndSendBufferSize;
            tcpClient.SendBufferSize = receiveAndSendBufferSize;
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
                    NetworkStream ns = tcpClient.GetStream();
                    ns.Write(connectPacket.GetPacketBytes());
                    //tcpClient.Client.Send(connectPacket.GetPacketBytes());
                }

                ReadPackets();

            }
        }

        void ReadPackets() {
            Task.Run(() => {
                bool readPackets = true;
                while (readPackets) {

                    var opcode = PacketReader.ReadByte();
                    var opCodeAsEnum = (OpCode)opcode;


                    switch (opCodeAsEnum) {
                        case OpCode.NewClientConnected:
                            ConnectedEvent?.Invoke();
                            break;
                        case OpCode.Guid:
                            Guid = PacketReader.ReadMessage();
                            break;
                        case OpCode.Message:
                            MessageReceivedEvent?.Invoke();
                            break;
                        case OpCode.Picture:
                            PictureReceivedEvent?.Invoke();
                            break;
                        case OpCode.ClientDisconnected:
                            UserDisconnectedEvent?.Invoke();
                            break;
                        case OpCode.ServerShutdown:
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

        public void SendMessage(string message, string receiverGuid) {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(OpCode.Message);
            messagePacket.WriteMessage(receiverGuid);
            messagePacket.WriteMessage(message);
            tcpClient.Client.Send(messagePacket.GetPacketBytes());
        }

        public void SendPicture(string picture, string receiverGuid) {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(OpCode.Picture);
            messagePacket.WriteMessage(receiverGuid);
            messagePacket.WriteImage(picture);
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
