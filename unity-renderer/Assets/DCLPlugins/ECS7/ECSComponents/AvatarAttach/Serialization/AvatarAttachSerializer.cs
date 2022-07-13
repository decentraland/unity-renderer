using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class AvatarAttachSerializer
    {
        public static byte[] Serialize(PBAvatarAttach model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBAvatarAttach Deserialize(object data)
        {
            return PBAvatarAttach.Parser.ParseFrom((byte[])data);
        }
    }
}