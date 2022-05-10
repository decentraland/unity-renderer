namespace DCL.CRDT
{
    public static class CRDTUtils
    {
        public static long KeyFromIds(int entityId, int componentId)
        {
            // we create an int64 that store both entityId and componentId.
            // entityId on the left 32 bit and componentId in the remaining 
            // 32 bits of the right
            return (long)entityId << 32 | (long)componentId;
        }

        public static int EntityIdFromKey(long key)
        {
            return (int)(key >> 32);
        }

        public static int ComponentIdFromKey(long key)
        {
            return (int)(key & 0xFFFFFFFF);
        }
    }
}