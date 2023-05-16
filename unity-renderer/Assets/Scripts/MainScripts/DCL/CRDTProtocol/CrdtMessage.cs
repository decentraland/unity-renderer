namespace DCL.CRDT
{
    public enum CrdtMessageType
    {
        NONE = 0,
        PUT_COMPONENT = 1,
        DELETE_COMPONENT = 2,
        DELETE_ENTITY = 3,
        APPEND_COMPONENT = 4
    }

    public static class CrdtConstants
    {
        public const int MESSAGE_HEADER_LENGTH = 8;

        public const int CRDT_PUT_COMPONENT_HEADER_LENGTH = 16;
        public const int CRDT_DELETE_COMPONENT_HEADER_LENGTH = 12;
        public const int CRDT_DELETE_ENTITY_HEADER_LENGTH = 4;
        public const int CRDT_APPEND_COMPONENT_HEADER_LENGTH = 16;

        public const int CRDT_PUT_COMPONENT_BASE_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_PUT_COMPONENT_HEADER_LENGTH;
        public const int CRDT_DELETE_COMPONENT_BASE_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_DELETE_COMPONENT_HEADER_LENGTH;
        public const int CRDT_DELETE_ENTITY_BASE_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_DELETE_ENTITY_HEADER_LENGTH;
        public const int CRDT_APPEND_COMPONENT_BASE_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_APPEND_COMPONENT_HEADER_LENGTH;
    }

    public readonly struct CrdtMessage
    {
        public readonly CrdtMessageType Type;

        // the entityId is stored as 64bit integer, but in the protocol is serialized as 32bit (ADR-117)
        public readonly long EntityId;
        public readonly int ComponentId;
        public readonly int Timestamp;
        public readonly object Data;

        public CrdtMessage(CrdtMessageType type, long entityId, int componentId, int timestamp, object data)
        {
            EntityId = entityId;
            ComponentId = componentId;
            Timestamp = timestamp;
            Data = data;
            Type = type;
        }

        public static int GetMessageDataLength(CrdtMessage message)
        {
            if (message.Type == CrdtMessageType.PUT_COMPONENT) { return CrdtConstants.CRDT_PUT_COMPONENT_BASE_LENGTH + (((byte[])message.Data)?.Length ?? 0); }

            if (message.Type == CrdtMessageType.DELETE_COMPONENT) { return CrdtConstants.CRDT_DELETE_COMPONENT_BASE_LENGTH; }

            if (message.Type == CrdtMessageType.DELETE_ENTITY) { return CrdtConstants.CRDT_DELETE_ENTITY_BASE_LENGTH; }

            if (message.Type == CrdtMessageType.APPEND_COMPONENT) { return CrdtConstants.CRDT_APPEND_COMPONENT_BASE_LENGTH + (((byte[])message.Data)?.Length ?? 0); }

            return 0;
        }
    }
}
