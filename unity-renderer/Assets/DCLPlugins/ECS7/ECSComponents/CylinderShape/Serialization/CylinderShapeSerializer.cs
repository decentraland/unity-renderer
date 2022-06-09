using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class CylinderShapeSerializer
    {
        public static byte[] Serialize(PBCylinderShape model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBCylinderShape Deserialize(object data)
        {
            return PBCylinderShape.Parser.ParseFrom((byte[])data);
        }
    }
}