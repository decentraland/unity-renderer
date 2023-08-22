namespace DCL.Social.Chat
{
    public interface IChatEntryFactory
    {
        ChatEntry Create(ChatEntryModel model);
        void Destroy(ChatEntry entry);
    }
}
