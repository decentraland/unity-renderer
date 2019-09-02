using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    public class SceneController : MonoBehaviour, IMessageHandler
    {
        public static SceneController i { get; private set; }

        public bool startDecentralandAutomatically = true;
        public static bool VERBOSE = false;

        private const float GLOBAL_MAX_MSG_BUDGET = 0.012f;
        private const float GLTF_BUDGET_MAX = 0.012f;
        private const float GLTF_BUDGET_MIN = 0.001f;

        public const string GLOBAL_MESSAGING_CONTROLLER = "global_messaging_controller";

        [FormerlySerializedAs("factoryManifest")]
        public DCLComponentFactory componentFactory;

        public HashSet<string> readyScenes = new HashSet<string>();
        public Dictionary<string, ParcelScene> loadedScenes = new Dictionary<string, ParcelScene>();

        [Header("Debug Tools")]
        public GameObject fpsPanel;

        [Header("Debug Panel")]
        public GameObject engineDebugPanel;
        public GameObject sceneDebugPanel;

        public bool debugScenes;

        public Vector2Int debugSceneCoords;
        public bool ignoreGlobalScenes = false;
        public bool msgStepByStep = false;

        #region BENCHMARK_EVENTS

        //NOTE(Brian): For performance reasons, these events may need to be removed for production.
        public Action<string> OnMessageWillQueue;
        public Action<string> OnMessageWillDequeue;

        public Action<string> OnMessageProcessStart;
        public Action<string> OnMessageProcessEnds;

        public Action<string> OnMessageDecodeStart;
        public Action<string> OnMessageDecodeEnds;

        #endregion

#if UNITY_EDITOR
        public delegate void ProcessDelegate(string sceneId, string method, string payload);
        public event ProcessDelegate OnMessageProcessInfoStart;
        public event ProcessDelegate OnMessageProcessInfoEnds;
#endif
        public Dictionary<string, MessagingController> messagingControllers = new Dictionary<string, MessagingController>();

        private List<ParcelScene> scenesSortedByDistance = new List<ParcelScene>();

        [System.NonSerialized]
        public bool isDebugMode;

        [System.NonSerialized]
        public bool isWssDebugMode;

        public bool hasPendingMessages => pendingMessagesCount > 0;

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

        public string GlobalSceneId
        {
            get { return globalSceneId; }
        }

        LoadParcelScenesMessage loadParcelScenesMessage = new LoadParcelScenesMessage();
        string globalSceneId = "";

        const float SORT_MESSAGE_CONTROLLER_TIME = 0.25f;
        float lastTimeMessageControllerSorted = 0;

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);

                return;
            }

            i = this;

#if !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);

            Debug.unityLogger.logEnabled = false;
