using ModernChat.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChatApp.MVVM.ViewModel {
    internal class MainViewModel {
        public ObservableCollection<MessageModel> Messages { get; set; }

        public ObservableCollection<ContactModel> Contacts { get; set; }


        public MainViewModel() { 
        
            Messages = new ObservableCollection<MessageModel>();
            Contacts = new ObservableCollection<ContactModel>();

            for (int i = 0; i < 5; i++) {
                Contacts.Add(new ContactModel());
                Contacts[i].Username = "Martin";
            }
        
        }
    }
}
