using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class GLTFShapeSerializer
    {
        public static byte[] Serialize(PBGLTFShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBGLTFShape Deserialize(object data)
        {
            return PBGLTFShape.Parser.ParseFrom((byte[])data);
        }
    }
}