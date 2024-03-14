using System;

namespace DCL
{
    public record GenericConfirmationNotificationData
    {
        public readonly string Title;
        public readonly string Body;
        public readonly string CancelButton;
        public readonly string ConfirmButton;
        public readonly Action CancelAction;
        public readonly Action ConfirmAction;

        public GenericConfirmationNotificationData(string title, string body, string cancelButton, string confirmButton,
            Action cancelAction, Action confirmAction)
        {
            Title = title;
            Body = body;
            CancelButton = cancelButton;
            ConfirmButton = confirmButton;
            CancelAction = cancelAction;
            ConfirmAction = confirmAction;
        }

        public static GenericConfirmationNotificationData CreateUnFriendData(string userName, Action confirmationAction) =>
            new (
                $"Are you sure you want to unfriend {userName}?",
                "This player and you will no longer be friends, meaning you won't be able to send each other private messages.",
                "CANCEL",
                "UNFRIEND",
                null,
                confirmationAction);

        public static GenericConfirmationNotificationData CreateBlockUserData(string userName, Action confirmationAction) =>
            new (
                $"Are you sure you want to block {userName}?",
                "Blocking someone means that you and that person can no longer see each other's avatars, messages, and send friend requests.",
                "CANCEL",
                "BLOCK",
                null,
                confirmationAction);

        public static GenericConfirmationNotificationData CreateUnBlockUserData(string userName, Action confirmationAction) =>
            new (
                $"Are you sure you want to unblock {userName}?",
                "Once you unblock someone, you and that person would be able to see each other's avatars, messages, and send friend requests.",
                "CANCEL",
                "UNBLOCK",
                null,
                confirmationAction);
    }

    public class DataStore_Notifications
    {
        public readonly BaseVariable<string> DefaultErrorNotification = new ();
        public readonly BaseVariable<GenericConfirmationNotificationData> GenericConfirmation = new ();
    }
}
