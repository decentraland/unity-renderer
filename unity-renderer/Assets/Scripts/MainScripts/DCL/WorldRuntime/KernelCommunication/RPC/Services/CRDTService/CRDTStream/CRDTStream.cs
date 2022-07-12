using System;
using System.IO;
using KernelCommunication;

namespace RPC.Services
{
    public class CRDTStream : Stream, IBinaryReader
    {
        private byte[] streamBuffer;
        private int offset;

        public override bool CanRead => Position < streamBuffer.Length;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => streamBuffer.Length;

        public override long Position { set => offset = (int)value; get => offset; }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value) { }

        public override void Write(byte[] buffer, int offset, int count)
        {
            streamBuffer = buffer;
            this.offset = offset;
        }

        int IBinaryReader.offset => offset;

        bool IBinaryReader.CanRead() => CanRead;

        unsafe int IBinaryReader.ReadInt32()
        {
            int ofs = offset;
            offset += 4;

            if (streamBuffer == null)
                throw new NullReferenceException("bytes array == null");
            if ((long)(uint)ofs >= (long)streamBuffer.Length)
                throw new IndexOutOfRangeException("offset is bigger than bytes array lenght");
            if (ofs > streamBuffer.Length - 4)
                throw new IndexOutOfRangeException("bytes.Length is not large enough to contain a valid Int32");

            fixed (byte* numPtr = &streamBuffer[ofs])
            {
                return ByteUtils.PointerToInt32(numPtr);
            }
        }

        unsafe long IBinaryReader.ReadInt64()
        {
            int ofs = offset;
            offset += 8;

            if (streamBuffer == null)
                throw new NullReferenceException("bytes array == null");
            if ((long)(uint)ofs >= (long)streamBuffer.Length)
                throw new IndexOutOfRangeException("offset is bigger than bytes array lenght");
            if (ofs > streamBuffer.Length - 8)
                throw new IndexOutOfRangeException("bytes.Length is not large enough to contain a valid Int64");

            fixed (byte* numPtr = &streamBuffer[ofs])
            {
                return ByteUtils.PointerToInt64(numPtr);
            }
        }

        byte[] IBinaryReader.ReadBytes(int length)
        {
            int ofs = offset;
            offset += length;
            byte[] data = new byte[length];
            Buffer.BlockCopy(streamBuffer, ofs, data, 0, length);
            return data;
        }

        void IBinaryReader.Skip(int bytesCount)
        {
            offset += bytesCount;
        }
    }
}