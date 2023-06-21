using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using Google.Protobuf;
using System;
using System.IO;
using UnityEngine;

namespace DCL.ECS7.ComponentWrapper
{
    public record TransformWrappedComponent : IWrappedComponent<ECSTransform>
    {
        public ECSTransform Model => model;

        private readonly ECSTransform model;

        public static implicit operator ECSTransform(TransformWrappedComponent wrapped) =>
            wrapped.model;

        public TransformWrappedComponent(ECSTransform model)
        {
            this.model = model;
        }

        public void SerializeTo(MemoryStream buffer, CodedOutputStream _)
        {
            buffer.SetLength(0);

            WriteSingle(model.position.x, buffer);
            WriteSingle(model.position.y, buffer);
            WriteSingle(model.position.z, buffer);

            WriteSingle(model.rotation.x, buffer);
            WriteSingle(model.rotation.y, buffer);
            WriteSingle(model.rotation.z, buffer);
            WriteSingle(model.rotation.w, buffer);

            WriteSingle(model.scale.x, buffer);
            WriteSingle(model.scale.y, buffer);
            WriteSingle(model.scale.z, buffer);

            WriteInt32((int)model.parentId, buffer);
        }

        public unsafe void DeserializeFrom(ReadOnlySpan<byte> bytes)
        {
            fixed (byte* numPtr = &bytes[0])
            {
                float* arr = (float*)numPtr;
                model.position.Set(arr[0], arr[1], arr[2]);
                model.rotation.Set(arr[3], arr[4], arr[5], arr[6]);
                model.scale.Set(arr[7], arr[8], arr[9]);
                model.parentId = *(int*)(numPtr + 40);
            }
        }

        public void ClearFields()
        {
            model.position = Vector3.zero;
            model.rotation = Quaternion.identity;
            model.scale = Vector3.one;
            model.parentId = 0;
        }

        private static unsafe void WriteInt32(int value, MemoryStream buffer)
        {
            byte* ptr = (byte*)&value;
            buffer.WriteByte(ptr[0]);
            buffer.WriteByte(ptr[1]);
            buffer.WriteByte(ptr[2]);
            buffer.WriteByte(ptr[3]);
        }

        public unsafe void WriteSingle(float value, MemoryStream buffer)
        {
            byte* ptr = (byte*)&value;
            buffer.WriteByte(ptr[0]);
            buffer.WriteByte(ptr[1]);
            buffer.WriteByte(ptr[2]);
            buffer.WriteByte(ptr[3]);
        }
    }
}
