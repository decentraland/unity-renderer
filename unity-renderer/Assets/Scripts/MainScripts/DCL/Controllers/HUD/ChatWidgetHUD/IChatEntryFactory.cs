namespace DCL.Chat.HUD
{
    public interface IChatEntryFactory
    {
        ChatEntry Create(ChatEntryModel model);
        void Destroy(ChatEntry entry);
    }
}