using Google.Protobuf;
using System;
using System.IO;

namespace DCL.ECS7.ComponentWrapper
{
    public interface IWrappedComponent
    {
        void SerializeTo(MemoryStream buffer, CodedOutputStream stream);

        void DeserializeFrom(ReadOnlySpan<byte> bytes);

        void ClearFields();
    }
}
