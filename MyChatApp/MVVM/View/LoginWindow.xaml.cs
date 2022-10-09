using MyChatApp.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        LoginViewModel loginViewModel;


        public LoginWindow() {
            InitializeComponent();
            loginViewModel = (LoginViewModel)this.DataContext;
            this.ConnectionMethodCombobox.DropDownClosed += OnConnectionMethodChoosen;
        }



        private void OnConnectionMethodChoosen(object? sender, EventArgs e) {
            switch (loginViewModel.ConnectionMethod) {
                case LoginViewModel.ConnectionMethods.Localhost:
                    this.IPList.Visibility = Visibility.Hidden;
                    break;
                case LoginViewModel.ConnectionMethods.SearchInLan:
                    this.IPList.Visibility = Visibility.Visible;
                    break;
            }
            
        }

            private void ButtonLogin_Click(object sender, RoutedEventArgs e) {
            this.ServerOfflineErrorMessage.Visibility = Visibility.Hidden;
            this.UsernameEmpyErrorMessage.Visibility = Visibility.Hidden;
            try {
                loginViewModel.ConnectToServerCommand.Execute(this);
            }
            catch (Exception) {
                this.ServerOfflineErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (!string.IsNullOrEmpty(loginViewModel.Username)) {
                MainWindow mainWindow = new MainWindow();
               MainViewModel mvm =  (MainViewModel)mainWindow.DataContext;
                mvm.GetDataFromLoginViewModel(loginViewModel.Username, loginViewModel.ProfilePictureSource, loginViewModel.Server);
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
