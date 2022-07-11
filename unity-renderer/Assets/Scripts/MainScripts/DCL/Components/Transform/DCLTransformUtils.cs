using System;

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
    }
}