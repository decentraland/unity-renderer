using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class TextShapeSerialization
    {
        public static byte[] Serialize(PBTextShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }
        
        public static PBTextShape Deserialize(object data)
        {
            return PBTextShape.Parser.ParseFrom((byte[])data);
        }
    }
}