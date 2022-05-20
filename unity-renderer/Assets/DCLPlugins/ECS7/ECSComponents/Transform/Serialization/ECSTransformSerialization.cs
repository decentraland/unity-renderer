using System.IO;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace DCL.ECSComponents
{
    public static class ECSTransformSerialization
    {
        private const int TRANSFORM_LENGTH = 44;

        private static readonly byte[] toLittleEndianBuffer = new byte[TRANSFORM_LENGTH];
        private static readonly MemoryStream memoryStream = new MemoryStream(TRANSFORM_LENGTH);
        private static readonly BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

        public static unsafe ECSTransform Deserialize(object data)
        {
            byte[] bytes = (byte[])data;
            for (int i = 0; i < TRANSFORM_LENGTH; i++)
            {
                toLittleEndianBuffer[i] = bytes[TRANSFORM_LENGTH - 1 - i];
            }

            ECSTransform model = new ECSTransform();
            fixed (byte* numPtr = &toLittleEndianBuffer[0])
            {
                float* arr = (float*)numPtr;
                model.position.Set(arr[10], arr[9], arr[8]);
                model.rotation.Set(arr[7], arr[6], arr[5], arr[4]);
                model.scale.Set(arr[3], arr[2], arr[1]);
                model.parentId = *(int*)numPtr;
            }

            return model;
        }

        public static byte[] Serialize(ECSTransform model)
        {
            binaryWriter.Write(model.position.x);
            binaryWriter.Write(model.position.y);
            binaryWriter.Write(model.position.z);

            binaryWriter.Write(model.rotation.x);
            binaryWriter.Write(model.rotation.y);
            binaryWriter.Write(model.rotation.z);
            binaryWriter.Write(model.rotation.w);

            binaryWriter.Write(model.scale.x);
            binaryWriter.Write(model.scale.y);
            binaryWriter.Write(model.scale.z);

            binaryWriter.Write((int)model.parentId);

            byte[] result = memoryStream.ToArray();
            memoryStream.SetLength(0);
            return result;
        }
    }
}