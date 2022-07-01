using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class AvatarModifierAreaSerializer
    {
        public static byte[] Serialize(PBAvatarModifierArea model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBAvatarModifierArea Deserialize(object data)
        {
            return PBAvatarModifierArea.Parser.ParseFrom((byte[])data);
        }
    }
}