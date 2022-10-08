using ChatClient.Net;
using ModernChat.MVVM.Model;
using MyChatApp.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace MyChatApp.MVVM.ViewModel {
    internal class MainViewModel : ObservableObject {


        public string Username { get; set; }

        private string profilePicture = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ProfilePictures/DefaultProfilePicture.png";
        public string ProfilePicture {
            get => profilePicture; set {

                if (value != profilePicture) {
                    profilePicture = value;
                    OnPropertyChanged();
                }
            }
        }


        public IEnumerable<ConnectionMethods> ConnectionMethodsValues {
            get {
                return Enum.GetValues(typeof(ConnectionMethods)).Cast<ConnectionMethods>();
            }
        }

        public ObservableCollection<ContactModel> Contacts { get; set; }

        private ContactModel? selectedContact;

        public ContactModel SelectedContact {

            get { return selectedContact; }
            set {
                selectedContact = value;
                OnPropertyChanged();
            }
        }

        private string message;

        public string Message {
            get { return message; }
            set {

                message = value;
                OnPropertyChanged();
            }

        }

        //Networking
        public enum ConnectionMethods {
            Localhost,
            SearchInLan
        }

        private ConnectionMethods connectionMethod = ConnectionMethods.Localhost;
        public ConnectionMethods ConnectionMethod {

            get { return connectionMethod; }
            set {
                connectionMethod = value;
                OnPropertyChanged();
            }
        }

        Server server;

        public ObservableCollection<string> ServerIPsInLan { get; set; }


        private string selectedServerIP;
        public string SelectedServerIP {
            get { return selectedServerIP; }
            set {
                selectedServerIP = value;
                OnPropertyChanged();
            }
        }

        public string LocalIPAddress { get { return GetLocalIPv4(NetworkInterfaceType.Wireless80211); } set { } }

        /* Commands */
        public RelayCommand SendMessageCommand { get; set; }

        public RelayCommand ConnectToServerCommand { get; set; }

        public MainViewModel() {

            Contacts = new ObservableCollection<ContactModel>();
            ServerIPsInLan = new ObservableCollection<string>();
            message = "";
            for (int i = 0; i < 10; i++) {
                if (!ServerIPsInLan.Contains(i.ToString())) {
                    ServerIPsInLan.Add(i.ToString());
                }
            }
            
            //Networking
            server = new Server();

            server.ConnectedEvent += UserConnected;
            server.UserDisconnectedEvent += RemoveUser;
            server.MessageReceivedEvent += MessageReceived;
            server.FoundServerInSubnetEvent += OnFoundServerInSubnetEvent;

            //Commands

            SendMessageCommand = new RelayCommand(o => SendMessage());

            ConnectToServerCommand = new RelayCommand(o => server.ConnectToServer(Username, connectionMethod, SelectedServerIP), o => !string.IsNullOrEmpty(Username));

        }

        string GetLocalIPv4(NetworkInterfaceType _type) {
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

        void UserConnected() {

            var username = server.PacketReader.ReadMessage();
            var guid = server.PacketReader.ReadMessage();

            var connectedUser = new ContactModel(username, guid);


            if (!Contacts.Any(x => x.Guid == connectedUser.Guid) && connectedUser.Guid != server.Guid) {
                Application.Current.Dispatcher.Invoke(() => Contacts.Add(connectedUser));
                connectedUser.Messages.Add(new MessageModel(connectedUser.Username, "Hello :)", connectedUser.ImageSource, DateTime.Now));
                connectedUser.LastMessage = connectedUser.Messages.Last().Message;
            }

        }

        void RemoveUser() {
            var uid = server.PacketReader.ReadMessage();
            var user = Contacts.Where(x => x.Guid == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Contacts.Remove(user));
        }

        void MessageReceived() {
            var GUID = server.PacketReader.ReadMessage();
            var msg = server.PacketReader.ReadMessage();
            var sender = Contacts.Where(x => x.Guid == GUID).FirstOrDefault();

            var msgModel = new MessageModel(sender.Username, msg, sender.ImageSource, DateTime.Now);
            Application.Current.Dispatcher.Invoke(() => sender.Messages.Add(msgModel));
            Application.Current.Dispatcher.Invoke(() => sender.LastMessage = msg);

        }

        void SendMessage() {
            if (selectedContact != null && !string.IsNullOrEmpty(message)) {
                selectedContact.Messages.Add(new MessageModel(Username, message, ProfilePicture, DateTime.Now));
                server.SendMessageToServer(message, selectedContact.Guid);
                RemoveMessageText();
            }
        }

        void RemoveMessageText() {
            Message = "";
        }

        public void LookForServerInLan() {
            server.SearchForServersInWlanSubnet();
        }

        void OnFoundServerInSubnetEvent(string serverIP) {
            Application.Current.Dispatcher.Invoke(() => {
                if (!ServerIPsInLan.Contains(serverIP)) {
                    ServerIPsInLan.Add(serverIP);
                }
            });
        }
    }
}

