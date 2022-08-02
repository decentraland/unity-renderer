using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class UITextSerialization
    {
        public static byte[] Serialize(PBUiTextShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }
        
        public static PBUiTextShape Deserialize(object data)
        {
            return PBUiTextShape.Parser.ParseFrom((byte[])data);
        }
    }
}