#endif

            messagingControllers[GLOBAL_MESSAGING_CONTROLLER] = new MessagingController(this, GLOBAL_MESSAGING_CONTROLLER);

            // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
            if (startDecentralandAutomatically)
            {
                WebInterface.StartDecentraland();
            }
        }

        private void Update()
        {
            float prevTimeBudget = GLOBAL_MAX_MSG_BUDGET;

            PrioritizeMessageControllerList();

            // First we process Load/Unload scene messages
            prevTimeBudget -= messagingControllers[GLOBAL_MESSAGING_CONTROLLER].UpdateThrottling(prevTimeBudget);

            // If we already have a messaging controller for global scene,
            // we update throttling
            if (!string.IsNullOrEmpty(globalSceneId) && messagingControllers.ContainsKey(globalSceneId))
                prevTimeBudget -= messagingControllers[globalSceneId].UpdateThrottling(prevTimeBudget);

            if (VERBOSE)
                Debug.Log("Priority controllers count = " + scenesSortedByDistance.Count);
            // Update throttling to the rest of the messaging controllers
            for (int i = 0; i < scenesSortedByDistance.Count; i++)
            {
                ParcelScene scene = scenesSortedByDistance[i];
                prevTimeBudget -= messagingControllers[scene.sceneData.id].UpdateThrottling(prevTimeBudget);
            }

            if (pendingInitMessagesCount == 0)
            {
                UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = Mathf.Clamp(GLTF_BUDGET_MAX - prevTimeBudget, GLTF_BUDGET_MIN, GLTF_BUDGET_MAX) * 1000f;
            }
            else
            {
                UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = 0;
            }
        }

        private void PrioritizeMessageControllerList(bool force = false)
        {
            if (force || Time.unscaledTime - lastTimeMessageControllerSorted >= SORT_MESSAGE_CONTROLLER_TIME)
            {
                lastTimeMessageControllerSorted = Time.unscaledDeltaTime;
                scenesSortedByDistance.Sort(SceneMessagingSortByDistance);
            }
        }

        private int SceneMessagingSortByDistance(ParcelScene sceneA, ParcelScene sceneB)
        {
            if (sceneA.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
                return -1;

            if (sceneB.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
                return 1;

            int dist1 = (int)(sceneA.transform.position - DCLCharacterController.i.transform.position).sqrMagnitude;
            int dist2 = (int)(sceneB.transform.position - DCLCharacterController.i.transform.position).sqrMagnitude;

            return dist1 - dist2;
        }

        public void CreateUIScene(string json)
        {
#if UNITY_EDITOR
            if (debugScenes && ignoreGlobalScenes)
            {
                return;
            }
#endif
            CreateUISceneMessage uiScene = SafeFromJson<CreateUISceneMessage>(json);

            string uiSceneId = uiScene.id;

            if (!loadedScenes.ContainsKey(uiSceneId))
            {
                var newGameObject = new GameObject("UI Scene - " + uiSceneId);

                var newScene = newGameObject.AddComponent<GlobalScene>();
                newScene.ownerController = this;
                newScene.unloadWithDistance = false;
                newScene.isPersistent = true;

                LoadParcelScenesMessage.UnityParcelScene data = new LoadParcelScenesMessage.UnityParcelScene();
                data.id = uiSceneId;
                data.basePosition = new Vector2Int(0, 0);
                data.baseUrl = uiScene.baseUrl;
                newScene.SetData(data);

                loadedScenes.Add(uiSceneId, newScene);

                globalSceneId = uiSceneId;

                if (!messagingControllers.ContainsKey(globalSceneId))
                    messagingControllers[globalSceneId] = new MessagingController(this, globalSceneId);

                if (VERBOSE)
                {
                    Debug.Log($"Creating UI scene {uiSceneId}");
                }
            }
        }

        public void SetDebug()
        {
            Debug.unityLogger.logEnabled = true;

            isDebugMode = true;
            fpsPanel.SetActive(true);
        }

        public void SetSceneDebugPanel()
        {
            engineDebugPanel.SetActive(false);
            sceneDebugPanel.SetActive(true);
        }

        public void SetEngineDebugPanel()
        {
            sceneDebugPanel.SetActive(false);
            engineDebugPanel.SetActive(true);
        }

        public bool IsCharacterInsideScene(string sceneId)
        {
            bool res = false;

            if (loadedScenes.ContainsKey(sceneId))
            {
                ParcelScene scene = loadedScenes[sceneId];

                if (scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
                    res = true;
            }

            return res;
        }

        public string GetCurrentScene(DCLCharacterPosition position)
        {
            using (var iterator = loadedScenes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    ParcelScene scene = iterator.Current.Value;
                    if (scene.sceneData.id != globalSceneId)
                    {
                        if (scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
                            return scene.sceneData.id;
                    }
                }
            }

            return null;
        }

        ParcelScene GetDecentralandSceneOfGridPosition(Vector2Int gridPosition)
        {
            foreach (var estate in loadedScenes)
            {
                if (estate.Value.sceneData.basePosition.Equals(gridPosition))
                {
                    return estate.Value;
                }

                foreach (var iteratedParcel in estate.Value.sceneData.parcels)
                {
                    if (iteratedParcel == gridPosition)
                    {
                        return estate.Value;
                    }
                }
            }

            return null;
        }

        public void LoadParcelScenesExecute(string decentralandSceneJSON)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_LOAD);
            scene = SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(decentralandSceneJSON);
            OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_LOAD);

            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;

            OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_LOAD);
            if (!loadedScenes.ContainsKey(sceneToLoad.id))
            {
                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                newScene.SetData(sceneToLoad);

                if (isDebugMode)
                {
                    newScene.InitializeDebugPlane();
                }

                newScene.ownerController = this;
                loadedScenes.Add(sceneToLoad.id, newScene);

                scenesSortedByDistance.Add(newScene);

                if (!messagingControllers.ContainsKey(newScene.sceneData.id))
                    messagingControllers[newScene.sceneData.id] = new MessagingController(this, newScene.sceneData.id);

                PrioritizeMessageControllerList(force: true);
            }

            OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_LOAD);
        }


        public void UpdateParcelScenesExecute(string decentralandSceneJSON)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            scene = SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(decentralandSceneJSON);
            OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_UPDATE);

            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;

            OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            if (loadedScenes.ContainsKey(sceneToLoad.id))
            {
                loadedScenes[sceneToLoad.id].SetUpdateData(sceneToLoad);
            }
            else
            {
                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                newScene.SetData(sceneToLoad);

                if (isDebugMode)
                {
                    newScene.InitializeDebugPlane();
                }

                newScene.ownerController = this;
                loadedScenes.Add(sceneToLoad.id, newScene);

            }

            OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_UPDATE);
        }

        public void UnloadScene(string sceneKey)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
            { type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_PARCEL, message = sceneKey };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            messagingControllers[GLOBAL_MESSAGING_CONTROLLER].ForceEnqueue(MessagingBusId.INIT, queuedMessage);

            if (messagingControllers.ContainsKey(sceneKey))
                messagingControllers[sceneKey].Stop();
        }

        public void UnloadParcelSceneExecute(string sceneKey)
        {
            OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_DESTROY);

            if (!loadedScenes.ContainsKey(sceneKey) || loadedScenes[sceneKey].isPersistent)
            {
                return;
            }

            if (VERBOSE)
            {
                Debug.Log($"Destroying scene {sceneKey}");
            }

            var scene = loadedScenes[sceneKey];

            loadedScenes.Remove(sceneKey);

            // Remove the scene id from the msg. priorities list
            scenesSortedByDistance.Remove(scene);

            // Remove messaging controller for unloaded scene
            if (messagingControllers.ContainsKey(scene.sceneData.id))
            {
                // We need to dispose the messaging controller to stop bus coroutines
                messagingControllers[scene.sceneData.id].Dispose();
                messagingControllers.Remove(scene.sceneData.id);
            }

            if (scene)
            {
                scene.Cleanup();
            }

            OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_DESTROY);
        }

        public void UnloadAllScenes()
        {
            var list = loadedScenes.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                UnloadParcelSceneExecute(list[i].Key);
            }
        }

        public void LoadParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
            { type = MessagingBus.QueuedSceneMessage.Type.LOAD_PARCEL, message = decentralandSceneJSON };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_LOAD);

            messagingControllers[GLOBAL_MESSAGING_CONTROLLER].ForceEnqueue(MessagingBusId.INIT, queuedMessage);
        }

        public void UpdateParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
            { type = MessagingBus.QueuedSceneMessage.Type.UPDATE_PARCEL, message = decentralandSceneJSON };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_UPDATE);

            messagingControllers[GLOBAL_MESSAGING_CONTROLLER].ForceEnqueue(MessagingBusId.INIT, queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage() { type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_SCENES };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            messagingControllers[GLOBAL_MESSAGING_CONTROLLER].ForceEnqueue(MessagingBusId.INIT, queuedMessage);
        }

        public string SendSceneMessage(string payload)
        {
            string busId = "none";

            OnMessageDecodeStart?.Invoke("Misc");
            var chunks = payload.Split('\n');
            OnMessageDecodeEnds?.Invoke("Misc");

            for (int i = 0; i < chunks.Length; i++)
            {
                if (chunks[i].Length > 0)
                {
                    string sceneId;
                    string message;
                    string tag;

                    if (!DecodePayloadChunk(chunks[i], out sceneId, out message, out tag))
                    {
                        continue;
                    }

                    var queuedMessage = DecodeSceneMessage(sceneId, message, tag);

                    busId = EnqueueMessage(queuedMessage);
                }
            }
            return busId;
        }

        private bool DecodePayloadChunk(string chunk, out string sceneId, out string message, out string tag)
        {
            OnMessageDecodeStart?.Invoke("Misc");

            sceneId = message = tag = null;

            var separatorPosition = chunk.IndexOf('\t');

            if (separatorPosition == -1)
            {
                OnMessageDecodeEnds?.Invoke("Misc");

                return false;
            }

            sceneId = chunk.Substring(0, separatorPosition);

            var lastPosition = separatorPosition + 1;
            separatorPosition = chunk.IndexOf('\t', lastPosition);

            message = chunk.Substring(lastPosition, separatorPosition - lastPosition);
            lastPosition = separatorPosition + 1;

            separatorPosition = chunk.IndexOf('\t', lastPosition);

            message += '\t' + chunk.Substring(lastPosition, separatorPosition - lastPosition);
            lastPosition = separatorPosition + 1;

            tag = chunk.Substring(lastPosition);

            OnMessageDecodeEnds?.Invoke("Misc");

            return true;
        }

        private MessagingBus.QueuedSceneMessage_Scene DecodeSceneMessage(string sceneId, string message, string tag)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage_Scene()
            { type = MessagingBus.QueuedSceneMessage.Type.SCENE_MESSAGE, sceneId = sceneId, message = message, tag = tag };

            OnMessageDecodeStart?.Invoke("Misc");
            var queuedMessageSeparatorIndex = queuedMessage.message.IndexOf('\t');

            queuedMessage.method = queuedMessage.message.Substring(0, queuedMessageSeparatorIndex);
            queuedMessage.payload = queuedMessage.message.Substring(queuedMessageSeparatorIndex + 1);
            OnMessageDecodeEnds?.Invoke("Misc");

            return queuedMessage;
        }

        private string EnqueueMessage(MessagingBus.QueuedSceneMessage_Scene queuedMessage)
        {
            string busId = "";

            ParcelScene scene = null;

            if (loadedScenes.ContainsKey(queuedMessage.sceneId))
                scene = loadedScenes[queuedMessage.sceneId];

            // If it doesn't exist, create messaging controller for this scene id
            if (!messagingControllers.ContainsKey(queuedMessage.sceneId))
                messagingControllers[queuedMessage.sceneId] = new MessagingController(this, queuedMessage.sceneId);

            busId = messagingControllers[queuedMessage.sceneId].Enqueue(scene, queuedMessage);

            return busId;
        }

        public bool ProcessMessage(string sceneId, string tag, string method, string payload, out CleanableYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;

            ParcelScene scene;

            if (loadedScenes.TryGetValue(sceneId, out scene))
            {
#if UNITY_EDITOR
                if (debugScenes)
                {
                    if (scene is GlobalScene && ignoreGlobalScenes)
                    {
                        return false;
                    }

                    if (scene.sceneData.basePosition.ToString() != debugSceneCoords.ToString())
                    {
                        return false;
                    }
                }
#endif
                if (!scene.gameObject.activeInHierarchy)
                {
                    return true;
                }

                if (VERBOSE)
                {
                    Debug.Log("SceneController ProcessMessage: \nMethod: " + method + "\nPayload: " + payload);
                }

#if UNITY_EDITOR
                OnMessageProcessInfoStart?.Invoke(sceneId, method, payload);
#endif
                OnMessageProcessStart?.Invoke(method);
                switch (method)
                {
                    case MessagingTypes.ENTITY_CREATE:
                        scene.CreateEntity(tag, payload);
                        break;
                    case MessagingTypes.ENTITY_REPARENT:
                        scene.SetEntityParent(payload);
                        break;

                    //NOTE(Brian): EntityComponent messages
                    case MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE:
                        scene.EntityComponentCreateOrUpdate(payload, out yieldInstruction);
                        break;
                    case MessagingTypes.ENTITY_COMPONENT_DESTROY:
                        scene.EntityComponentRemove(payload);
                        break;

                    //NOTE(Brian): SharedComponent messages
                    case MessagingTypes.SHARED_COMPONENT_ATTACH:
                        scene.SharedComponentAttach(payload);
                        break;
                    case MessagingTypes.SHARED_COMPONENT_CREATE:
                        scene.SharedComponentCreate(payload);
                        break;
                    case MessagingTypes.SHARED_COMPONENT_DISPOSE:
                        scene.SharedComponentDispose(payload);
                        break;
                    case MessagingTypes.SHARED_COMPONENT_UPDATE:
                        scene.SharedComponentUpdate(payload, out yieldInstruction);
                        break;
                    case MessagingTypes.ENTITY_DESTROY:
                        scene.RemoveEntity(tag);
                        break;
                    case MessagingTypes.INIT_DONE:
                        scene.SetInitMessagesDone();
                        break;
                    default:
                        Debug.LogError($"Unknown method {method}");
                        return true;
                }

                OnMessageProcessEnds?.Invoke(method);

#if UNITY_EDITOR
                OnMessageProcessInfoEnds?.Invoke(sceneId, method, payload);
#endif

                return true;
            }
            else
            {
                return false;
            }
        }

        public T SafeFromJson<T>(string data)
        {
            OnMessageDecodeStart?.Invoke("Misc");
            T result = Utils.SafeFromJson<T>(data);
            OnMessageDecodeEnds?.Invoke("Misc");

            return result;
        }

        public ParcelScene CreateTestScene(LoadParcelScenesMessage.UnityParcelScene data = null)
        {
            if (data == null)
            {
                data = new LoadParcelScenesMessage.UnityParcelScene();
            }

            if (data.basePosition == null)
            {
                data.basePosition = new Vector2Int(0, 0);
            }

            if (data.parcels == null)
            {
                data.parcels = new Vector2Int[] { data.basePosition };
            }

            if (string.IsNullOrEmpty(data.id))
            {
                data.id = $"(test):{data.basePosition.x},{data.basePosition.y}";
            }

            var go = new GameObject();
            var newScene = go.AddComponent<ParcelScene>();
            newScene.SetData(data);
            newScene.InitializeDebugPlane();
            newScene.ownerController = this;
            newScene.isTestScene = true;

            scenesSortedByDistance.Add(newScene);

            if (!messagingControllers.ContainsKey(data.id))
            {
                messagingControllers[data.id] = new MessagingController(this, data.id);
            }

            if (!loadedScenes.ContainsKey(data.id))
            {
                loadedScenes.Add(data.id, newScene);
            }
            else
            {
                Debug.LogWarning($"Scene {data.id} is already loaded.");
            }

            return newScene;
        }

        public void SendSceneReady(string sceneId)
        {
            readyScenes.Add(sceneId);

            // Start processing SYSTEM queue
            if (messagingControllers.ContainsKey(sceneId))
            {
                // Start processing SYSTEM queue 
                MessagingController sceneMessagingController = messagingControllers[sceneId];
                sceneMessagingController.StartBus(MessagingBusId.SYSTEM);
                sceneMessagingController.StartBus(MessagingBusId.UI);
                sceneMessagingController.StopBus(MessagingBusId.INIT);
            }

            //TODO LifeCycle Iteration 2:
            //Temporary location for this code, must be called when browser's LoadingScreen fades out
            if (isDebugMode)
            {
                fpsPanel.GetComponent<DCL.FrameTimeCounter>().Reset();
            }
            WebInterface.ReportControlEvent(new WebInterface.SceneReady(sceneId));
        }

        public string TryToGetSceneCoordsID(string id)
        {
            if (loadedScenes.ContainsKey(id))
                return loadedScenes[id].sceneData.basePosition.ToString();

            return id;
        }

        public void BuilderReady()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}
