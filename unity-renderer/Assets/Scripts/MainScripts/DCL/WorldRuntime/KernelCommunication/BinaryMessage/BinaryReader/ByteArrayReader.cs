using System;

namespace KernelCommunication
{
    public class ByteArrayReader : IBinaryReader
    {
        private readonly byte[] bytes;
        private int currentOffset;

        public ByteArrayReader(byte[] bytes)
        {
            this.bytes = bytes;
            currentOffset = 0;
        }

        int IBinaryReader.offset => currentOffset;

        bool IBinaryReader.CanRead() => currentOffset < bytes.Length;

        unsafe int IBinaryReader.ReadInt32()
        {
            int ofs = currentOffset;
            currentOffset += 4;

            if (bytes == null)
                throw new NullReferenceException("bytes array == null");
            if ((long)(uint)ofs >= (long)bytes.Length)
                throw new IndexOutOfRangeException("offset is bigger than bytes array lenght");
            if (ofs > bytes.Length - 4)
                throw new IndexOutOfRangeException("bytes.Length is not large enough to contain a valid Int32");

            fixed (byte* numPtr = &bytes[ofs])
            {
                return ByteUtils.PointerToInt32(numPtr);
            }
        }

        unsafe long IBinaryReader.ReadInt64()
        {
            int ofs = currentOffset;
            currentOffset += 8;

            if (bytes == null)
                throw new NullReferenceException("bytes array == null");
            if ((long)(uint)ofs >= (long)bytes.Length)
                throw new IndexOutOfRangeException("offset is bigger than bytes array lenght");
            if (ofs > bytes.Length - 8)
                throw new IndexOutOfRangeException("bytes.Length is not large enough to contain a valid Int64");

            fixed (byte* numPtr = &bytes[ofs])
            {
                return ByteUtils.PointerToInt64(numPtr);
            }
        }

        byte[] IBinaryReader.ReadBytes(int length)
        {
            int ofs = currentOffset;
            currentOffset += length;
            byte[] data = new byte[length];
            Buffer.BlockCopy(bytes, ofs, data, 0, length);
            return data;
        }

        void IBinaryReader.Skip(int bytesCount)
        {
            currentOffset += bytesCount;
        }
    }
}