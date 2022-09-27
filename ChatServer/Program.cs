using ChatServer;
using ChatServer.Net.IO;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

internal static class Program {

    public static List<Client> clients = new List<Client>();

    static TcpListener listener;

    static bool? useLocalHost = null;

    static void Main(string[] args) {

        while (useLocalHost == null) {
            Console.WriteLine("Do you want to use localhost? y/n");
            switch (Console.ReadLine()) {
                case "y":
                    useLocalHost = true;
                    Console.WriteLine("You are using localhost.");
                    break;
                case "n":
                    useLocalHost = false;
                    Console.WriteLine("You are not using localhost.");
                    break;
                default:
                    Console.WriteLine("Please answer with either y for yes or n for no.");
                    break;
            }
        }

        switch (useLocalHost) {

            case false:

                string localIpAdress;
                string strHostName = System.Net.Dns.GetHostName(); ;
                IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
                IPAddress[] addr = ipEntry.AddressList;
                localIpAdress = addr[addr.Length - 1].ToString(); //ipv4

                if (addr[0].AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
                    localIpAdress = addr[0].ToString(); //ipv6
                }

                listener = new TcpListener((IPAddress.Parse(localIpAdress)), 36);
                break;

            default:
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 36);
                break;
        }

        listener.Start();

        AcceptTcpClients();


    }

    static void AcceptTcpClients() {

        while (true) {
            Client newClient = new Client(listener.AcceptTcpClient());
            SendClientOwnGuid(newClient);
            clients.Add(newClient);
            BroadcastConnection();
        }
    }

     static void SendClientOwnGuid(Client client) {

        var clientGuidPacket = new PacketBuilder();
        clientGuidPacket.WriteOpCode(2);
        clientGuidPacket.WriteMessage(client.Guid.ToString()); ;
        client.TcpClient.Client.Send(clientGuidPacket.GetPacketBytes());
    }

    public static void SendMessage(string message, string senderGuid, string receiverGuid) {

        var receiverUser = clients.Where(x => x.Guid.ToString() == receiverGuid).First();

        var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(senderGuid);
            messagePacket.WriteMessage(message);

            receiverUser.TcpClient.Client.Send(messagePacket.GetPacketBytes());


    }



    public static void BroadcastMessage(string message, string senderGuid) {

        for (int i = 0; i < clients.Count; i++) {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(senderGuid);
            messagePacket.WriteMessage(message);

            clients[i].TcpClient.Client.Send(messagePacket.GetPacketBytes());
        }


    }

    public static void BroadcastConnection() {
        foreach (var _client in clients) {
            foreach (var client in clients) {
                PacketBuilder broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(1);
                broadcastPacket.WriteMessage(_client.Username);
                broadcastPacket.WriteMessage(_client.Guid.ToString());

                client.TcpClient.Client.Send(broadcastPacket.GetPacketBytes());

            }
        }
    }

    public static void BroadcastDisconnection(string guid) {

        var disconnectedUser = RemoveDisconnectedUser(guid);

        foreach (var user in clients) {

            var broadcastPacket = new PacketBuilder();
            broadcastPacket.WriteOpCode(10);
            broadcastPacket.WriteMessage(guid);
            user.TcpClient.Client.Send(broadcastPacket.GetPacketBytes());
        }
    }

    static Client RemoveDisconnectedUser(string guid) {
        var disconnectedUser = clients.Where(x => x.Guid.ToString() == guid).FirstOrDefault();
        clients.Remove(disconnectedUser);
        return disconnectedUser;
    }
}







