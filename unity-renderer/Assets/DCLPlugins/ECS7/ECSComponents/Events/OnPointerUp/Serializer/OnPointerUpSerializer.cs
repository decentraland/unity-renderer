using DCL.ECSComponents;
using Google.Protobuf;

namespace DCLPlugins.ECS7.ECSComponents.Events.OnPointerDown.OnPointerUp.Serializer
{
    public class OnPointerUpSerializer
    {
        public static byte[] Serialize(PBOnPointerUp model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBOnPointerUp Deserialize(object data) { return PBOnPointerUp.Parser.ParseFrom((byte[])data); }
    }
}