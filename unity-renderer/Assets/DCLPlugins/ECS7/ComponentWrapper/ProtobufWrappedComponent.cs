using DCL.ECS7.ComponentWrapper.Generic;
using Google.Protobuf;
using System;
using System.IO;

namespace DCL.ECS7.ComponentWrapper
{
    public record ProtobufWrappedComponent<T> : IWrappedComponent<T> where T: class, IMessage<T>
    {
        private readonly T model;

        public T Model => model;

        public static implicit operator T(ProtobufWrappedComponent<T> wrapped) =>
            wrapped.model;

        public ProtobufWrappedComponent(T model)
        {
            this.model = model;
        }

        public void SerializeTo(MemoryStream buffer, CodedOutputStream stream)
        {
            buffer.SetLength(0);
            model.WriteTo(stream);
            stream.Flush();
        }

        public void DeserializeFrom(ReadOnlySpan<byte> bytes)
        {
            ClearFields();
            model.MergeFrom(bytes);
        }

        public void ClearFields()
        {
            var fields = model.Descriptor.Fields.InDeclarationOrder();

            for (int i = 0; i < fields.Count; i++)
            {
                fields[i].Accessor.Clear(model);
            }
        }
    }
}
