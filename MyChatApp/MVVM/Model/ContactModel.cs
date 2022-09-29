using MyChatApp.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernChat.MVVM.Model {
    internal class ContactModel : ObservableObject {
        public string Username { get; set; }
        public string ImageSource { get; set; }

        public ObservableCollection<MessageModel> Messages {
            get { return messages; }
            set { messages = value; }
        }

        ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();

        string lastMessage;
        public string LastMessage { get { return lastMessage; } set { lastMessage = value; OnPropertyChanged(); } }
        public string Guid { get; set; }



        public ContactModel(string username, string guid) {
            Username = username;
            ImageSource = @"C:\Users\dylan\source\repos\MyChatApp\MyChatApp\ProfilePictures\DefaultProfilePicture.png";
            this.Guid = guid;
        }
    }

}
