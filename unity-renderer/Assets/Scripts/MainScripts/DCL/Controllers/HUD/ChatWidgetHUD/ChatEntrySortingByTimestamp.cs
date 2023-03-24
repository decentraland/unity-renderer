using System.Collections.Generic;

namespace DCL.Social.Chat
{
    public class ChatEntrySortingByTimestamp : IComparer<ChatEntryModel>
    {
        public int Compare(ChatEntryModel x, ChatEntryModel y) =>
            x.timestamp.CompareTo(y.timestamp);
    }
}
