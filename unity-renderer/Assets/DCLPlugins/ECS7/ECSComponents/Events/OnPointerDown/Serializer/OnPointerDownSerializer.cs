using DCL.ECSComponents;
using Google.Protobuf;

namespace DCLPlugins.ECS7.ECSComponents.Events.OnPointerDown.Serializer
{
    public static class OnPointerDownSerializer
    {
        public static byte[] Serialize(PBOnPointerDown model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBOnPointerDown Deserialize(object data)
        {
            return PBOnPointerDown.Parser.ParseFrom((byte[])data);
        }
    }
}