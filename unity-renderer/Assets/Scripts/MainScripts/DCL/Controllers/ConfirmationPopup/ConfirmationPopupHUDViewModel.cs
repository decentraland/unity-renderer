namespace DCL.ConfirmationPopup
{
    public record ConfirmationPopupHUDViewModel
    {
        public readonly string Title;
        public readonly string Body;
        public readonly string CancelButton;
        public readonly string ConfirmButton;

        public ConfirmationPopupHUDViewModel() : this("", "", "cancel", "confirm")
        {
        }

        public ConfirmationPopupHUDViewModel(string title, string body, string cancelButton, string confirmButton)
        {
            Title = title;
            Body = body;
            CancelButton = cancelButton;
            ConfirmButton = confirmButton;
        }
    }
}
