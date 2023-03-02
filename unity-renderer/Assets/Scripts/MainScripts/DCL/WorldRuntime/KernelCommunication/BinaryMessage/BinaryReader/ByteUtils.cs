using System;

namespace KernelCommunication
{
    public static class ByteUtils
    {
        public static unsafe int PointerToInt32(byte* ptr)
        {
            return (int)*ptr << 24 | (int)ptr[1] << 16 | (int)ptr[2] << 8 | (int)ptr[3];
        }

        public static unsafe long PointerToInt64(byte* ptr)
        {
            int num = (int)*ptr << 24 | (int)ptr[1] << 16 | (int)ptr[2] << 8 | (int)ptr[3];
            return (long)((uint)((int)ptr[4] << 24 | (int)ptr[5] << 16 | (int)ptr[6] << 8) | (uint)ptr[7]) | (long)num << 32;
        }

        public static unsafe int ReadInt32(ReadOnlySpan<byte> span, int position)
        {
            if ((long)(uint)position >= (long)span.Length)
                throw new IndexOutOfRangeException("offset is bigger than bytes array lenght");
            if (position > span.Length - 4)
                throw new IndexOutOfRangeException("bytes.Length is not large enough to contain a valid Int32");

            fixed (byte* numPtr = &span[position])
            {
                return PointerToInt32(numPtr);
            }
        }

        public static unsafe long ReadInt64(ReadOnlySpan<byte> span, int position)
        {
            if ((long)(uint)position >= (long)span.Length)
                throw new IndexOutOfRangeException("offset is bigger than bytes array lenght");
            if (position > span.Length - 8)
                throw new IndexOutOfRangeException("bytes.Length is not large enough to contain a valid Int64");

            fixed (byte* numPtr = &span[position])
            {
                return PointerToInt64(numPtr);
            }
        }
    }
}