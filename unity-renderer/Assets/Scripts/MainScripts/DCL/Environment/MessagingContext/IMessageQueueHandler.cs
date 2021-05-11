using System.Collections.Generic;

namespace DCL
{
    public interface IMessageQueueHandler
    {
        void EnqueueSceneMessage(QueuedSceneMessage_Scene message);
        Queue<QueuedSceneMessage_Scene> sceneMessagesPool { get; }
    }
}