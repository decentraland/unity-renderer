using System;
using System.IO;
using System.Text;

namespace KernelCommunication
{
    // Write BigEndian binary to stream
    public class BinaryWriter : IDisposable
    {
        private Stream stream;

        public BinaryWriter(Stream stream)
        {
            this.stream = stream;
        }

        public unsafe void Write(int value)
        {
            byte* ptr = (byte*)&value;
            stream.WriteByte(ptr[3]);
            stream.WriteByte(ptr[2]);
            stream.WriteByte(ptr[1]);
            stream.WriteByte(ptr[0]);
        }

        public unsafe void Write(long value)
        {
            byte* ptr = (byte*)&value;
            stream.WriteByte(ptr[7]);
            stream.WriteByte(ptr[6]);
            stream.WriteByte(ptr[5]);
            stream.WriteByte(ptr[4]);
            stream.WriteByte(ptr[3]);
            stream.WriteByte(ptr[2]);
            stream.WriteByte(ptr[1]);
            stream.WriteByte(ptr[0]);
        }

        public void Write(byte[] value)
        {
            stream.Write(value, 0, value.Length);
        }

        public void Write(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            stream = null;
        }
    }
}
