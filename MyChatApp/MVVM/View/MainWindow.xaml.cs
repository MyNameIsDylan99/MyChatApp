using MyChatApp.MVVM.View;
using MyChatApp.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyChatApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            MainViewModel mvm = (MainViewModel)this.DataContext;
            mvm.OnServerShutdown = OnServerShutdown;
        }

        void Border_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DragMove();
            }
        }
        void pop_MouseMove(object sender, MouseButtonEventArgs e) {
            Border send = sender as Border;
            Canvas contentCanvas = send.Parent as Canvas;
            Popup pop = contentCanvas.Parent as Popup;
            var thumb = new Thumb {
                Width = 0,
                Height = 0,
            };

            contentCanvas.Children.Add(thumb);

                thumb.RaiseEvent(e);

            thumb.DragDelta += (sender, e) =>
            {
                pop.HorizontalOffset += e.HorizontalChange;
                pop.VerticalOffset += e.VerticalChange;
            };
        }

        void ButtonMinimize_Click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        void ButtonMaximize_Click(object sender, RoutedEventArgs e) {

            if (Application.Current.MainWindow.WindowState != WindowState.Maximized) {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }

            else {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

         void ButtonClose_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void ButtonBackToLoginWindow_Click(object sender, RoutedEventArgs e) {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Application.Current.MainWindow = loginWindow;
            this.Close();
        }

        void OnServerShutdown() {
            Application.Current.Dispatcher.Invoke(() => this.ServerShutdownPop.IsOpen = true);
        }


    }
}
