using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class AvatarShapeSerializer
    {
        public static byte[] Serialize(PBAvatarShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBAvatarShape Deserialize(object data)
        {
            return PBAvatarShape.Parser.ParseFrom((byte[])data);
        }
    }
}