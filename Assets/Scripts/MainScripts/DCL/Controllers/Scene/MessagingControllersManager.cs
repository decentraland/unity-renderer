using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MessagingControllersManager : Singleton<MessagingControllersManager>
    {
        public static bool VERBOSE = false;

        private const float MAX_INIT_TIME_BUDGET = 0.3f;
        private const float GLOBAL_MAX_MSG_BUDGET = 0.008f;
        private const float GLTF_BUDGET_MAX = 0.006f;
        private const float GLTF_BUDGET_MIN = 0.0001f;

        public const string GLOBAL_MESSAGING_CONTROLLER = "global_messaging_controller";

        public Dictionary<string, MessagingController> messagingControllers = new Dictionary<string, MessagingController>();
        private string globalSceneId = null;

        private Coroutine mainCoroutine;

        public bool hasPendingMessages => pendingMessagesCount > 0;
        public MessageThrottlingController throttler;

        public int pendingMessagesCount
        {
            get
            {
                int total = 0;
                using (var iterator = messagingControllers.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        total += iterator.Current.Value.pendingMessagesCount;
                    }
                }
                return total;
            }
        }

        public int pendingInitMessagesCount
        {
            get
            {
                int total = 0;
                using (var iterator = messagingControllers.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        total += iterator.Current.Value.pendingInitMessagesCount;
                    }
                }
                return total;
            }
        }

        public long processedInitMessagesCount
        {
            get
            {
                long total = 0;
                using (var iterator = messagingControllers.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        total += iterator.Current.Value.processedInitMessagesCount;
                    }
                }
                return total;
            }
        }

        public void Initialize(IMessageHandler messageHandler)
        {
            throttler = new MessageThrottlingController();
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER] = new MessagingController(messageHandler, GLOBAL_MESSAGING_CONTROLLER);

            if (mainCoroutine == null)
            {
                mainCoroutine = SceneController.i.StartCoroutine(ProcessMessages());
            }
        }

        public bool isRunning { get { return mainCoroutine != null; } }

        public void Start()
        {
        }

        public void Stop()
        {
            if (mainCoroutine != null)
            {
                SceneController.i.StopCoroutine(mainCoroutine);
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

            messagingControllers.Clear();
        }


        public bool ContainsController(string sceneId)
        {
            return messagingControllers.ContainsKey(sceneId);
        }

        public void AddController(IMessageHandler messageHandler, string sceneId, bool isGlobal = false)
        {
            if (!messagingControllers.ContainsKey(sceneId))
                messagingControllers[sceneId] = new MessagingController(messageHandler, sceneId);

            if (isGlobal)
                globalSceneId = sceneId;
        }

        public void RemoveController(string sceneId)
        {
            if (messagingControllers.ContainsKey(sceneId))
            {
                DisposeController(messagingControllers[sceneId]);
                messagingControllers.Remove(sceneId);
            }
        }

        void DisposeController(MessagingController controller)
        {
            controller.Stop();
            controller.Dispose();
        }

        public string Enqueue(ParcelScene scene, MessagingBus.QueuedSceneMessage_Scene queuedMessage)
        {
            return messagingControllers[queuedMessage.sceneId].Enqueue(scene, queuedMessage);
        }

        public void ForceEnqueueToGlobal(string busId, MessagingBus.QueuedSceneMessage queuedMessage)
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
                sceneMessagingController.StartBus(MessagingBusId.SYSTEM);
                sceneMessagingController.StartBus(MessagingBusId.UI);
                sceneMessagingController.StopBus(MessagingBusId.INIT);
            }
        }

        public void UpdateThrottling()
        {
            List<ParcelScene> scenesSortedByDistance = SceneController.i.scenesSortedByDistance;
            int count = scenesSortedByDistance.Count;
            float prevTimeBudget = MAX_INIT_TIME_BUDGET;

            // Let's calculate the time budget
            float timeBudget = throttler.Update(
                pendingInitMessagesCount,
                processedInitMessagesCount,
                prevTimeBudget
             );

            // First we process Load/Unload scene messages
            prevTimeBudget = messagingControllers[GLOBAL_MESSAGING_CONTROLLER].UpdateTimeBudget(MessagingBusId.INIT, timeBudget, prevTimeBudget);

            // If we already have a messaging controller for global scene,
            // we update throttling
            if (!string.IsNullOrEmpty(globalSceneId) && messagingControllers.ContainsKey(globalSceneId))
            {
                prevTimeBudget = messagingControllers[globalSceneId].UpdateTimeBudget(MessagingBusId.INIT, timeBudget, prevTimeBudget);
            }

            // Update throttling to the rest of the messaging controllers
            for (int i = 0; i < count; i++)
            {
                ParcelScene scene = scenesSortedByDistance[i];
                if (messagingControllers.ContainsKey(scene.sceneData.id))
                {
                    prevTimeBudget = messagingControllers[scene.sceneData.id].UpdateTimeBudget(MessagingBusId.INIT, timeBudget, prevTimeBudget);
                }
            }

            if (pendingInitMessagesCount == 0)
            {
                UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = Mathf.Clamp(prevTimeBudget, GLTF_BUDGET_MIN, GLTF_BUDGET_MAX) * 1000f;
            }
            else
            {
                UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = 0;
            }
        }

        IEnumerator ProcessMessages()
        {
            float prevTimeBudget = GLOBAL_MAX_MSG_BUDGET;
            int count = 0;
            List<ParcelScene> scenesSortedByDistance = SceneController.i.scenesSortedByDistance;
            IEnumerator yieldReturn;

            while (true)
            {
                bool isGlobalSceneAvailable = !string.IsNullOrEmpty(globalSceneId) && messagingControllers.ContainsKey(globalSceneId);
                //-------------------------------------------------------------------------------------------
                // Global scene UI
                if (isGlobalSceneAvailable)
                {
                    if (ProcessBus(messagingControllers[globalSceneId], MessagingBusId.UI, ref prevTimeBudget, out yieldReturn))
                        yield return yieldReturn;
                }

                //-------------------------------------------------------------------------------------------
                // Global Controller INIT
                if (ProcessBus(messagingControllers[GLOBAL_MESSAGING_CONTROLLER], MessagingBusId.INIT, ref prevTimeBudget, out yieldReturn))
                    yield return yieldReturn;

                //-------------------------------------------------------------------------------------------
                // Global scene INIT
                if (isGlobalSceneAvailable)
                {
                    if (ProcessBus(messagingControllers[globalSceneId], MessagingBusId.INIT, ref prevTimeBudget, out yieldReturn))
                        yield return yieldReturn;
                }

                //-------------------------------------------------------------------------------------------
                // Rest of the scenes INIT
                count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it 
                                                        // may change after a yield return
                for (int i = 0; i < count; i++)
                {
                    string controllerId = scenesSortedByDistance[i].sceneData.id;
                    if (messagingControllers.ContainsKey(controllerId))
                    {
                        if (ProcessBus(messagingControllers[controllerId], MessagingBusId.INIT, ref prevTimeBudget, out yieldReturn))
                            yield return yieldReturn;
                    }
                }

                //-------------------------------------------------------------------------------------------
                // Rest of the scenes UI
                count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it 
                                                        // may change after a yield return
                for (int i = 0; i < count; i++)
                {
                    string controllerId = scenesSortedByDistance[i].sceneData.id;
                    if (messagingControllers.ContainsKey(controllerId))
                    {
                        if (ProcessBus(messagingControllers[controllerId], MessagingBusId.UI, ref prevTimeBudget, out yieldReturn))
                            yield return yieldReturn;
                    }
                }

                //-------------------------------------------------------------------------------------------
                // Global scene SYSTEM
                if (isGlobalSceneAvailable)
                {
                    if (ProcessBus(messagingControllers[globalSceneId], MessagingBusId.SYSTEM, ref prevTimeBudget, out yieldReturn))
                        yield return yieldReturn;
                }

                //-------------------------------------------------------------------------------------------
                // Rest of the scenes SYSTEM
                count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it 
                                                        // may change after a yield return
                for (int i = 0; i < count; i++)
                {
                    string controllerId = scenesSortedByDistance[i].sceneData.id;
                    if (messagingControllers.ContainsKey(controllerId))
                    {
                        if (ProcessBus(messagingControllers[controllerId], MessagingBusId.SYSTEM, ref prevTimeBudget, out yieldReturn))
                            yield return yieldReturn;
                    }
                }
            }
        }

        bool ProcessBus(MessagingController controller, string busId, ref float prevTimeBudget, out IEnumerator yieldReturn)
        {
            float startTime = DCLTime.realtimeSinceStartup;

            if (controller.messagingBuses[busId].isRunning)
            {
                yieldReturn = null;

                MessagingBus bus = controller.messagingBuses[busId];

                float timeBudget = Mathf.Max(bus.timeBudget, bus.budgetMin);

                if (RenderingController.i.renderingEnabled)
                    timeBudget = Mathf.Min(timeBudget, prevTimeBudget);

                if (VERBOSE && timeBudget == 0)
                {
                    string finalTag = SceneController.i.TryToGetSceneCoordsID(bus.debugTag);
                    Debug.Log($"#{bus.processedMessagesCount} ... bus = {finalTag}, id = {bus.id}... timeBudget is zero!!!");
                }

                if (bus.ProcessQueue(timeBudget, out yieldReturn))
                {
                    prevTimeBudget = GLOBAL_MAX_MSG_BUDGET;
                    return true;
                }

            }
            else
            {
                yieldReturn = null;
            }

            prevTimeBudget -= DCLTime.realtimeSinceStartup - startTime;

            if (prevTimeBudget <= 0)
            {
                prevTimeBudget = GLOBAL_MAX_MSG_BUDGET;
                return true;
            }

            return false;
        }
    }
}
