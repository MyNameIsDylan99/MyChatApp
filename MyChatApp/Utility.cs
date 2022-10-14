using System;

namespace MyChatApp {
    internal static class Utility {

        public static string ChoosePicture() {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = string.Empty;
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            bool? result = dlg.ShowDialog();
            var choosenPicturePath = "";
            if (result == true) {

                choosenPicturePath = dlg.FileName;
            }

            return choosenPicturePath;

        }

    }
}