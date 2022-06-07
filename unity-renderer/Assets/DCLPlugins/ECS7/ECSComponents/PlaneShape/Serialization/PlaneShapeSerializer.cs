using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class PlaneShapeSerializer
    {
        public static byte[] Serialize(PBPlaneShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBPlaneShape Deserialize(object data)
        {
            return PBPlaneShape.Parser.ParseFrom((byte[])data);
        }
    }
}