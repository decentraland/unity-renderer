using System;

namespace DCL
{
    public record ConfirmationPopupData
    {
        public readonly string Title;
        public readonly string Body;
        public readonly string CancelButton;
        public readonly string ConfirmButton;
        public readonly Action CancelAction;
        public readonly Action ConfirmAction;

        public ConfirmationPopupData(string title, string body, string cancelButton, string confirmButton,
            Action cancelAction, Action confirmAction)
        {
            Title = title;
            Body = body;
            CancelButton = cancelButton;
            ConfirmButton = confirmButton;
            CancelAction = cancelAction;
            ConfirmAction = confirmAction;
        }

        public static ConfirmationPopupData CreateUnFriendData(string userName, Action confirmationAction) =>
            new (
                $"Are you sure you want to unfriend {userName}?",
                "This player and you will no longer be friends, meaning you won't be able to send each other private messages.",
                "CANCEL",
                "UNFRIEND",
                null,
                confirmationAction);
    }

    public class DataStore_Notifications
    {
        public readonly BaseVariable<string> DefaultErrorNotification = new ();
        public readonly BaseVariable<ConfirmationPopupData> ConfirmationPopup = new ();
    }
}
