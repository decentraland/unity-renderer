using DCL.ECSComponents;
using Google.Protobuf;

namespace DCLPlugins.ECSComponents
{
    public static class OnPointerResultSerializer
    {
        public static byte[] Serialize(PBOnPointerUpResult model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static byte[] Serialize(PBOnPointerDownResult model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }
        
        public static PBOnPointerDownResult DeserializeDown(object data)
        {
            return PBOnPointerDownResult.Parser.ParseFrom((byte[])data);
        }
        
        public static PBOnPointerUpResult DeserializeUp(object data)
        {
            return PBOnPointerUpResult.Parser.ParseFrom((byte[])data);
        }
    }
}