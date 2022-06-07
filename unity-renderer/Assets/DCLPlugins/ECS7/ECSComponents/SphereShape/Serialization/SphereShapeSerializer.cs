using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class SphereShapeSerializer
    {
        public static byte[] Serialize(PBSphereShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBSphereShape Deserialize(object data)
        {
            return PBSphereShape.Parser.ParseFrom((byte[])data);
        }
    }
}