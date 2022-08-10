using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class UITextSerializer
    {
        public static byte[] Serialize(PBUiText model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }
        
        public static PBUiText Deserialize(object data)
        {
            return PBUiText.Parser.ParseFrom((byte[])data);
        }
    }
}