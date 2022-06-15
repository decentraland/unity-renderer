namespace RPC.Services.Interfaces
{
    public interface ICRDTServiceContext
    {
        void PushNotification(string sceneId, byte[] data);
    }
}