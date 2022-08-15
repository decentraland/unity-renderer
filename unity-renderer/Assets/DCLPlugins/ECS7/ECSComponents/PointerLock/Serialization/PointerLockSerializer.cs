using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class PointerLockSerializer
    {
        public static byte[] Serialize(PBPointerLock model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBPointerLock Deserialize(object data)
        {
            return PBPointerLock.Parser.ParseFrom((byte[])data);
        }
    }
}