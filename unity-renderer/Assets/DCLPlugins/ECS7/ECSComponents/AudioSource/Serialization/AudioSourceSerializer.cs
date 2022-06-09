using DCL.ECSComponents;
using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class AudioSourceSerializer
    {
        public static byte[] Serialize(PBAudioSource model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBAudioSource Deserialize(object data)
        {
            return PBAudioSource.Parser.ParseFrom((byte[])data);
        }
    }
}