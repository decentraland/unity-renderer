using System;

namespace DCL.Chat.Notifications
{
    [Serializable]
    public class ChatNotificationMessageComponentModel : BaseComponentModel
    {
        public int maxHeaderCharacters;
        public int maxContentCharacters;
        public int maxSenderCharacters;

        public string message;
        public string time;
        public string messageHeader;
        public string messageSender;
        public bool isPrivate;
        public bool isFriendRequest;
        public string imageUri;
        public string notificationTargetId;
        public bool isImageVisible = true;
        public bool isDockedLeft = true;
        public bool isOwnPlayerMentioned;
    }
}
