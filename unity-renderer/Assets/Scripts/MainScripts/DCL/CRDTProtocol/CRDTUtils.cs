namespace DCL.CRDT
{
    public static class CRDTUtils
    {
        public static long KeyFromIds(int entityId, int componentId)
        {
            return (long)entityId << 32 | (uint)componentId;
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