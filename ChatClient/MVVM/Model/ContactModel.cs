using System.Collections.ObjectModel;

namespace MyChatApp {
    internal class ContactModel : ObservableObject {
        public string Username { get; set; }
        public string ImageSource { get; set; }

        public ObservableCollection<SendableObject> Messages {
            get { return messages; }
            set { messages = value; }
        }

        ObservableCollection<SendableObject> messages = new ObservableCollection<SendableObject>();

        string lastMessage;
        public string LastMessage { get { return lastMessage; } set { lastMessage = value; OnPropertyChanged(); } }
        public string Guid { get; set; }



        public ContactModel(string username, string guid, string imageSource) {
            Username = username;
            ImageSource = imageSource;
            Guid = guid;
        }
    }
}