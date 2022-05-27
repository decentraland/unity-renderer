public class FriendRequestEntryModel : FriendEntryModel
{
    public bool isReceived;

    public FriendRequestEntryModel()
    {
    }

    public FriendRequestEntryModel(FriendEntryModel model, bool isReceived)
        : base(model)
    {
        this.isReceived = isReceived;
    }
}