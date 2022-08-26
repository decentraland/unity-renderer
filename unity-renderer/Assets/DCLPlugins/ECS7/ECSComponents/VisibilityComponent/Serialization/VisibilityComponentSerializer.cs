using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class VisibilityComponentSerializer
    {
        public static byte[] Serialize(PBVisibilityComponent model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBVisibilityComponent Deserialize(object data)
        {
            return PBVisibilityComponent.Parser.ParseFrom((byte[])data);
        }
    }
}