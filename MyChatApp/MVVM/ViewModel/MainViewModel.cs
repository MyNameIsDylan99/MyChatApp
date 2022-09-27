using ChatClient.Net;
using ModernChat.MVVM.Model;
using MyChatApp.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace MyChatApp.MVVM.ViewModel {
    internal class MainViewModel : ObservableObject {

        public string Username { get; set; }

        private string profilePicture = @"C:\Users\dylan\source\repos\MyChatApp\MyChatApp\ProfilePictures\DefaultProfilePicture.png";
        public string ProfilePicture {
            get => profilePicture; set {

                if (value != profilePicture) {
                    profilePicture = value;
                    OnPropertyChanged();
                }
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
        Server server;

        /* Commands */
        public RelayCommand SendMessageCommand { get; set; }

        public RelayCommand ConnectToServerCommand { get; set; }

        public MainViewModel() {

            Contacts = new ObservableCollection<ContactModel>();

            message = "";

            //Networking
            server = new Server();

            server.ConnectedEvent += UserConnected;
            server.UserDisconnectedEvent += RemoveUser;
            server.MessageReceivedEvent += MessageReceived;

            //Commands

            SendMessageCommand = new RelayCommand(o => SendMessage());

            ConnectToServerCommand = new RelayCommand(o => server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));

        }


        void UserConnected() {


            var connectedUser = new ContactModel(server.PacketReader.ReadMessage(), server.PacketReader.ReadMessage());


            if (!Contacts.Any(x => x.Guid == connectedUser.Guid) && connectedUser.Guid != server.Guid) {
                Application.Current.Dispatcher.Invoke(() => Contacts.Add(connectedUser));
                connectedUser.Messages.Add(new MessageModel(connectedUser.Username, $"I just connected and my Guid is {connectedUser.Guid} :)", connectedUser.ImageSource, DateTime.Now));
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
                server.SendMessageToServer(message,selectedContact.Guid);
                RemoveMessageText();
            }
        }

         void RemoveMessageText() {
            Message = "";
        }


    }
}

