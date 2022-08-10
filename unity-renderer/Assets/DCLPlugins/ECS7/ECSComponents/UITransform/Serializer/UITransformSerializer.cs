using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class UITransformSerializer
    {
        public static byte[] Serialize(PBUiTransform model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }
        
        public static PBUiTransform Deserialize(object data)
        {
            return PBUiTransform.Parser.ParseFrom((byte[])data);
        }
    }
}