﻿using ChatServer;
using ChatServer.Net.IO;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

internal static class Program {

    public static List<Client> clients = new List<Client>();

    static TcpListener listener;

    static UdpClient udpClient;


    static int port = 11000;

    static void Main(string[] args) {

        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Task.Run(AcceptTcpClients);
        ListenForUdpRequests();

    }

    static void ListenForUdpRequests() {
        var from = new IPEndPoint(0,0);
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        while (true) {
            var recvBuffer = udpClient.Receive(ref from);
            string receivedMessage = Encoding.ASCII.GetString(recvBuffer);
            if (receivedMessage == "Apple") {
                Console.WriteLine("Received Udp message");
                byte[] buffer = Encoding.ASCII.GetBytes("Banana");
                Console.WriteLine(from.ToString());
                udpClient.Send(buffer, new IPEndPoint(IPAddress.Parse("192.168.1.255"),port+1));
            }
        }

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







