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
    }

    public class DataStore_Notifications
    {
        public readonly BaseVariable<string> DefaultErrorNotification = new ();
        public readonly BaseVariable<ConfirmationPopupData> ConfirmationPopup = new ();
    }
}
