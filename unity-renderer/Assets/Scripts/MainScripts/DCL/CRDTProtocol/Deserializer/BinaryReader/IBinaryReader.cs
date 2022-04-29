namespace DCL.CRDT.BinaryReader
{
    public interface IBinaryReader
    {
        int offset { get; }
        bool CanRead();
        int ReadInt32();
        long ReadInt64();
        byte[] ReadBytes(int size);
    }
}