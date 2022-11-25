public class FriendRequestEntryModel : FriendEntryModel
{
    public string bodyMessage;
    public bool isReceived;

    public FriendRequestEntryModel()
    {
    }

    public FriendRequestEntryModel(FriendEntryModel model, string bodyMessage, bool isReceived)
        : base(model)
    {
        this.bodyMessage = bodyMessage;
        this.isReceived = isReceived;
    }
}