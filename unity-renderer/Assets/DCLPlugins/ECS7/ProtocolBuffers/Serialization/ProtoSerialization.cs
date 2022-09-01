using Google.Protobuf;

public class ProtoSerialization
{
    public static byte[] Serialize<T>(T model) where T : IMessage
    {
        int size = model.CalculateSize();
        byte[] buffer = new byte[size];
        CodedOutputStream output = new CodedOutputStream(buffer);
        model.WriteTo(output);
        return buffer;
    }

    public static T Deserialize<T>(object data) where T : IMessage<T>, new()
    {
        T ret = new T();
        ret.MergeFrom((byte[])data);
        return ret;
    }
}