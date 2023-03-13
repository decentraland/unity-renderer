using System.IO;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace DCL.ECSComponents
{
    public static class ECSTransformSerialization
    {
        private const int TRANSFORM_LENGTH = 44;

        private static readonly MemoryStream memoryStream = new MemoryStream(TRANSFORM_LENGTH);
        private static readonly BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

        public static unsafe ECSTransform Deserialize(object data)
        {
            byte[] bytes = (byte[])data;

            ECSTransform model = new ECSTransform();

            fixed (byte* numPtr = &bytes[0])
            {
                float* arr = (float*)numPtr;
                model.position.Set(arr[0], arr[1], arr[2]);
                model.rotation.Set(arr[3], arr[4], arr[5], arr[6]);
                model.scale.Set(arr[7], arr[8], arr[9]);
                model.parentId = *(int*)(numPtr + 40);
            }

            return model;
        }

        public static byte[] Serialize(ECSTransform model)
        {
            binaryWriter.WriteSingle(model.position.x);
            binaryWriter.WriteSingle(model.position.y);
            binaryWriter.WriteSingle(model.position.z);

            binaryWriter.WriteSingle(model.rotation.x);
            binaryWriter.WriteSingle(model.rotation.y);
            binaryWriter.WriteSingle(model.rotation.z);
            binaryWriter.WriteSingle(model.rotation.w);

            binaryWriter.WriteSingle(model.scale.x);
            binaryWriter.WriteSingle(model.scale.y);
            binaryWriter.WriteSingle(model.scale.z);

            binaryWriter.WriteInt32((int)model.parentId);

            byte[] result = memoryStream.ToArray();
            memoryStream.SetLength(0);
            return result;
        }
    }
}
