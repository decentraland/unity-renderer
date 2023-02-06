namespace DCL.CRDT
{
    public enum CrdtMessageType
    {
        NONE = 0,
        PUT_COMPONENT = 1,
        DELETE_COMPONENT = 2,
        DELETE_ENTITY = 3
    }

    public static class CrdtConstants
    {
        public const int MESSAGE_HEADER_LENGTH = 8;

        public const int CRDT_PUT_COMPONENT_HEADER_LENGTH = 16;
        public const int CRDT_DELETE_COMPONENT_HEADER_LENGTH = 12;
        public const int CRDT_DELETE_ENTITY_HEADER_LENGTH = 4;

        public const int CRDT_PUT_COMPONENT_BASE_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_PUT_COMPONENT_HEADER_LENGTH;
        public const int CRDT_DELETE_COMPONENT_BASE_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_DELETE_COMPONENT_HEADER_LENGTH;
        public const int CRDT_DELETE_ENTITY_BASE_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_DELETE_ENTITY_HEADER_LENGTH;
    }
    public class CRDTMessage
    {
        public CrdtMessageType type = CrdtMessageType.NONE;
        // the entityId is stored as 64bit integer, but in the protocol is serialized as 32bit (ADR-117)
        public long entityId;
        public int componentId;
        public int timestamp;
        public object data;

        public static int GetMessageDataLength(CRDTMessage message)
        {
            if (message.type == CrdtMessageType.PUT_COMPONENT) { return CrdtConstants.CRDT_PUT_COMPONENT_BASE_LENGTH + (((byte[])message.data)?.Length ?? 0); }
            if (message.type == CrdtMessageType.DELETE_COMPONENT) { return CrdtConstants.CRDT_DELETE_COMPONENT_BASE_LENGTH; }
            if (message.type == CrdtMessageType.DELETE_ENTITY) { return CrdtConstants.CRDT_DELETE_ENTITY_BASE_LENGTH; }

            return 0;
        }
    }
}
