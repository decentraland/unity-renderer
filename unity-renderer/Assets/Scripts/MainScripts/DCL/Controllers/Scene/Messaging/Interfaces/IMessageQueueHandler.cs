using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DCL
{
    public interface IMessageQueueHandler
    {
        void EnqueueSceneMessage(QueuedSceneMessage_Scene message);
        ConcurrentQueue<QueuedSceneMessage_Scene> sceneMessagesPool { get; }
    }
}