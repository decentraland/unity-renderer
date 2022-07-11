using System;
using System.IO;
using UnityEngine;

namespace DCL.Components
{
    public static class DCLTransformUtils
    {
        public static unsafe void DecodeTransform(string payload, ref DCLTransform.Model model)
        {
            byte[] bytes = Convert.FromBase64String(payload);
            fixed (byte* numPtr = &bytes[0])
            {
                float* arr = (float*)numPtr;
                model.position.x = arr[0];
                model.position.y = arr[1];
                model.position.z = arr[2];
                model.rotation.x = arr[3];
                model.rotation.y = arr[4];
                model.rotation.z = arr[5];
                model.rotation.w = arr[6];
                model.scale.x = arr[7];
                model.scale.y = arr[8];
                model.scale.z = arr[9];
            }
        }
        
        public static unsafe string EncodeTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            unsafe void WriteSingleToStream(float value, Stream stream)
            {
                byte* ptr = (byte*)&value;
                stream.WriteByte(ptr[0]);
                stream.WriteByte(ptr[1]);
                stream.WriteByte(ptr[2]);
                stream.WriteByte(ptr[3]);
            }

            using (var memoryStream = new MemoryStream(40))
            {
                WriteSingleToStream(position.x, memoryStream);
                WriteSingleToStream(position.y, memoryStream);
                WriteSingleToStream(position.z, memoryStream);
                WriteSingleToStream(rotation.x, memoryStream);
                WriteSingleToStream(rotation.y, memoryStream);
                WriteSingleToStream(rotation.z, memoryStream);
                WriteSingleToStream(rotation.w, memoryStream);
                WriteSingleToStream(scale.x, memoryStream);
                WriteSingleToStream(scale.y, memoryStream);
                WriteSingleToStream(scale.z, memoryStream);
                
                return Convert.ToBase64String(memoryStream.GetBuffer());
            }
        }        
    }
}