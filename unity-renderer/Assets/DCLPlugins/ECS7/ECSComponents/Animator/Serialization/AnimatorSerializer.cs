using Google.Protobuf;

namespace DCL.ECSComponents
{
    public static class AnimatorSerializer
    {
        public static byte[] Serialize(PBAnimator model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBAnimator Deserialize(object data)
        {
            return PBAnimator.Parser.ParseFrom((byte[])data);
        }
    }
}