using System;
using System.Collections.Generic;

namespace DCL
{
    public interface IMessagingControllersManager : IService
    {
        float timeBudgetCounter { get; set; }
        bool hasPendingMessages { get; }
        bool isRunning { get; }
        bool paused { get; set; }
        int pendingInitMessagesCount { get; set; }
        long processedInitMessagesCount { get; set; }
        int pendingMessagesCount { get; set; }
        void MarkBusesDirty();
        void PopulateBusesToBeProcessed();
        bool ContainsController(int sceneNumber);
        void AddController(IMessageProcessHandler messageHandler, int sceneNumber, bool isGlobal = false);
        void AddControllerIfNotExists(IMessageProcessHandler messageHandler, int sceneNumber, bool isGlobal = false);
        void RemoveController(int sceneNumber);
        void Enqueue(bool isUiBus, QueuedSceneMessage_Scene queuedMessage);
        void ForceEnqueueToGlobal(MessagingBusType busId, QueuedSceneMessage queuedMessage);
        void SetSceneReady(int sceneNumber);
        bool HasScenePendingMessages(int sceneNumber);
    }
}
