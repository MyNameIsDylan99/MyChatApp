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

        string username;
        public string Username {
            get => username; 
            set {
                    username = value;
                    OnPropertyChanged();
            }
        }

        private string profilePictureSource;
        public string ProfilePictureSource {
            get => profilePictureSource; set {

                if (value != profilePictureSource) {
                    profilePictureSource = value;
                    OnPropertyChanged();
                }
            }
        }

        public Action OnServerShutdown;

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

        public Server Server;

        /* Commands */
        public RelayCommand SendMessageCommand { get; set; }

        public MainViewModel() {

            Contacts = new ObservableCollection<ContactModel>();
            Message = "";

            //Commands
            SendMessageCommand = new RelayCommand(o => SendMessage());
        }

        public void GetDataFromLoginViewModel(string username, string profilePictureSource, Server server) {
            this.Username = username;
            this.ProfilePictureSource = profilePictureSource;
            this.Server = server;

            server.ConnectedEvent += UserConnected;
            server.UserDisconnectedEvent += RemoveUser;
            server.MessageReceivedEvent += MessageReceived;
            server.ServerShutdownEvent += OnServerShutdown;
        }

        void UserConnected() {

            var username = Server.PacketReader.ReadMessage();
            var guid = Server.PacketReader.ReadMessage();
            var profilePictureImgSource = Server.PacketReader.ReadImage();
            var connectedUser = new ContactModel(username, guid, profilePictureImgSource);


            if (!Contacts.Any(x => x.Guid == connectedUser.Guid) && connectedUser.Guid != Server.Guid) {
                Application.Current.Dispatcher.Invoke(() => Contacts.Add(connectedUser));
                connectedUser.Messages.Add(new MessageModel(connectedUser.Username, "I just connected :)", connectedUser.ImageSource, DateTime.Now));
                connectedUser.LastMessage = connectedUser.Messages.Last().Message;
            }

        }

        void RemoveUser() {
            var uid = Server.PacketReader.ReadMessage();
            var user = Contacts.Where(x => x.Guid == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Contacts.Remove(user));
        }

        void MessageReceived() {
            var GUID = Server.PacketReader.ReadMessage();
            var msg = Server.PacketReader.ReadMessage();
            var sender = Contacts.Where(x => x.Guid == GUID).FirstOrDefault();

            var msgModel = new MessageModel(sender.Username, msg, sender.ImageSource, DateTime.Now);
            Application.Current.Dispatcher.Invoke(() => sender.Messages.Add(msgModel));
            Application.Current.Dispatcher.Invoke(() => sender.LastMessage = msg);

        }

        void SendMessage() {
            if (selectedContact != null && !string.IsNullOrEmpty(message)) {
                selectedContact.Messages.Add(new MessageModel(Username, message, ProfilePictureSource, DateTime.Now));
                Server.SendMessageToServer(message, selectedContact.Guid);
                RemoveMessageText();
            }
        }

        void RemoveMessageText() {
            Message = "";
        }

    }
}

