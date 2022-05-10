namespace DCL.CRDT
{
    internal class CRDTComponentMessageHeader
    {
        public int entityId;
        public int componentClassId;
        public long timestamp;
        public int dataLength;
    }
}