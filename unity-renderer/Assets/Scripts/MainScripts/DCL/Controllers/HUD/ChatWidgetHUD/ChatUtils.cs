public static class ChatUtils
{
    public static string AddNoParse(string message)
    {
        var filteredMessage = message.Replace("noparse", "");
        filteredMessage = filteredMessage.Insert(0, "<noparse>");
        filteredMessage = filteredMessage.Insert(filteredMessage.Length, "</noparse>");
        return filteredMessage;
    }
}
