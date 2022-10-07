using MyChatApp.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyChatApp.MVVM.View {
    /// <summary>
    /// Interaktionslogik für LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {
        MainViewModel mainViewModel;


        public LoginWindow() {
            InitializeComponent();
            mainViewModel = (MainViewModel)this.DataContext;
            mainViewModel.LookForServerInLan();
            this.ConnectionMethodCombobox.DropDownClosed += OnConnectionMethodChoosen;
        }



        private void OnConnectionMethodChoosen(object? sender, EventArgs e) {
            switch (mainViewModel.ConnectionMethod) {
                case MainViewModel.ConnectionMethods.Localhost:
                    this.IPList.Visibility = Visibility.Hidden;
                    break;
                case MainViewModel.ConnectionMethods.SearchInLan:
                    this.IPList.Visibility = Visibility.Visible;
                    break;
            }
            
        }

        private void ButtonProfilePicture_Click(object sender, RoutedEventArgs e) {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            Nullable<bool> result = dlg.ShowDialog();

            if (result == true) {
                // Open document 
                string sourceFile = dlg.FileName;
                string destinationFile = @"C:\Users\dylan\source\repos\MyChatApp\MyChatApp\ProfilePictures\" + dlg.SafeFileName;
                try {
                    File.Copy(sourceFile, destinationFile, true);
                }
                catch (Exception) {
                    
                }
                finally {
                    mainViewModel.ProfilePicture = destinationFile;
                }

            }

        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e) {
            this.ServerOfflineErrorMessage.Visibility = Visibility.Hidden;
            this.UsernameEmpyErrorMessage.Visibility = Visibility.Hidden;
            try {
                mainViewModel.ConnectToServerCommand.Execute(this);
            }
            catch (Exception) {
                this.ServerOfflineErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (!string.IsNullOrEmpty(mainViewModel.Username)) {
                MainWindow mainWindow = new MainWindow();

                mainWindow.DataContext = mainViewModel;
                mainWindow.Show();
                Application.Current.MainWindow = mainWindow;
                this.Close();
            }
            else {
                this.UsernameEmpyErrorMessage.Visibility = Visibility.Visible;
            }
        }
    }
}
