using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class CameraModeSerializer
    {
        public static byte[] Serialize(PBCameraMode model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBCameraMode Deserialize(object data)
        {
            return PBCameraMode.Parser.ParseFrom((byte[])data);
        }
    }
}