namespace DCL.CRDT
{
    internal class CRDTMessageHeader
    {
        public int length;
        public int type;
    }

    internal class CRDTComponentMessageHeader
    {
        public int entityId;
        public int componentClassId;
        public long timestamp;
        public int dataLength;
    }
}