﻿using MyChatApp.MVVM.ViewModel;
using System;
using System.Windows;

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
                case ConnectionMethods.Localhost:
                    this.IPList.Visibility = Visibility.Hidden;
                    break;
                case ConnectionMethods.SearchInLan:
                    this.IPList.Visibility = Visibility.Visible;
                    break;
            }
            
        }

            private void ButtonLogin_Click(object sender, RoutedEventArgs e) {

            this.ServerOfflineErrorMessage.Visibility = Visibility.Hidden;
            this.UsernameEmpyErrorMessage.Visibility = Visibility.Hidden;
            MainWindow mainWindow = new MainWindow();
            MainViewModel mvm = (MainViewModel)mainWindow.DataContext;

            if (!string.IsNullOrEmpty(loginViewModel.Username)) {

                mvm.GetDataFromLoginViewModel(loginViewModel.Username, loginViewModel.ProfilePictureSource, loginViewModel.Server); }

            else {
                this.UsernameEmpyErrorMessage.Visibility = Visibility.Visible;
            }

            try {
                loginViewModel.ConnectToServerCommand.Execute(this);
            }
            catch (Exception) {
                this.ServerOfflineErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
            this.Close();

        }
    }
}
