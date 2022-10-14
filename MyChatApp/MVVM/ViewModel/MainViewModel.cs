using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Reflection;
using System.IO;

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
        public RelayCommand SendPictureCommand { get; set; }

        public MainViewModel() {

            Contacts = new ObservableCollection<ContactModel>();
            Message = "";

            //Commands
            SendMessageCommand = new RelayCommand(o => SendMessage());
            SendPictureCommand = new RelayCommand(o => SendPicture());
        }

        public void GetDataFromLoginViewModel(string username, string profilePictureSource, Server server) {
            this.Username = username;
            this.ProfilePictureSource = profilePictureSource;
            this.Server = server;

            server.ConnectedEvent += UserConnected;
            server.UserDisconnectedEvent += RemoveUser;
            server.MessageReceivedEvent += MessageReceived;
            server.PictureReceivedEvent += PictureReceived;
            server.ServerShutdownEvent += OnServerShutdown;
        }



        void UserConnected() {

            var username = Server.PacketReader.ReadMessage();
            var guid = Server.PacketReader.ReadMessage();
            bool userAlreadyExists = Contacts.Any(x => x.Guid.ToString() == guid) || guid == Server.Guid.ToString();

            if (!userAlreadyExists) {
                var profilePictureImgSource = Server.PacketReader.ReadAndSaveImage(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ProfilePictures\");
                var connectedUser = new ContactModel(username, guid, profilePictureImgSource);
                Application.Current.Dispatcher.Invoke(() => Contacts.Add(connectedUser));
                connectedUser.Messages.Add(new SendableObject(SendableObjectType.TextMessage,connectedUser.Username, "I just connected :)", connectedUser.ImageSource, DateTime.Now));
                connectedUser.LastMessage = connectedUser.Messages.Last().Content;
            }
            else {
                Server.PacketReader.ReadImage();
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

            var msgModel = new SendableObject(SendableObjectType.TextMessage,sender.Username, msg, sender.ImageSource, DateTime.Now);
            Application.Current.Dispatcher.Invoke(() => sender.Messages.Add(msgModel));
            Application.Current.Dispatcher.Invoke(() => sender.LastMessage = msg);

        }

        private void PictureReceived() {
            var GUID = Server.PacketReader.ReadMessage();
            var img = Server.PacketReader.ReadAndSaveImage(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ChatImages\");
            var sender = Contacts.Where(x => x.Guid == GUID).FirstOrDefault();

            var msgModel = new SendableObject(SendableObjectType.Picture, sender.Username, img, sender.ImageSource, DateTime.Now);
            Application.Current.Dispatcher.Invoke(() => sender.Messages.Add(msgModel));
            Application.Current.Dispatcher.Invoke(() => sender.LastMessage = "\"Image\"");
        }

        void SendMessage() {
            if (selectedContact != null && !string.IsNullOrEmpty(message)) {
                selectedContact.Messages.Add(new SendableObject(SendableObjectType.TextMessage,Username, message, ProfilePictureSource, DateTime.Now));
                Server.SendMessage(message, selectedContact.Guid);
                RemoveMessageText();
            }
        }

        void SendPicture() {

            if (selectedContact != null) {
                selectedContact.Messages.Add(new SendableObject(SendableObjectType.Picture, Username, message, ProfilePictureSource, DateTime.Now));
                Server.SendPicture(Utility.ChoosePicture(), selectedContact.Guid);
            }
            
        }

        void RemoveMessageText() {
            Message = "";
        }

    }
}

