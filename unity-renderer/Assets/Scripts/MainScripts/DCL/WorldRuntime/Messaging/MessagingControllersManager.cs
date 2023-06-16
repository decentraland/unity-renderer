using DCL.Controllers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using UnityEngine;

namespace DCL
{
    public class MessagingControllersManager : IMessagingControllersManager
    {
        public static bool VERBOSE = false;

        private const float MAX_GLOBAL_MSG_BUDGET = 0.02f;
        private const float MAX_SYSTEM_MSG_BUDGET_FOR_FAR_SCENES = 0.003f;

        private const float GLTF_BUDGET_MAX = 0.033f;
        private const float GLTF_BUDGET_MIN = 0.008f;

        private const int GLOBAL_MESSAGING_CONTROLLER_SCENE_NUMBER = 999999;

        public ConcurrentDictionary<int, MessagingController> messagingControllers { get; set; } =
            new ConcurrentDictionary<int, MessagingController>();

        private Coroutine mainCoroutine;

        public bool hasPendingMessages => pendingMessagesCount > 0;

        public float timeBudgetCounter { get; set; } = MAX_GLOBAL_MSG_BUDGET;
        public long processedInitMessagesCount { get; set; }
        public int pendingMessagesCount { get; set; }
        public int pendingInitMessagesCount { get; set; }

        public bool isRunning => mainCoroutine != null;

        public bool paused { get; set; }

        private readonly ConcurrentDictionary<int, MessagingController> globalSceneControllers =
            new ConcurrentDictionary<int, MessagingController>();

        private readonly List<MessagingController> sortedControllers = new List<MessagingController>();
        private readonly List<MessagingBus> busesToProcess = new List<MessagingBus>();
        private int busesToProcessCount = 0;
        private int sortedControllersCount = 0;

        private MessagingController globalController = null;
        private MessagingController currentSceneController = null;

        private IMessageProcessHandler messageHandler;

        public MessagingControllersManager(IMessageProcessHandler messageHandler = null)
        {
            this.messageHandler = messageHandler;
        }

        public void Initialize()
        {
            if (messageHandler == null)
                messageHandler = Environment.i.world.sceneController;

            globalController = new MessagingController(this, messageHandler, GLOBAL_MESSAGING_CONTROLLER_SCENE_NUMBER);
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER_SCENE_NUMBER] = globalController;

            Environment.i.world.sceneController.OnSortScenes += MarkBusesDirty;

            if (mainCoroutine == null)
            {
                mainCoroutine = CoroutineStarter.Start(ProcessMessages());
            }
        }

        bool populateBusesDirty = true;

        public void MarkBusesDirty()
        {
            populateBusesDirty = true;
        }

        public void PopulateBusesToBeProcessed()
        {
            lock (sortedControllers)
            lock (busesToProcess)
            {
                IWorldState worldState = Environment.i.world.state;
                int currentSceneNumber = worldState.GetCurrentSceneNumber();
                var scenesSortedByDistance = worldState.GetScenesSortedByDistance();

                int count = scenesSortedByDistance.Count; // we need to retrieve list count everytime because it
                // may change after a yield return

                sortedControllers.Clear();

                if (currentSceneNumber > 0 && messagingControllers.ContainsKey(currentSceneNumber))
                    currentSceneController = messagingControllers[currentSceneNumber];

                for (int i = 0; i < count; i++)
                {
                    int controllerSceneNumber = scenesSortedByDistance[i].sceneData.sceneNumber;

                    if (controllerSceneNumber != currentSceneNumber)
                    {
                        if (!messagingControllers.ContainsKey(controllerSceneNumber))
                            continue;

                        sortedControllers.Add(messagingControllers[controllerSceneNumber]);
                    }
                }

                sortedControllersCount = sortedControllers.Count;

                bool globalSceneControllerActive = globalSceneControllers.Count > 0;
                bool globalControllerActive = globalController != null && globalController.enabled;
                bool currentSceneControllerActive = currentSceneController != null && currentSceneController.enabled;

                bool atLeastOneControllerShouldBeProcessed = globalSceneControllerActive || globalControllerActive ||
                                                             currentSceneControllerActive || sortedControllersCount > 0;

                if (!atLeastOneControllerShouldBeProcessed)
                    return;

                busesToProcess.Clear();

                //-------------------------------------------------------------------------------------------
                // Global scenes
                using (var globalScenecontrollersIterator = globalSceneControllers.GetEnumerator())
                {
                    while (globalScenecontrollersIterator.MoveNext())
                    {
                        busesToProcess.Add(globalScenecontrollersIterator.Current.Value.uiBus);
                        busesToProcess.Add(globalScenecontrollersIterator.Current.Value.initBus);
                        busesToProcess.Add(globalScenecontrollersIterator.Current.Value.systemBus);
                    }
                }

                if (globalControllerActive)
                {
                    busesToProcess.Add(globalController.initBus);
                }

                if (currentSceneControllerActive)
                {
                    busesToProcess.Add(currentSceneController.initBus);
                    busesToProcess.Add(currentSceneController.uiBus);
                    busesToProcess.Add(currentSceneController.systemBus);
                }

                for (int i = 0; i < sortedControllersCount; ++i)
                {
                    MessagingController msgController = sortedControllers[i];

                    busesToProcess.Add(msgController.initBus);
                    busesToProcess.Add(msgController.uiBus);
                }

                for (int i = 0; i < sortedControllersCount; ++i)
                {
                    MessagingController msgController = sortedControllers[i];
                    busesToProcess.Add(msgController.systemBus);
                }

                busesToProcessCount = busesToProcess.Count;
            }
        }

