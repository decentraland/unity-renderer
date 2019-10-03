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

        public static Action OnDebugModeSet;

#if UNITY_EDITOR
        public delegate void ProcessDelegate(string sceneId, string method, string payload);
        public event ProcessDelegate OnMessageProcessInfoStart;
        public event ProcessDelegate OnMessageProcessInfoEnds;
#endif
        public List<ParcelScene> scenesSortedByDistance = new List<ParcelScene>();

        [System.NonSerialized]
        public bool isDebugMode;

        [System.NonSerialized]
        public bool isWssDebugMode;

        public bool hasPendingMessages => pendingMessagesCount > 0;

        public int pendingMessagesCount
        {
            get
            {
                return MessagingControllersManager.i.pendingMessagesCount;
            }
        }

        public int pendingInitMessagesCount
        {
            get
            {
                return MessagingControllersManager.i.pendingInitMessagesCount;
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

            PointerEventsController.i.Initialize();

#if !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);

            Debug.unityLogger.logEnabled = false;
#endif

            MessagingControllersManager.i.Initialize(this);

            // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
            if (startDecentralandAutomatically)
            {
                WebInterface.StartDecentraland();
            }
        }

        private void Update()
        {
            InputController.i.Update();

            PrioritizeMessageControllerList();

            MessagingControllersManager.i.UpdateThrottling();
        }

        private void PrioritizeMessageControllerList(bool force = false)
        {
            if (force || DCLTime.realtimeSinceStartup - lastTimeMessageControllerSorted >= SORT_MESSAGE_CONTROLLER_TIME)
            {
                lastTimeMessageControllerSorted = DCLTime.realtimeSinceStartup;
                scenesSortedByDistance.Sort(SceneMessagingSortByDistance);
            }
        }

        private int SceneMessagingSortByDistance(ParcelScene sceneA, ParcelScene sceneB)
        {
            if (sceneA.IsInsideSceneBoundaries(Utils.WorldToGridPosition(DCLCharacterController.i.characterPosition.worldPosition)))
                return -1;

            if (sceneB.IsInsideSceneBoundaries(Utils.WorldToGridPosition(DCLCharacterController.i.characterPosition.worldPosition)))
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

                if (!MessagingControllersManager.i.ContainsController(globalSceneId))
                    MessagingControllersManager.i.AddController(this, globalSceneId, isGlobal: true);

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

            OnDebugModeSet?.Invoke();
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

                if (!MessagingControllersManager.i.ContainsController(newScene.sceneData.id))
                    MessagingControllersManager.i.AddController(this, newScene.sceneData.id);

                PrioritizeMessageControllerList(force: true);

                if (VERBOSE)
                    Debug.Log($"{Time.frameCount} : Load parcel scene {newScene.sceneData.basePosition}");
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

            MessagingControllersManager.i.ForceEnqueueToGlobal(MessagingBusId.INIT, queuedMessage);

            if (MessagingControllersManager.i.ContainsController(sceneKey))
                MessagingControllersManager.i.RemoveController(sceneKey);
        }

        public void UnloadParcelSceneExecute(string sceneKey)
        {
            OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_DESTROY);

            if (!loadedScenes.ContainsKey(sceneKey) || loadedScenes[sceneKey].isPersistent)
            {
                return;
            }


            var scene = loadedScenes[sceneKey];

            loadedScenes.Remove(sceneKey);

            // Remove the scene id from the msg. priorities list
            scenesSortedByDistance.Remove(scene);

            // Remove messaging controller for unloaded scene
            if (MessagingControllersManager.i.ContainsController(scene.sceneData.id))
                MessagingControllersManager.i.RemoveController(scene.sceneData.id);

            if (scene)
            {
                scene.Cleanup();

                if (VERBOSE)
                {
                    Debug.Log($"{Time.frameCount} : Destroying scene {scene.sceneData.basePosition}");
                }
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

            MessagingControllersManager.i.ForceEnqueueToGlobal(MessagingBusId.INIT, queuedMessage);

            if (VERBOSE)
                Debug.Log($"{Time.frameCount} : Load parcel scene queue {decentralandSceneJSON}");

        }

        public void UpdateParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
            { type = MessagingBus.QueuedSceneMessage.Type.UPDATE_PARCEL, message = decentralandSceneJSON };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_UPDATE);

            MessagingControllersManager.i.ForceEnqueueToGlobal(MessagingBusId.INIT, queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage() { type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_SCENES };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            MessagingControllersManager.i.ForceEnqueueToGlobal(MessagingBusId.INIT, queuedMessage);
        }

        public string SendSceneMessage(string payload)
        {
            if (payload.IndexOf('\n') != -1)
            {
                throw new InvalidProgramException("The kernel sent 'SendSceneMessages' with an end of line");
            }
            string sceneId;
            string message;
            string messageTag;

            if (!DecodePayloadChunk(payload, out sceneId, out message, out messageTag))
            {
                return null;
            }

            var queuedMessage = DecodeSceneMessage(sceneId, message, messageTag);
            return EnqueueMessage(queuedMessage);
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
            if (!MessagingControllersManager.i.ContainsController(queuedMessage.sceneId))
                MessagingControllersManager.i.AddController(this, queuedMessage.sceneId);

            busId = MessagingControllersManager.i.Enqueue(scene, queuedMessage);

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
                    case MessagingTypes.QUERY:
                        ParseQuery(payload, scene.sceneData.id);
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

        public Vector3 ConvertUnityToScenePosition(Vector3 pos, ParcelScene scene = null)
        {
            if (scene == null)
            {
                string sceneId = GetCurrentScene(DCLCharacterController.i.characterPosition);

                if (!string.IsNullOrEmpty(sceneId) && loadedScenes.ContainsKey(sceneId))
                    scene = loadedScenes[GetCurrentScene(DCLCharacterController.i.characterPosition)];
                else
                    return pos;
            }

            Vector3 worldPosition = DCLCharacterController.i.characterPosition.UnityToWorldPosition(pos);
            return worldPosition - Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
        }

        public void ParseQuery(string payload, string sceneId)
        {
            QueryMessage query = new QueryMessage();
            query.FromJSON(payload);

            ParcelScene scene = loadedScenes[sceneId];

            Vector3 worldOrigin = query.payload.ray.origin + Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
            query.payload.ray.unityOrigin = DCLCharacterController.i.characterPosition.WorldToUnityPosition(worldOrigin);

            switch (query.queryId)
            {
                case "raycast":
                    query.payload.sceneId = sceneId;
                    PhysicsCast.i.Query(query.payload);
                    break;
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

            if (!MessagingControllersManager.i.ContainsController(data.id))
                MessagingControllersManager.i.AddController(this, data.id);

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

            MessagingControllersManager.i.SetSceneReady(sceneId);

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
