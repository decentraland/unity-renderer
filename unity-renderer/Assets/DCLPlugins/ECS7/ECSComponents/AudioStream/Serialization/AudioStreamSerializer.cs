using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class AudioStreamSerializer
    {
        public static byte[] Serialize(PBAudioStream model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBAudioStream Deserialize(object data)
        {
            return PBAudioStream.Parser.ParseFrom((byte[])data);
        }
    }
}