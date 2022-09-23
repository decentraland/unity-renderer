namespace DCL.Interface
{
    [System.Serializable]
    public class ChatMessage
    {
        public enum Type
        {
            NONE,
            PUBLIC,
            PRIVATE,
            SYSTEM
        }

        public ChatMessage() { }
        public ChatMessage(Type messageType, string sender, string body)
        {
            this.messageType = messageType;
            this.sender = sender;
            this.body = body;
        }

        public ChatMessage(string messageId, Type messageType, string sender, string body): this(messageType, sender, body)
        {
            this.messageId = messageId;
        }

        public string messageId;
        public Type messageType;
        public string sender;
        public string recipient;
        public ulong timestamp;
        public string body;
    }
}