        public void Dispose()
        {
            if (mainCoroutine != null)
            {
                CoroutineStarter.Stop(mainCoroutine);
                mainCoroutine = null;
            }

            using (var controllersIterator = messagingControllers.GetEnumerator())
            {
                while (controllersIterator.MoveNext())
                {
                    controllersIterator.Current.Value.Stop();
                    DisposeController(controllersIterator.Current.Value);
                }
            }

            Environment.i.world.sceneController.OnSortScenes -= PopulateBusesToBeProcessed;

            messagingControllers.Clear();
        }

        public bool ContainsController(int sceneNumber)
        {
            return messagingControllers.ContainsKey(sceneNumber);
        }

        public void AddController(IMessageProcessHandler messageHandler, int sceneNumber, bool isGlobal = false)
        {
            if (!messagingControllers.ContainsKey(sceneNumber))
            {
                messagingControllers.TryAdd(sceneNumber, new MessagingController(this, messageHandler, sceneNumber));
            }

            if (isGlobal && sceneNumber > 0)
            {
                messagingControllers.TryGetValue(sceneNumber, out MessagingController newGlobalSceneController);

                if (!globalSceneControllers.ContainsKey(sceneNumber))
                    globalSceneControllers.TryAdd(sceneNumber, newGlobalSceneController);
            }

            PopulateBusesToBeProcessed();
        }

        public void AddControllerIfNotExists(IMessageProcessHandler messageHandler, int sceneNumber,
            bool isGlobal = false)
        {
            if (!ContainsController(sceneNumber))
                AddController(messageHandler, sceneNumber, isGlobal);
        }

        public void RemoveController(int sceneNumber)
        {
            if (messagingControllers.ContainsKey(sceneNumber))
            {
                // In case there is any pending message from a scene being unloaded we decrease the count accordingly
                pendingMessagesCount -= messagingControllers[sceneNumber].messagingBuses[MessagingBusType.INIT]
                                                                         .pendingMessagesCount +
                                        messagingControllers[sceneNumber].messagingBuses[MessagingBusType.UI]
                                                                         .pendingMessagesCount +
                                        messagingControllers[sceneNumber].messagingBuses[MessagingBusType.SYSTEM]
                                                                         .pendingMessagesCount;

                DisposeController(messagingControllers[sceneNumber]);
                messagingControllers.TryRemove(sceneNumber, out MessagingController _);
            }

            globalSceneControllers.TryRemove(sceneNumber, out MessagingController _);
        }

        void DisposeController(MessagingController controller)
        {
            controller.Stop();
            controller.Dispose();
        }

        public void Enqueue(bool isUiBus, QueuedSceneMessage_Scene queuedMessage)
        {
            PerformanceAnalytics.MessagesEnqueuedTracker.Track();
            messagingControllers[queuedMessage.sceneNumber].Enqueue(isUiBus, queuedMessage, out MessagingBusType busId);
        }

        public void ForceEnqueueToGlobal(MessagingBusType busId, QueuedSceneMessage queuedMessage)
        {
            PerformanceAnalytics.MessagesEnqueuedTracker.Track();
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER_SCENE_NUMBER].ForceEnqueue(busId, queuedMessage);
        }

        public void SetSceneReady(int sceneNumber)
        {
            // Start processing SYSTEM queue
            if (messagingControllers.ContainsKey(sceneNumber))
            {
                // Start processing SYSTEM queue
                MessagingController sceneMessagingController = messagingControllers[sceneNumber];
                sceneMessagingController.StartBus(MessagingBusType.SYSTEM);
                sceneMessagingController.StartBus(MessagingBusType.UI);
                sceneMessagingController.StopBus(MessagingBusType.INIT);
            }
        }

        public bool HasScenePendingMessages(int sceneNumber)
        {
            if (!messagingControllers.TryGetValue(sceneNumber, out MessagingController newGlobalSceneController))
                return false;

            return newGlobalSceneController.initBus.hasPendingMessages
                   || newGlobalSceneController.systemBus.hasPendingMessages
                   || newGlobalSceneController.uiBus.hasPendingMessages;
        }

        IEnumerator ProcessMessages()
        {
            while (true)
            {
                if (paused)
                {
                    yield return null;

                    continue;
                }

                if (populateBusesDirty)
                {
                    PopulateBusesToBeProcessed();
                    populateBusesDirty = false;
                }

                timeBudgetCounter =
                    CommonScriptableObjects.rendererState.Get() ? MAX_GLOBAL_MSG_BUDGET : float.MaxValue;

                for (int i = 0; i < busesToProcessCount; ++i)
                {
                    MessagingBus bus;

                    lock (busesToProcess)
                    {
                        bus = busesToProcess[i];
                    }

                    if (ProcessBus(bus))
                        break;
                }

                yield return null;
            }
        }

        bool ProcessBus(MessagingBus bus)
        {
            if (!bus.enabled || bus.pendingMessagesCount <= 0)
                return false;

            float startTime = Time.realtimeSinceStartup;

            float timeBudget = timeBudgetCounter;

            //TODO(Brian): We should use the returning yieldReturn IEnumerator and MoveNext() it manually each frame to
            //             account the coroutine processing into the budget. Until we do that we just skip it.
            bus.ProcessQueue(timeBudget, out _);
            RefreshControllerEnabledState(bus.owner);

            timeBudgetCounter -= Time.realtimeSinceStartup - startTime;

            if (timeBudgetCounter <= 0)
                return true;

            return false;
        }

        private void RefreshControllerEnabledState(MessagingController controller)
        {
            if (controller == null || !controller.enabled)
                return;

            if (controller.uiBus.pendingMessagesCount != 0)
                return;

            if (controller.initBus.pendingMessagesCount != 0)
                return;

            if (controller.systemBus.pendingMessagesCount != 0)
                return;

            controller.enabled = false;
        }
    }
}
