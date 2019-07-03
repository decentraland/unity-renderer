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
    public interface IMessageHandler
    {
        bool ProcessMessage(string sceneId, string id, string method, string payload, out Coroutine routine);
        void LoadParcelScenesExecute(string decentralandSceneJSON);
        void UnloadAllScenes();
    }


    public class SceneController : MonoBehaviour, IMessageHandler
    {
        public static SceneController i { get; private set; }

        public bool startDecentralandAutomatically = true;
        public static bool VERBOSE = false;

        private const float UI_MSG_BUS_BUDGET_MAX = 0.01f;
        private const float INIT_MSG_BUS_BUDGET_MAX = 0.3f;
        private const float SYSTEM_MSG_BUS_BUDGET_MAX = 0.01f;

        private const float MSG_BUS_BUDGET_MIN = 0.005f;


        [FormerlySerializedAs("factoryManifest")]
        public DCLComponentFactory componentFactory;

        public HashSet<string> readyScenes = new HashSet<string>();
        public Dictionary<string, ParcelScene> loadedScenes = new Dictionary<string, ParcelScene>();

        [Header("Debug Tools")]
        public GameObject fpsPanel;
        public bool enableLoadingScreenInEditor = false;

        [Header("Debug Panel")]
        public GameObject engineDebugPanel;
        public GameObject sceneDebugPanel;

        public bool debugScenes;

        public string debugSceneName;
        public bool ignoreGlobalScenes = false;
        public bool msgStepByStep = false;

        private LoadingScreenController loadingScreenController = null;

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

        public Dictionary<string, MessagingSystem> messagingSystems = new Dictionary<string, MessagingSystem>();
        private List<string> priorities = new List<string>();

        private Dictionary<string, PendingMessage> pendingLossyMessages = new Dictionary<string, PendingMessage>();
        private Dictionary<string, LinkedList<PendingMessage>> pendingReliableMessages = new Dictionary<string, LinkedList<PendingMessage>>();

        [System.NonSerialized]
        public bool isDebugMode;

        public bool hasPendingMessages => pendingMessagesCount > 0;
        public int pendingMessagesCount
        {
            get
            {
                int total = 0;

                using (var iterator = messagingSystems.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        //access to pair using iterator.Current
                        MessagingBus messageBus = iterator.Current.Value.bus;
                        total += messageBus != null ? messageBus.pendingMessagesCount : 0;
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
#endif

            // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
            if (startDecentralandAutomatically)
            {
                WebInterface.StartDecentraland();
            }

            messagingSystems.Add(MessagingBusId.UI, new MessagingSystem(this, MSG_BUS_BUDGET_MIN, UI_MSG_BUS_BUDGET_MAX, enableThrottler: false));
            messagingSystems.Add(MessagingBusId.INIT, new MessagingSystem(this, MSG_BUS_BUDGET_MIN, INIT_MSG_BUS_BUDGET_MAX, enableThrottler: true));
            messagingSystems.Add(MessagingBusId.SYSTEM, new MessagingSystem(this, MSG_BUS_BUDGET_MIN, SYSTEM_MSG_BUS_BUDGET_MAX, enableThrottler: false));

            priorities.Add(MessagingBusId.UI);
            priorities.Add(MessagingBusId.INIT);
            priorities.Add(MessagingBusId.SYSTEM);

            InitializeLoadingScreen();
        }

        private void OnEnable()
        {
            DCLCharacterController.OnPositionSet += OnCharacterPositionSet;
        }

        private void OnDisable()
        {
            DCLCharacterController.OnPositionSet -= OnCharacterPositionSet;
        }

        private void Update()
        {
            UpdateThrottling();
        }

        private void UpdateThrottling()
        {
            float prevTimeBudget = 0;

            for (int i = 0; i < priorities.Count; i++)
            {
                string id = priorities[i];

                MessagingSystem system = messagingSystems[id];

                prevTimeBudget += system.Update(prevTimeBudget);
            }
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

                if (VERBOSE)
                {
                    Debug.Log($"Creating UI scene {uiSceneId}");
                }
            }
        }

        public void SetDebug()
        {
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

#if UNITY_EDITOR
            if (debugScenes && sceneToLoad.id != debugSceneName)
                return;
#endif

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

                bool enableLoadingScreen = true;

#if UNITY_EDITOR
                enableLoadingScreen = enableLoadingScreenInEditor;
#endif

                if (!newScene.isPersistent && !loadingScreenController.started && enableLoadingScreen)
                {
                    Vector2 playerPos = ParcelScene.WorldToGridPosition(DCLCharacterController.i.transform.position);
                    if (Vector2Int.Distance(new Vector2Int((int)playerPos.x, (int)playerPos.y), sceneToLoad.basePosition) <= LoadingScreenController.MAX_DISTANCE_TO_PLAYER)
                    {
                        loadingScreenController.StartLoadingScreen();
                    }
                }
            }

            OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_LOAD);
        }

        public void UnloadScene(string sceneKey)
        {
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

            if (scene)
            {
                Utils.SafeDestroy(scene.gameObject);
            }
        }

        public void UnloadAllScenes()
        {
            var list = loadedScenes.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                UnloadScene(list[i].Key);
            }
        }

        public void LoadParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
            { type = MessagingBus.QueuedSceneMessage.Type.LOAD_PARCEL, message = decentralandSceneJSON };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_LOAD);

            messagingSystems[MessagingBusId.INIT].Enqueue(queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage() { type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_SCENES };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            messagingSystems[MessagingBusId.INIT].Enqueue(queuedMessage);
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

#if UNITY_EDITOR
                    if (debugScenes && sceneId != debugSceneName)
                    {
                        continue;
                    }
#endif
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

            QueueMode queueMode = QueueMode.Reliable;
            bool shouldEnqueue = true;

            if (queuedMessage.sceneId == globalSceneId)
            {
                busId = MessagingBusId.UI;
            }
            else if (readyScenes.Contains(queuedMessage.sceneId))
            {
                busId = MessagingBusId.SYSTEM;
            }
            else
            {
                busId = MessagingBusId.INIT;
            }

            ParcelScene scene = null;

            if (loadedScenes.ContainsKey(queuedMessage.sceneId))
                scene = loadedScenes[queuedMessage.sceneId];

            if (queuedMessage.method == MessagingTypes.ENTITY_COMPONENT_CREATE ||
                queuedMessage.method == MessagingTypes.ENTITY_DESTROY ||
                queuedMessage.method == MessagingTypes.SHARED_COMPONENT_ATTACH ||
                queuedMessage.method == MessagingTypes.ENTITY_REPARENT)
            {
                // By default, the tag is the id of the entity/component
                string entityId = queuedMessage.tag;

                // Check if the message type is an UpdateEntityComponent 
                if (queuedMessage.method == MessagingTypes.ENTITY_COMPONENT_CREATE)
                {
                    int classId = 0;

                    // If it is, we need to extract the entityId and the classId from the tag.
                    // The tag format is "entityId_classId", i.e: "E1_2". 
                    GetEntityIdAndClassIdFromTag(queuedMessage.tag, out entityId, out classId);

                    // If it is a transform update, the queue mode is Lossy
                    if (classId == (int)CLASS_ID_COMPONENT.TRANSFORM)
                    {
                        queueMode = QueueMode.Lossy;
                    }
                }

                // We need to check if the entity exists
                bool delayed = scene == null || !scene.entities.ContainsKey(entityId);

                string id = FormatQueueId(queuedMessage.sceneId, entityId);

                // If it doesn't exist, we need to save this message to be enqueued later
                if (delayed)
                {
                    shouldEnqueue = false;

                    if (queueMode == QueueMode.Lossy)
                        pendingLossyMessages[id] = new PendingMessage(busId, queuedMessage, queueMode);
                    else
                    {
                        if (!pendingReliableMessages.ContainsKey(id))
                            pendingReliableMessages[id] = new LinkedList<PendingMessage>();

                        pendingReliableMessages[id].AddLast(new PendingMessage(busId, queuedMessage, queueMode));
                    }
                }
            }
            else if (queuedMessage.method == MessagingTypes.ENTITY_COMPONENT_DESTROY ||
                queuedMessage.method == MessagingTypes.SHARED_COMPONENT_DISPOSE ||
                queuedMessage.method == MessagingTypes.SHARED_COMPONENT_UPDATE)
            {
                // We need to check if the component exists
                bool delayed = scene == null || !scene.disposableComponents.ContainsKey(queuedMessage.tag);

                string id = FormatQueueId(queuedMessage.sceneId, queuedMessage.tag);

                // If it doesn't exist, we need to save this message to be enqueued later
                if (delayed)
                {
                    shouldEnqueue = false;

                    if (!pendingReliableMessages.ContainsKey(id))
                        pendingReliableMessages[id] = new LinkedList<PendingMessage>();

                    pendingReliableMessages[id].AddLast(new PendingMessage(busId, queuedMessage, queueMode));
                }
            }

            // If the message wasn't delayed, we add it to the queue
            if (shouldEnqueue)
                messagingSystems[busId].Enqueue(queuedMessage, queueMode);

            return busId;
        }

        private void GetEntityIdAndClassIdFromTag(string tag, out string entityId, out int classId)
        {
            int separator = tag.IndexOf('_');
            entityId = tag.Substring(0, separator);
            classId = System.Convert.ToInt32(tag.Substring(separator + 1));
        }

        private string GetEntityIdFromTag(string tag)
        {
            int separator = tag.IndexOf('_');
            return tag.Substring(0, separator);
        }

        private string FormatQueueId(string sceneId, string tag)
        {
            return sceneId + tag;
        }

        private void EnqueueReliableMessages(string sceneId, string tag)
        {
            string id = FormatQueueId(sceneId, tag);

            if (pendingReliableMessages.ContainsKey(id))
            {
                PendingMessage message;

                while (pendingReliableMessages[id].First != null)
                {
                    message = pendingReliableMessages[id].First();

                    messagingSystems[message.busId].Enqueue(message.message, message.queueMode);

                    pendingReliableMessages[id].RemoveFirst();
                }

                pendingReliableMessages.Remove(id);
            }
        }

        private void EnqueueLossyMessages(string sceneId, string tag)
        {
            string id = FormatQueueId(sceneId, tag);

            if (pendingLossyMessages.ContainsKey(id))
            {
                PendingMessage pm = pendingLossyMessages[id];

                messagingSystems[pm.busId].Enqueue(pm.message, pm.queueMode);
                pendingLossyMessages.Remove(id);
            }
        }

        private void PurgeReliableMessages(string sceneId, string tag)
        {
            string id = FormatQueueId(sceneId, tag);

            if (pendingReliableMessages.ContainsKey(id))
                pendingReliableMessages.Remove(id);

            for (int i = 0; i < priorities.Count; i++)
            {
                messagingSystems[priorities[i]].Purge(sceneId, tag);
            }
        }


        private void PurgeLossyMessages(string sceneId, string tag)
        {
            string id = FormatQueueId(sceneId, tag);

            if (pendingLossyMessages.ContainsKey(id))
                pendingLossyMessages.Remove(id);

            for (int i = 0; i < priorities.Count; i++)
            {
                messagingSystems[priorities[i]].Purge(sceneId, tag);
            }
        }

        public bool ProcessMessage(string sceneId, string tag, string method, string payload, out Coroutine routine)
        {
            routine = null;
#if UNITY_EDITOR
            if (debugScenes && sceneId != debugSceneName)
            {
                return false;
            }
#endif

            ParcelScene scene;

            if (loadedScenes.TryGetValue(sceneId, out scene))
            {
#if UNITY_EDITOR
                if (scene is GlobalScene && ignoreGlobalScenes && debugScenes)
                {
                    return false;
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

                        EnqueueLossyMessages(sceneId, tag);
                        EnqueueReliableMessages(sceneId, tag);
                        break;
                    case MessagingTypes.ENTITY_REPARENT:
                        scene.SetEntityParent(payload);
                        break;

                    //NOTE(Brian): EntityComponent messages
                    case MessagingTypes.ENTITY_COMPONENT_CREATE:
                        scene.EntityComponentCreate(payload);

                        EnqueueLossyMessages(sceneId, tag);
                        EnqueueReliableMessages(sceneId, tag);
                        break;
                    case MessagingTypes.ENTITY_COMPONENT_DESTROY:
                        PurgeReliableMessages(sceneId, tag);
                        scene.EntityComponentRemove(payload);
                        break;

                    //NOTE(Brian): SharedComponent messages
                    case MessagingTypes.SHARED_COMPONENT_ATTACH:
                        scene.SharedComponentAttach(payload);
                        break;
                    case MessagingTypes.SHARED_COMPONENT_CREATE:
                        scene.SharedComponentCreate(payload);
                        EnqueueReliableMessages(sceneId, tag);
                        break;
                    case MessagingTypes.SHARED_COMPONENT_DISPOSE:
                        PurgeReliableMessages(sceneId, tag);
                        scene.SharedComponentDispose(payload);
                        break;
                    case MessagingTypes.SHARED_COMPONENT_UPDATE:
                        scene.SharedComponentUpdate(payload, out routine);
                        break;
                    case MessagingTypes.ENTITY_DESTROY:
                        PurgeLossyMessages(sceneId, tag);
                        PurgeReliableMessages(sceneId, tag);
                        scene.RemoveEntity(tag);
                        break;
                    case MessagingTypes.SCENE_STARTED:
                        readyScenes.Add(scene.sceneData.id);
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

        private void InitializeLoadingScreen()
        {
            loadingScreenController = new LoadingScreenController(this, enableLoadingScreenInEditor);
            loadingScreenController.OnLoadingDone += LoadingDone;
        }

        private void OnCharacterPositionSet(Vector3 newPosition)
        {
            if (!DCLCharacterController.i.initialPositionAlreadySet) return;

            InitializeLoadingScreen();

            // Flush pending messages
            using (var iterator = SceneController.i.messagingSystems.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.bus.pendingMessages.Clear();
                }
            }
        }

        private void LoadingDone()
        {
            loadingScreenController.OnLoadingDone -= LoadingDone;

            if (isDebugMode)
            {
                fpsPanel.GetComponent<DCL.FrameTimeCounter>().Reset();
            }
        }
    }
}
