using System;
using System.Runtime.InteropServices;

namespace KernelCommunication
{
    public class UnmanagedMemoryReader : IBinaryReader
    {
        private unsafe byte* ptr;
        private int currentOffset;

        private readonly int dataLenght;

        public unsafe UnmanagedMemoryReader(IntPtr intPtr, int dataLenght)
        {
            this.ptr = (byte*)intPtr.ToPointer();
            this.currentOffset = 0;
            this.dataLenght = dataLenght;
        }

        int IBinaryReader.offset => currentOffset;

        bool IBinaryReader.CanRead() => currentOffset < dataLenght;

        unsafe int IBinaryReader.ReadInt32()
        {
            if (ptr == null)
                throw new NullReferenceException("pointer == null");
            if ((long)(uint)currentOffset >= (long)dataLenght)
                throw new IndexOutOfRangeException("offset is bigger than data lenght");
            if (currentOffset > dataLenght - 4)
                throw new IndexOutOfRangeException("data lenght is not large enough to contain a valid Int32");

            byte* numPtr = ptr;
            currentOffset += 4;
            ptr += 4;

            return ByteUtils.PointerToInt32(numPtr);
        }

        unsafe long IBinaryReader.ReadInt64()
        {
            if (ptr == null)
                throw new NullReferenceException("pointer == null");
            if ((long)(uint)currentOffset >= (long)dataLenght)
                throw new IndexOutOfRangeException("offset is bigger than data lenght");
            if (currentOffset > dataLenght - 8)
                throw new IndexOutOfRangeException("data lenght is not large enough to contain a valid Int64");

            byte* numPtr = ptr;
            currentOffset += 8;
            ptr += 8;

            return ByteUtils.PointerToInt64(numPtr);
        }

        unsafe byte[] IBinaryReader.ReadBytes(int length)
        {
            if (ptr == null)
                throw new NullReferenceException("pointer == null");
            if ((long)(uint)currentOffset > (long)dataLenght - length)
                throw new IndexOutOfRangeException($"data lenght is not large enough to read {length} bytes");

            byte[] data = new byte[length];
            Marshal.Copy(new IntPtr(ptr), data, 0, length);
            currentOffset += length;
            ptr += length;
            return data;
        }

        unsafe void IBinaryReader.Skip(int bytesCount)
        {
            currentOffset += bytesCount;
            ptr += bytesCount;
        }
    }
}