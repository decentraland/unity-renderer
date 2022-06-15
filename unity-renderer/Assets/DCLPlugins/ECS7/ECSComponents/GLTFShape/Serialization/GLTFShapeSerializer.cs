using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class GLTFShapeSerializer
    {
        public static byte[] Serialize(PBBoxShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBBoxShape Deserialize(object data)
        {
            return PBBoxShape.Parser.ParseFrom((byte[])data);
        }
    }
}