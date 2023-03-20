using System;

namespace DCL.Chat.Notifications
{
    public interface IChatNotificationMessageComponentView
    {
        /// <summary>
        /// Event that will be triggered when the notification is clicked.
        /// </summary>
        event Action<string> OnClickedNotification;

        /// <summary>
        /// Set the notification text.
        /// </summary>
        /// <param name="message">New message.</param>
        void SetMessage(string message);

        /// <summary>
        /// Set the notification time.
        /// </summary>
        /// <param name="timestamp">New timestamp.</param>
        void SetTimestamp(string timestamp);

        /// <summary>
        /// Set the notification header, can be a private chat name or a channel name.
        /// </summary>
        /// <param name="header">New header.</param>
        void SetNotificationHeader(string header);

        /// <summary>
        /// Set the notification sender, a username.
        /// </summary>
        /// <param name="username">New username.</param>
        void SetNotificationSender(string username);

        /// <summary>
        /// Set the notification type, can be a private or not.
        /// </summary>
        /// <param name="isPrivate">If the notification is private or not.</param>
        void SetIsPrivate(bool isPrivate);

        /// <summary>
        /// Set the notification player icon if isPrivate is true.
        /// </summary>
        /// <param name="uri">Uri of the image. Null for hide the icon.</param>
        void SetImage(string uri);

        /// <summary>
        /// Set the notification maximum content characters.
        /// </summary>
        /// <param name="maxContentCharacters">Max content characters</param>
        void SetMaxContentCharacters(int maxContentCharacters);

        /// <summary>
        /// Set the notification maximum header characters.
        /// </summary>
        /// <param name="maxHeaderCharacters">Max header characters</param>
        void SetMaxHeaderCharacters(int maxHeaderCharacters);

        /// <summary>
        /// Set the notification target id (either a channel or a user id)
        /// </summary>
        /// <param name="notificationTargetId">New target id.</param>
        void SetNotificationTargetId(string notificationTargetId);

        /// <summary>
        /// Set if the own player is mentioned in the notification message.
        /// </summary>
        /// <param name="isMentioned">If the own player is mentioned or not.</param>
        void SetOwnPlayerMention(bool isMentioned);
    }
}
