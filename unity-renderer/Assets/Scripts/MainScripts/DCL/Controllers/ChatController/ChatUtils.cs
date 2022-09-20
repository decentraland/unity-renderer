namespace DCL.Chat
{
    public static class ChatUtils
    {
        public const string NEARBY_CHANNEL_ID = "nearby";
        
        public static string AddNoParse(string message)
        {
            var filteredMessage = message.Replace("<noparse", "")
                .Replace("</noparse", "");
            filteredMessage = filteredMessage.Insert(0, "<noparse>");
            filteredMessage = filteredMessage.Insert(filteredMessage.Length, "</noparse>");
            return filteredMessage;
        }
    }
}