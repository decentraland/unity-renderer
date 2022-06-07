using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class NFTShapeSerializer
    {
        public static byte[] Serialize(PBNFTShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBNFTShape Deserialize(object data)
        {
            return PBNFTShape.Parser.ParseFrom((byte[])data);
        }
    }
}