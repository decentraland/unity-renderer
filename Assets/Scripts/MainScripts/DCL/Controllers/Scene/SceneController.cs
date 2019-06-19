using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace DCL
{
    public interface IMessageHandler
    {
        bool ProcessMessage(string sceneId, string method, string payload, out Coroutine routine);
        void LoadParcelScenesExecute(string decentralandSceneJSON);
        void UnloadAllScenes();
    }

    public class SceneController : MonoBehaviour, IMessageHandler
    {
        public static SceneController i { get; private set; }

        public bool startDecentralandAutomatically = true;
        public static bool VERBOSE = false;

        private const float CHAT_MSG_BUS_BUDGET_MAX = 0.1f;
        private const float UI_MSG_BUS_BUDGET_MAX = 0.1f;
        private const float INIT_MSG_BUS_BUDGET_MAX = 0.3f;
        private const float SYSTEM_MSG_BUS_BUDGET_MAX = 0.1f;

        private const float MSG_BUS_BUDGET_MIN = 0.01f;

        private const float GLTF_BUDGET_MAX = 2.5f;

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

            messagingSystems.Add(MessagingBusId.UI, new MessagingSystem(this, MSG_BUS_BUDGET_MIN, UI_MSG_BUS_BUDGET_MAX));
            messagingSystems.Add(MessagingBusId.INIT, new MessagingSystem(this, MSG_BUS_BUDGET_MIN, INIT_MSG_BUS_BUDGET_MAX));
            messagingSystems.Add(MessagingBusId.SYSTEM, new MessagingSystem(this, MSG_BUS_BUDGET_MIN, SYSTEM_MSG_BUS_BUDGET_MAX));

            priorities.Add(MessagingBusId.UI);
            priorities.Add(MessagingBusId.INIT);
            priorities.Add(MessagingBusId.SYSTEM);

            loadingScreenController = new LoadingScreenController(this, enableLoadingScreenInEditor);
            loadingScreenController.OnLoadingDone += LoadingDone;
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

            OnMessageDecodeStart?.Invoke("LoadScene");
            scene = SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(decentralandSceneJSON);
            OnMessageDecodeEnds?.Invoke("LoadScene");

            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;

#if UNITY_EDITOR
            if (debugScenes && sceneToLoad.id != debugSceneName)
                return;
#endif

            OnMessageProcessStart?.Invoke("LoadScene");
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

                if (!newScene.isPersistent && !loadingScreenController.started)
                {
                    Vector2 playerPos = ParcelScene.WorldToGridPosition(DCLCharacterController.i.transform.position);
                    if (Vector2Int.Distance(new Vector2Int((int)playerPos.x, (int)playerPos.y), sceneToLoad.basePosition) <= LoadingScreenController.MAX_DISTANCE_TO_PLAYER)
                    {
                        loadingScreenController.StartLoadingScreen();
                    }
                }
            }

            OnMessageProcessEnds?.Invoke("LoadScene");
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

            OnMessageWillQueue?.Invoke("LoadScene");

            messagingSystems[MessagingBusId.INIT].bus.pendingMessages.Enqueue(queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage() { type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_SCENES };

            OnMessageWillQueue?.Invoke("UnloadScene");

            messagingSystems[MessagingBusId.INIT].bus.pendingMessages.Enqueue(queuedMessage);
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
                    OnMessageDecodeStart?.Invoke("Misc");
                    var separatorPosition = chunks[i].IndexOf('\t');

                    if (separatorPosition == -1)
                    {
                        OnMessageDecodeEnds?.Invoke("Misc");
                        continue;
                    }

                    var sceneId = chunks[i].Substring(0, separatorPosition);
                    var message = chunks[i].Substring(separatorPosition + 1);
                    OnMessageDecodeEnds?.Invoke("Misc");

#if UNITY_EDITOR
                    if (debugScenes && sceneId != debugSceneName)
                    {
                        continue;
                    }
#endif

                    var queuedMessage = new MessagingBus.QueuedSceneMessage_Scene()
                    { type = MessagingBus.QueuedSceneMessage.Type.SCENE_MESSAGE, sceneId = sceneId, message = message };

                    OnMessageDecodeStart?.Invoke("Misc");
                    var queuedMessageSeparatorIndex = queuedMessage.message.IndexOf('\t');

                    queuedMessage.method = queuedMessage.message.Substring(0, queuedMessageSeparatorIndex);
                    queuedMessage.payload = queuedMessage.message.Substring(queuedMessageSeparatorIndex + 1);
                    OnMessageDecodeEnds?.Invoke("Misc");

                    OnMessageWillQueue?.Invoke(queuedMessage.method);

                    // Check if the message is for UI
                    if (sceneId == globalSceneId)
                    {
                        busId = MessagingBusId.UI;
                    }
                    else if (readyScenes.Contains(sceneId))
                    {
                        busId = MessagingBusId.SYSTEM;
                    }
                    else
                    {
                        busId = MessagingBusId.INIT;
                    }

                    messagingSystems[busId].bus.pendingMessages.Enqueue(queuedMessage);
                }
            }
            return busId;
        }

        public bool ProcessMessage(string sceneId, string method, string payload, out Coroutine routine)
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
                    case "CreateEntity":
                        scene.CreateEntity(payload);
                        break;
                    case "SetEntityParent":
                        scene.SetEntityParent(payload);
                        break;

                    //NOTE(Brian): EntityComponent messages
                    case "UpdateEntityComponent":
                        scene.EntityComponentCreate(payload);
                        break;
                    case "ComponentRemoved":
                        scene.EntityComponentRemove(payload);
                        break;

                    //NOTE(Brian): SharedComponent messages
                    case "AttachEntityComponent":
                        scene.SharedComponentAttach(payload);
                        break;
                    case "ComponentCreated":
                        scene.SharedComponentCreate(payload);
                        break;
                    case "ComponentDisposed":
                        scene.SharedComponentDispose(payload);
                        break;
                    case "ComponentUpdated":
                        scene.SharedComponentUpdate(payload, out routine);
                        break;
                    case "RemoveEntity":
                        scene.RemoveEntity(payload);
                        break;
                    case "SceneStarted":
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
                //Debug.LogError($"Scene not found {sceneId}");
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
                Debug.LogError($"Scene {data.id} is already loaded.");
            }

            return newScene;
        }

        private void LoadingDone()
        {
            loadingScreenController.OnLoadingDone -= LoadingDone;
            fpsPanel.GetComponent<DCL.FrameTimeCounter>().Reset();
        }
    }
}
