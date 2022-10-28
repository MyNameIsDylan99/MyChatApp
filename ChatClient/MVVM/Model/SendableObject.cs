using System;

namespace MyChatApp {
    public class SendableObject {

        public SendableObjectType Type { get; set; }
        public string Username { get; set; }
        public string Content { get; set; } = "";
        public string UsernameColor { get; set; }
        public string ProfilePictureImageSource { get; set; } = "";

        public DateTime Time { get; set; }

        public SendableObject(SendableObjectType type, string username, string content, string profilePictureImageSource, DateTime time) {
            Type = type;
            Username = username;
            Content = content;
            UsernameColor = "White";
            ProfilePictureImageSource = profilePictureImageSource;
            Time = time;
        }
    }
    public enum SendableObjectType {
        TextMessage,
        Picture
    }

}


