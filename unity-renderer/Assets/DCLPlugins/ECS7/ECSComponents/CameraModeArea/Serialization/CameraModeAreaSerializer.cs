using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class CameraModeAreaSerializer
    {
        public static byte[] Serialize(PBCameraModeArea model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBCameraModeArea Deserialize(object data)
        {
            return PBCameraModeArea.Parser.ParseFrom((byte[])data);
        }
    }
}