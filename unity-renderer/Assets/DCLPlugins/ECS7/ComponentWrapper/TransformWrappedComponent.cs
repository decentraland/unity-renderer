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

        public void SerializeTo(MemoryStream buffer, CodedOutputStream stream)
        {
            buffer.SetLength(0);

            stream.WriteFloat(model.position.x);
            stream.WriteFloat(model.position.y);
            stream.WriteFloat(model.position.z);

            stream.WriteFloat(model.rotation.x);
            stream.WriteFloat(model.rotation.y);
            stream.WriteFloat(model.rotation.z);
            stream.WriteFloat(model.rotation.w);

            stream.WriteFloat(model.scale.x);
            stream.WriteFloat(model.scale.y);
            stream.WriteFloat(model.scale.z);

            stream.WriteInt32((int)model.parentId);

            stream.Flush();
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
    }
}
