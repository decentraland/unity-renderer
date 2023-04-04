namespace DCL.Chat
{
    public static class ChatUtils
    {
        public const string NEARBY_CHANNEL_ID = "nearby";
        public const string CONVERSATION_LIST_ID = "conversationList";

        public static string AddNoParse(string message)
        {
            string filteredMessage = message.Replace("<noparse", "")
                                            .Replace("</noparse", "")
                                            .Replace("<link", "</noparse><link")
                                            .Replace("</link>", "</link><noparse>");
            filteredMessage = filteredMessage.Insert(0, "<noparse>");
            filteredMessage = filteredMessage.Insert(filteredMessage.Length, "</noparse>");
            return filteredMessage;
        }
    }
}
