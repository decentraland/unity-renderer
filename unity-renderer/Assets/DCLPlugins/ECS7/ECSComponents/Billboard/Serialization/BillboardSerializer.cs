using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class BillboardSerializer
    {
        public static byte[] Serialize(PBBillboard model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBBillboard Deserialize(object data)
        {
            return PBBillboard.Parser.ParseFrom((byte[])data);
        }
    }
}