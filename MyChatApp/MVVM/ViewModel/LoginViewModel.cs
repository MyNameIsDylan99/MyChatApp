using ChatClient.Net;
using MyChatApp.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MyChatApp.MVVM.ViewModel.MainViewModel;

namespace MyChatApp.MVVM.ViewModel {
    internal class LoginViewModel : ObservableObject {

        public string Username {get; set;}


        string profilePictureSource = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ProfilePictures/DefaultProfilePicture.png";
        public string ProfilePictureSource {
            get { return profilePictureSource; }
            set {
                profilePictureSource=value;
                OnPropertyChanged();
            }
        }
        public string LocalIPAddress { get { return GetLocalIPv4(NetworkInterfaceType.Wireless80211); } set { } }

        public Server Server;

        private string selectedServerIP;
        public string SelectedServerIP {
            get { return selectedServerIP; }
            set {
                selectedServerIP = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ServerIPsInLan { get; set; }

        public IEnumerable<ConnectionMethods> ConnectionMethodsValues {
            get {
                return Enum.GetValues(typeof(ConnectionMethods)).Cast<ConnectionMethods>();
            }
        }

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

        public RelayCommand ConnectToServerCommand { get; set; }

        public RelayCommand ChangeProfilePictureCommand { get; set; }

        public LoginViewModel() {

            Server = new Server();
            ServerIPsInLan = new ObservableCollection<string>();
            Server.FoundServerInSubnetEvent += OnFoundServerInSubnetEvent;
            LookForServerInLan();
            ConnectToServerCommand = new RelayCommand(o => Server.ConnectToServer(Username,ProfilePictureSource, connectionMethod, SelectedServerIP), o => !string.IsNullOrEmpty(Username));
            ChangeProfilePictureCommand = new RelayCommand(ChooseProfilePicture);
        }

        public void LookForServerInLan() {
            Server.SearchForServersInWlanSubnet();
        }

        void OnFoundServerInSubnetEvent(string serverIP) {
            Application.Current.Dispatcher.Invoke(() => {
                if (!ServerIPsInLan.Contains(serverIP)) {
                    ServerIPsInLan.Add(serverIP);
                }
            });
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

        private void ChooseProfilePicture(object sender) {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            Nullable<bool> result = dlg.ShowDialog();

            if (result == true) {
                // Open document 
                string sourceFile = dlg.FileName;
                string destinationFile = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ProfilePictures/" + dlg.SafeFileName;
                try {
                    File.Copy(sourceFile, destinationFile, true);
                }
                catch (Exception) {

                }
                finally {
                    ProfilePictureSource = destinationFile;
                }

            }

        }
    }
}
