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

        public Type messageType;
        public string sender;
        public string recipient;
        public ulong timestamp;
        public string body;
    }
}
