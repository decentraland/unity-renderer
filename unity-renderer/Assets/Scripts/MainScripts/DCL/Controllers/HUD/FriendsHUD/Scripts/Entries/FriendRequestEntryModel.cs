using System;

public class FriendRequestEntryModel : FriendEntryModel
{
    public string bodyMessage;
    public bool isReceived;
    public DateTime timestamp;
    public bool isShortcutButtonsActive;

    public FriendRequestEntryModel()
    {
    }

    public FriendRequestEntryModel(FriendEntryModel model, string bodyMessage, bool isReceived, DateTime timestamp, bool isShortcutButtonsActive)
        : base(model)
    {
        this.bodyMessage = bodyMessage;
        this.isReceived = isReceived;
        this.timestamp = timestamp;
        this.isShortcutButtonsActive = isShortcutButtonsActive;
    }
}
