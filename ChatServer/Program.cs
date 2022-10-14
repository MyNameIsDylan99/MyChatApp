using ChatServer;
using ChatServer.Net.IO;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;

internal static class Program {

    public static ConcurrentDictionary<string,Client> clients = new ConcurrentDictionary<string,Client>();

    static TcpListener listener;

    static UdpClient udpClient;

    static string localIPAddress = GetLocalIPv4(NetworkInterfaceType.Wireless80211);

    static string localBroadcastAddress = localIPAddress.Substring(0, localIPAddress.Length-localIPAddress.IndexOf("."))+"255";

     const int port = 11000;

    public enum OpCode : byte {
        NewClientConnected = 1,
        Guid = 2,
        Message = 5,
        Picture=6,
        ClientDisconnected = 10,
        ServerShutdown = 11
    }

    static void Main(string[] args) {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(BroadcastServerShutdown);
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        StartTimedMethod(2000, BroadcastUdpPackets);
        AcceptTcpClients();
        

    }

    static void AcceptTcpClients() {

        while (true) {
            Client newClient = new Client(listener.AcceptTcpClient());
            SendClientOwnGuid(newClient);
            clients.TryAdd(newClient.Guid.ToString(),newClient);
            BroadcastConnection();
        }
    }

    static void BroadcastUdpPackets(object? sender, ElapsedEventArgs e) {
        byte[] broadCastMessage = Encoding.ASCII.GetBytes("Apple");
        udpClient.Send(broadCastMessage, new IPEndPoint(IPAddress.Parse(localBroadcastAddress), port));
        Console.WriteLine("Sent Upd Package");
    }

    static System.Timers.Timer StartTimedMethod(int intervall, ElapsedEventHandler timedMethod) {
        System.Timers.Timer timer = new System.Timers.Timer(intervall);
        timer.AutoReset = true;
        timer.Enabled = true;
        timer.Elapsed += timedMethod;
        return timer;
    }


    static void SendClientOwnGuid(Client client) {

        var clientGuidPacket = new PacketBuilder();
        clientGuidPacket.WriteOpCode(OpCode.Guid);
        clientGuidPacket.WriteMessage(client.Guid.ToString());
        client.TcpClient.Client.Send(clientGuidPacket.GetPacketBytes());
    }





    public static void BroadcastMessage(string message, string senderGuid) {

        foreach(var entry in clients) {

            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(OpCode.Message);
            messagePacket.WriteMessage(senderGuid);
            messagePacket.WriteMessage(message);

            entry.Value.TcpClient.Client.Send(messagePacket.GetPacketBytes());
        }


    }

    public static void BroadcastConnection() {
        foreach (var _client in clients) {
            foreach (var client in clients) {
                PacketBuilder broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(OpCode.NewClientConnected);
                broadcastPacket.WriteMessage(_client.Value.Username);
                broadcastPacket.WriteMessage(_client.Value.Guid.ToString());
                broadcastPacket.AddBytesToPacket(_client.Value.ProfileImgData);

                client.Value.TcpClient.GetStream().Write(broadcastPacket.GetPacketBytes());

            }
        }
    }

    public static void BroadcastDisconnection(string guid) {

        var disconnectedUser = RemoveDisconnectedUser(guid);

        foreach (var user in clients) {

            var broadcastPacket = new PacketBuilder();
            broadcastPacket.WriteOpCode(OpCode.ClientDisconnected);
            broadcastPacket.WriteMessage(guid);
            user.Value.TcpClient.Client.Send(broadcastPacket.GetPacketBytes());
        }
    }

    public static void BroadcastServerShutdown(object sender, EventArgs e) {
        foreach (var user in clients) {

            var broadcastPacket = new PacketBuilder();
            broadcastPacket.WriteOpCode(OpCode.ServerShutdown);
            user.Value.TcpClient.Client.Send(broadcastPacket.GetPacketBytes());
        }
    }

    static Client RemoveDisconnectedUser(string guid) {
        Client disconnectedUser = null;
        bool removedDisconnectedUser = false;
        while (!removedDisconnectedUser) {
            removedDisconnectedUser = clients.TryRemove(guid,out disconnectedUser); //TODO: FUCK CONCURRENTBAG
        }
        
        return disconnectedUser;
    }
    static string GetLocalIPv4(NetworkInterfaceType _type) {
        string output = "";
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) {
            if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up) {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses) {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
                        output = ip.Address.ToString();
                    }
                }
            }
        }
        return output;
    }

}







