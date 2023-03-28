using System.Collections.Generic;

namespace DCL.Social.Chat
{
    public class ChatEntrySortingSequentially : IComparer<ChatEntryModel>
    {
        public int Compare(ChatEntryModel x, ChatEntryModel y) => 0;
    }
}
