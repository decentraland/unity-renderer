using System.Collections.Generic;
using DCL;
using RPC.Services.Interfaces;

namespace RPC.Services
{
    public class CRDTServiceContext : ICRDTServiceContext
    {
        public readonly Queue<(string, byte[])> notifications = new Queue<(string, byte[])>();
        public IMessageQueueHandler messageQueueHanlder;

        public void PushNotification(string sceneId, byte[] data)
        {
            notifications.Enqueue((sceneId, data));
        }
    }
}