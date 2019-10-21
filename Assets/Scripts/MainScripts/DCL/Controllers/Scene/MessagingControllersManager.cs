using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MessagingControllersManager : Singleton<MessagingControllersManager>
    {
        public static bool VERBOSE = false;

        private const float GLOBAL_MAX_MSG_BUDGET = 0.033f;
        private const float GLOBAL_MAX_MSG_BUDGET_WHEN_LOADING = 1f;
        private const float GLOBAL_MIN_MSG_BUDGET_WHEN_LOADING = 1f;
        public const float UI_MSG_BUS_BUDGET_MAX = 0.016f;
        public const float INIT_MSG_BUS_BUDGET_MAX = 0.033f;
        public const float SYSTEM_MSG_BUS_BUDGET_MAX = 0.016f;
        public const float MSG_BUS_BUDGET_MIN = 0.00001f;
        private const float GLTF_BUDGET_MAX = 0.033f;
        private const float GLTF_BUDGET_MIN = 0.008f;

        public const string GLOBAL_MESSAGING_CONTROLLER = "global_messaging_controller";

        public Dictionary<string, MessagingController> messagingControllers = new Dictionary<string, MessagingController>();
        private string globalSceneId = null;

        private Coroutine mainCoroutine;

        public bool hasPendingMessages => pendingMessagesCount > 0;
        public MessageThrottlingController throttler;

        public int pendingMessagesCount;
        public int pendingInitMessagesCount;
        public long processedInitMessagesCount;

        public bool isRunning { get { return mainCoroutine != null; } }

        public void Initialize(IMessageHandler messageHandler)
        {
            throttler = new MessageThrottlingController();
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER] = new MessagingController(messageHandler, GLOBAL_MESSAGING_CONTROLLER);

            if (mainCoroutine == null)
            {
                mainCoroutine = SceneController.i.StartCoroutine(ProcessMessages());
            }
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
            {
                messagingControllers[sceneId] = new MessagingController(messageHandler, sceneId);
            }

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
            string busId = "";

            messagingControllers[queuedMessage.sceneId].Enqueue(scene, queuedMessage, out busId);

            return busId;
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
            if (pendingInitMessagesCount == 0)
            {
                UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = Mathf.Clamp(throttler.currentTimeBudget, GLTF_BUDGET_MIN, GLTF_BUDGET_MAX) * 1000f;
            }
            else
            {
                UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = 0;
            }
        }

        IEnumerator ProcessMessages()
        {
            float prevTimeBudget;
            int count = 0;
            List<ParcelScene> scenesSortedByDistance = SceneController.i.scenesSortedByDistance;
            IEnumerator yieldReturn;
            float start;
            string currentSceneId = "";

            while (true)
            {
                prevTimeBudget = INIT_MSG_BUS_BUDGET_MAX;
                start = Time.unscaledTime;

                currentSceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                // When breaking this second loop, we skip a frame
                while (true)
                {
                    bool isGlobalSceneAvailable = !string.IsNullOrEmpty(globalSceneId) && messagingControllers.ContainsKey(globalSceneId);
                    //-------------------------------------------------------------------------------------------
                    // Global scene UI
                    if (isGlobalSceneAvailable)
                    {
                        if (ProcessBus(messagingControllers[globalSceneId], MessagingBusId.UI, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Global Controller INIT
                    if (ProcessBus(messagingControllers[GLOBAL_MESSAGING_CONTROLLER], MessagingBusId.INIT, ref prevTimeBudget, out yieldReturn))
                        break;

                    //-------------------------------------------------------------------------------------------
                    // Global scene INIT
                    if (isGlobalSceneAvailable)
                    {
                        if (ProcessBus(messagingControllers[globalSceneId], MessagingBusId.INIT, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Current Scene INIT, UI and SYSTEM
                    if (!string.IsNullOrEmpty(currentSceneId) && messagingControllers.ContainsKey(currentSceneId))
                    {
                        if (ProcessBus(messagingControllers[currentSceneId], MessagingBusId.INIT, ref prevTimeBudget, out yieldReturn))
                            break;
                        if (ProcessBus(messagingControllers[currentSceneId], MessagingBusId.UI, ref prevTimeBudget, out yieldReturn))
                            break;
                        if (ProcessBus(messagingControllers[currentSceneId], MessagingBusId.SYSTEM, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Rest of the scenes INIT
                    count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it 
                                                            // may change after a yield return
                    bool shouldRestart = false;
                    for (int i = 0; i < count; i++)
                    {
                        string controllerId = scenesSortedByDistance[i].sceneData.id;
                        if (controllerId != currentSceneId && messagingControllers.ContainsKey(controllerId))
                        {
                            if (ProcessBus(messagingControllers[controllerId], MessagingBusId.INIT, ref prevTimeBudget, out yieldReturn))
                            {
                                shouldRestart = true;
                                break;
                            }
                        }
                    }

                    if (shouldRestart)
                        break;

                    //-------------------------------------------------------------------------------------------
                    // Rest of the scenes UI
                    count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it 
                                                            // may change after a yield return
                    for (int i = 0; i < count; i++)
                    {
                        string controllerId = scenesSortedByDistance[i].sceneData.id;
                        if (controllerId != currentSceneId && messagingControllers.ContainsKey(controllerId))
                        {
                            if (ProcessBus(messagingControllers[controllerId], MessagingBusId.UI, ref prevTimeBudget, out yieldReturn))
                            {
                                shouldRestart = true;
                                break;
                            }
                        }
                    }

                    if (shouldRestart)
                        break;

                    //-------------------------------------------------------------------------------------------
                    // Global scene SYSTEM
                    if (isGlobalSceneAvailable)
                    {
                        if (ProcessBus(messagingControllers[globalSceneId], MessagingBusId.SYSTEM, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Rest of the scenes SYSTEM
                    count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it 
                                                            // may change after a yield return
                    for (int i = 0; i < count; i++)
                    {
                        string controllerId = scenesSortedByDistance[i].sceneData.id;
                        if (controllerId != currentSceneId && messagingControllers.ContainsKey(controllerId))
                        {
                            if (ProcessBus(messagingControllers[controllerId], MessagingBusId.SYSTEM, ref prevTimeBudget, out yieldReturn))
                            {
                                shouldRestart = true;
                                break;
                            }
                        }
                    }

                    if (shouldRestart || Time.realtimeSinceStartup - start >= GLOBAL_MAX_MSG_BUDGET)
                        break;
                }

                yield return null;
            }
        }

        bool ProcessBus(MessagingController controller, string busId, ref float prevTimeBudget, out IEnumerator yieldReturn)
        {
            float startTime = DCLTime.realtimeSinceStartup;
            MessagingBus bus = controller.messagingBuses[busId];

            if (bus.isRunning && bus.pendingMessagesCount > 0)
            {
                yieldReturn = null;

                float timeBudget = prevTimeBudget;

                if (RenderingController.i.renderingEnabled)
                    timeBudget = Mathf.Clamp(timeBudget, bus.budgetMin, bus.budgetMax);
                else
                    timeBudget = Mathf.Clamp(timeBudget, GLOBAL_MIN_MSG_BUDGET_WHEN_LOADING, GLOBAL_MAX_MSG_BUDGET_WHEN_LOADING);

                if (VERBOSE && timeBudget == 0)
                {
                    string finalTag = SceneController.i.TryToGetSceneCoordsID(bus.debugTag);
                    Debug.Log($"#{bus.processedMessagesCount} ... bus = {finalTag}, id = {bus.id}... timeBudget is zero!!!");
                }

                if (bus.ProcessQueue(timeBudget, out yieldReturn))
                {
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
                return true;
            }

            return false;
        }
    }
}
