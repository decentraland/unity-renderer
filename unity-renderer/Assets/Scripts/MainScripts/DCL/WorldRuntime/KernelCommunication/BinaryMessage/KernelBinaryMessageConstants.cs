namespace KernelCommunication
{
    public enum KernelBinaryMessageType
    {
        PUT_COMPONENT = 1,
        DELETE_COMPONENT = 2
    }

    public static class BinaryMessageConstants
    {
        public const int MESSAGE_HEADER_LENGTH = 8;
        public const int CRDT_HEADER_LENGTH = 20;

        public const int CRDT_MESSAGE_BASE_HEADER_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_HEADER_LENGTH;
    }
}