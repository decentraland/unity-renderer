using System.Collections.Generic;

namespace DCL
{
    public interface IMessageQueueHandler
    {
        void EnqueueSceneMessage(MessagingBus.QueuedSceneMessage_Scene message);
        Queue<MessagingBus.QueuedSceneMessage_Scene> sceneMessagesPool { get; }
    }
}