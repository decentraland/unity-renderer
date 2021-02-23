using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MessagingControllersManager : IMessagingControllersManager
    {
        public static bool VERBOSE = false;

        private const float MAX_GLOBAL_MSG_BUDGET = 0.006f;
        private const float MAX_SYSTEM_MSG_BUDGET_FOR_FAR_SCENES = 0.003f;

        private const float GLTF_BUDGET_MAX = 0.033f;
        private const float GLTF_BUDGET_MIN = 0.008f;

        public const string GLOBAL_MESSAGING_CONTROLLER = "global_messaging_controller";

        public Dictionary<string, MessagingController> messagingControllers { get; set; } = new Dictionary<string, MessagingController>();

        private Coroutine mainCoroutine;

        public bool hasPendingMessages => pendingMessagesCount > 0;

        public float timeBudgetCounter = MAX_GLOBAL_MSG_BUDGET;
        public int pendingMessagesCount;
        public int pendingInitMessagesCount;
        public long processedInitMessagesCount;

        public bool isRunning
        {
            get { return mainCoroutine != null; }
        }

        public bool paused { get; set; }

        private Dictionary<string, MessagingController> globalSceneControllers = new Dictionary<string, MessagingController>();
        private List<MessagingController> sortedControllers = new List<MessagingController>();
        private List<MessagingBus> busesToProcess = new List<MessagingBus>();
        private int busesToProcessCount = 0;
        private int sortedControllersCount = 0;

        private MessagingController globalController = null;
        private MessagingController currentSceneController = null;

        public void Initialize(IMessageProcessHandler messageHandler)
        {
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER] = new MessagingController(messageHandler, GLOBAL_MESSAGING_CONTROLLER);

            if (!string.IsNullOrEmpty(GLOBAL_MESSAGING_CONTROLLER))
                messagingControllers.TryGetValue(GLOBAL_MESSAGING_CONTROLLER, out globalController);

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
            IWorldState worldState = Environment.i.world.state;
            string currentSceneId = worldState.currentSceneId;
            List<ParcelScene> scenesSortedByDistance = worldState.scenesSortedByDistance;

            int count = scenesSortedByDistance.Count; // we need to retrieve list count everytime because it
            // may change after a yield return

            sortedControllers.Clear();

            if (!string.IsNullOrEmpty(currentSceneId) && messagingControllers.ContainsKey(currentSceneId))
                currentSceneController = messagingControllers[currentSceneId];

            for (int i = 0; i < count; i++)
            {
                string controllerId = scenesSortedByDistance[i].sceneData.id;

                if (controllerId != currentSceneId)
                {
                    if (!messagingControllers.ContainsKey(controllerId))
                        continue;

                    sortedControllers.Add(messagingControllers[controllerId]);
                }
            }

            sortedControllersCount = sortedControllers.Count;

            bool globalSceneControllerActive = globalSceneControllers.Count > 0;
            bool globalControllerActive = globalController != null && globalController.enabled;
            bool currentSceneControllerActive = currentSceneController != null && currentSceneController.enabled;

            bool atLeastOneControllerShouldBeProcessed = globalSceneControllerActive || globalControllerActive || currentSceneControllerActive || sortedControllersCount > 0;

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


        public bool ContainsController(string sceneId)
        {
            return messagingControllers.ContainsKey(sceneId);
        }

        public void AddController(IMessageProcessHandler messageHandler, string sceneId, bool isGlobal = false)
        {
            if (!messagingControllers.ContainsKey(sceneId))
                messagingControllers[sceneId] = new MessagingController(messageHandler, sceneId);

            if (isGlobal && !string.IsNullOrEmpty(sceneId))
            {
                messagingControllers.TryGetValue(sceneId, out MessagingController newGlobalSceneController);

                if (!globalSceneControllers.ContainsKey(sceneId))
                    globalSceneControllers.Add(sceneId, newGlobalSceneController);
            }

            PopulateBusesToBeProcessed();
        }

        public void AddControllerIfNotExists(IMessageProcessHandler messageHandler, string sceneId, bool isGlobal = false)
        {
            if (!ContainsController(sceneId))
                AddController(messageHandler, sceneId, isGlobal);
        }

        public void RemoveController(string sceneId)
        {
            if (messagingControllers.ContainsKey(sceneId))
            {
                // In case there is any pending message from a scene being unloaded we decrease the count accordingly
                pendingMessagesCount -= messagingControllers[sceneId].messagingBuses[MessagingBusType.INIT].pendingMessagesCount +
                                        messagingControllers[sceneId].messagingBuses[MessagingBusType.UI].pendingMessagesCount +
                                        messagingControllers[sceneId].messagingBuses[MessagingBusType.SYSTEM].pendingMessagesCount;

                DisposeController(messagingControllers[sceneId]);
                messagingControllers.Remove(sceneId);
            }

            globalSceneControllers.Remove(sceneId);
        }

        void DisposeController(MessagingController controller)
        {
            controller.Stop();
            controller.Dispose();
        }

        public void Enqueue(bool isUiBus, QueuedSceneMessage_Scene queuedMessage)
        {
            messagingControllers[queuedMessage.sceneId].Enqueue(isUiBus, queuedMessage, out MessagingBusType busId);
        }

        public void ForceEnqueueToGlobal(MessagingBusType busId, QueuedSceneMessage queuedMessage)
        {
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER].ForceEnqueue(busId, queuedMessage);
        }

        public void SetSceneReady(string sceneId)
        {
            // Start processing SYSTEM queue
            if (messagingControllers.ContainsKey(sceneId))
            {
                // Start processing SYSTEM queue
                MessagingController sceneMessagingController = messagingControllers[sceneId];
                sceneMessagingController.StartBus(MessagingBusType.SYSTEM);
                sceneMessagingController.StartBus(MessagingBusType.UI);
                sceneMessagingController.StopBus(MessagingBusType.INIT);
            }
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

                timeBudgetCounter = CommonScriptableObjects.rendererState.Get() ? MAX_GLOBAL_MSG_BUDGET : float.MaxValue;

                for (int i = 0; i < busesToProcessCount; ++i)
                {
                    MessagingBus bus = busesToProcess[i];

                    if (ProcessBus(bus))
                        break;
                }

                if (pendingInitMessagesCount == 0)
                {
                    UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = Mathf.Clamp(timeBudgetCounter, GLTF_BUDGET_MIN, GLTF_BUDGET_MAX) * 1000f;
                }
                else
                {
                    UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = 0;
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

            if (controller.uiBus.pendingMessagesCount != 0) return;
            if (controller.initBus.pendingMessagesCount != 0) return;
            if (controller.systemBus.pendingMessagesCount != 0) return;

            controller.enabled = false;
        }
    }
}