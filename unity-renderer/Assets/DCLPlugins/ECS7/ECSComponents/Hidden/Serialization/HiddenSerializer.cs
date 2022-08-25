using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class HiddenSerializer
    {
        public static byte[] Serialize(PBHidden model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBHidden Deserialize(object data)
        {
            return PBHidden.Parser.ParseFrom((byte[])data);
        }
    }
}