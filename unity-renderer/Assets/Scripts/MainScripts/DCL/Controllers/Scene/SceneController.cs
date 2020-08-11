using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCL.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Components;
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

        [Header("Debug Tools")] public GameObject fpsPanel;

        [Header("Debug Panel")] public GameObject engineDebugPanel;
        public GameObject sceneDebugPanel;

        public bool debugScenes;

        public Vector2Int debugSceneCoords;
        public bool ignoreGlobalScenes = false;
        public bool msgStepByStep = false;

        [NonSerialized] public bool deferredMessagesDecoding = false;
        Queue<string> payloadsToDecode = new Queue<string>();
        const float MAX_TIME_FOR_DECODE = 0.005f;


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
        public delegate void ProcessDelegate(string sceneId, string method);

        public event ProcessDelegate OnMessageProcessInfoStart;
        public event ProcessDelegate OnMessageProcessInfoEnds;
#endif
        [System.NonSerialized] public List<ParcelScene> scenesSortedByDistance = new List<ParcelScene>();
        public Queue<MessagingBus.QueuedSceneMessage_Scene> sceneMessagesPool = new Queue<MessagingBus.QueuedSceneMessage_Scene>();

        [System.NonSerialized] public bool isDebugMode;
        [System.NonSerialized] public bool isWssDebugMode;
        [System.NonSerialized] public bool prewarmSceneMessagesPool = true;
        [System.NonSerialized] public bool useBoundariesChecker = true;

        [System.NonSerialized] public bool prewarmEntitiesPool = true;

        public bool hasPendingMessages => MessagingControllersManager.i.pendingMessagesCount > 0;

        public string globalSceneId { get; private set; }
        public string currentSceneId { get; private set; }

        public SceneBoundariesChecker boundariesChecker { get; private set; }

        private bool sceneSortDirty = false;
        private bool positionDirty = true;
        private int lastSortFrame = 0;

        public event Action OnSortScenes;
        public event Action<ParcelScene, string> OnOpenExternalUrlRequest;
        public event Action<ParcelScene> OnNewSceneAdded;

        public delegate void OnOpenNFTDialogDelegate(string assetContractAddress, string tokenId, string comment);

        public event OnOpenNFTDialogDelegate OnOpenNFTDialogRequest;

        private Vector2Int currentGridSceneCoordinate = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
        private Vector2Int sortAuxiliaryVector = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);

        private EntryPoint_World worldEntryPoint;

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

            InitializeSceneBoundariesChecker();

            MessagingControllersManager.i.Initialize(this);
            MemoryManager.i.Initialize();

            // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
            if (startDecentralandAutomatically)
            {
                WebInterface.StartDecentraland();
            }

            ParcelScene.parcelScenesCleaner.Start();

            if (deferredMessagesDecoding)
                StartCoroutine(DeferredDecoding());

            DCLCharacterController.OnCharacterMoved += SetPositionDirty;

#if !UNITY_EDITOR
            worldEntryPoint = new EntryPoint_World(this);
#endif
        }


        void InitializeSceneBoundariesChecker()
        {
            if (!useBoundariesChecker) return;

            if (boundariesChecker != null)
                boundariesChecker.Stop();

            if (isDebugMode)
            {
                boundariesChecker = new SceneBoundariesDebugModeChecker();
                boundariesChecker.timeBetweenChecks = 0f;
            }
            else
            {
                boundariesChecker = new SceneBoundariesChecker();
            }
        }

        private void SetPositionDirty(DCLCharacterPosition character)
        {
            var currentX = (int) Math.Floor(character.worldPosition.x / ParcelSettings.PARCEL_SIZE);
            var currentY = (int) Math.Floor(character.worldPosition.z / ParcelSettings.PARCEL_SIZE);

            positionDirty = currentX != currentGridSceneCoordinate.x || currentY != currentGridSceneCoordinate.y;

            if (positionDirty)
            {
                sceneSortDirty = true;
                currentGridSceneCoordinate.x = currentX;
                currentGridSceneCoordinate.y = currentY;
            }
        }

        private void SortScenesByDistance()
        {
            if (DCLCharacterController.i == null)
                return;

            currentSceneId = null;
            scenesSortedByDistance.Sort(SortScenesByDistanceMethod);

            using (var iterator = scenesSortedByDistance.GetEnumerator())
            {
                ParcelScene scene;
                bool characterIsInsideScene;

                while (iterator.MoveNext())
                {
                    scene = iterator.Current;

                    if (scene == null)
                        continue;

                    characterIsInsideScene = scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition);

                    if (scene.sceneData.id != globalSceneId && characterIsInsideScene)
                    {
                        currentSceneId = scene.sceneData.id;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentSceneId))
            {
                if (TryGetScene(currentSceneId, out ParcelScene scene) && scene.isReady)
                {
                    CommonScriptableObjects.rendererState.RemoveLock(this);
                }
            }

            CommonScriptableObjects.sceneID.Set(currentSceneId);

            OnSortScenes?.Invoke();
        }

        private int SortScenesByDistanceMethod(ParcelScene sceneA, ParcelScene sceneB)
        {
            sortAuxiliaryVector = sceneA.sceneData.basePosition - currentGridSceneCoordinate;
            int dist1 = sortAuxiliaryVector.sqrMagnitude;

            sortAuxiliaryVector = sceneB.sceneData.basePosition - currentGridSceneCoordinate;
            int dist2 = sortAuxiliaryVector.sqrMagnitude;

            return dist1 - dist2;
        }

        public const string EMPTY_GO_POOL_NAME = "Empty";

        public void EnsureEntityPool()
        {
            if (PoolManager.i.ContainsPool(EMPTY_GO_POOL_NAME))
                return;

            GameObject go = new GameObject();
            Pool pool = PoolManager.i.AddPool(EMPTY_GO_POOL_NAME, go, maxPrewarmCount: 2000, isPersistent: true);

            if (prewarmEntitiesPool)
                pool.ForcePrewarm();
        }

        void Start()
        {
            if (prewarmSceneMessagesPool)
            {
                for (int i = 0; i < 100000; i++)
                {
                    sceneMessagesPool.Enqueue(new MessagingBus.QueuedSceneMessage_Scene());
                }
            }

            if (prewarmEntitiesPool)
            {
                EnsureEntityPool();
            }

            componentFactory.PrewarmPools();

            if (!debugScenes)
            {
                CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChange;
                OnRenderingStateChange(CommonScriptableObjects.rendererState.Get(), false);
            }
        }

        private void OnRenderingStateChange(bool enabled, bool prevState)
        {
            if (!enabled && !string.IsNullOrEmpty(currentSceneId))
            {
                CommonScriptableObjects.rendererState.AddLock(this);
            }
        }

        private void OnSceneReady(ParcelScene scene)
        {
            if (scene.sceneData.id == currentSceneId)
            {
                CommonScriptableObjects.rendererState.RemoveLock(this);
            }
        }

        public void Restart()
        {
            MessagingControllersManager.i.Cleanup();
            MessagingControllersManager.i.Initialize(this);

            MemoryManager.i.CleanupPoolsIfNeeded(true);
            MemoryManager.i.Initialize();

            PointerEventsController.i.Cleanup();
            PointerEventsController.i.Initialize();

            ParcelScene.parcelScenesCleaner.ForceCleanup();
        }

        void OnDestroy()
        {
            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChange;
            DCLCharacterController.OnCharacterMoved -= SetPositionDirty;
            ParcelScene.parcelScenesCleaner.Stop();
        }

        private void Update()
        {
            InputController_Legacy.i.Update();

            if (lastSortFrame != Time.frameCount || sceneSortDirty)
            {
                lastSortFrame = Time.frameCount;
                sceneSortDirty = false;
                SortScenesByDistance();
            }
        }

        public void CreateUIScene(string json)
        {
#if UNITY_EDITOR
            if (debugScenes && ignoreGlobalScenes)
                return;
#endif
            CreateUISceneMessage uiScene = SafeFromJson<CreateUISceneMessage>(json);

            string uiSceneId = uiScene.id;

            if (loadedScenes.ContainsKey(uiSceneId))
                return;

            var newGameObject = new GameObject("UI Scene - " + uiSceneId);

            var newScene = newGameObject.AddComponent<GlobalScene>();
            newScene.ownerController = this;
            newScene.unloadWithDistance = false;
            newScene.isPersistent = true;

            LoadParcelScenesMessage.UnityParcelScene data = new LoadParcelScenesMessage.UnityParcelScene
            {
                id = uiSceneId,
                basePosition = new Vector2Int(0, 0),
                baseUrl = uiScene.baseUrl
            };

            newScene.SetData(data);

            loadedScenes.Add(uiSceneId, newScene);
            OnNewSceneAdded?.Invoke(newScene);

            globalSceneId = uiSceneId;

            if (!MessagingControllersManager.i.ContainsController(globalSceneId))
                MessagingControllersManager.i.AddController(this, globalSceneId, isGlobal: true);

            if (VERBOSE)
            {
                Debug.Log($"Creating UI scene {uiSceneId}");
            }
        }

        public void SetDebug()
        {
            Debug.unityLogger.logEnabled = true;

            isDebugMode = true;
            fpsPanel.SetActive(true);

            InitializeSceneBoundariesChecker();

            OnDebugModeSet?.Invoke();
        }

        public void HideFPSPanel()
        {
            fpsPanel.SetActive(false);
        }

        public void ShowFPSPanel()
        {
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

        public void SetDisableAssetBundles()
        {
            RendereableAssetLoadHelper.loadingType = RendereableAssetLoadHelper.LoadingType.GLTF_ONLY;
        }

        public bool IsCharacterInsideScene(ParcelScene scene)
        {
            return scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition);
        }

        public void LoadParcelScenesExecute(string decentralandSceneJSON)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_LOAD);
            scene = SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(decentralandSceneJSON);
            OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_LOAD);

            if (scene == null || scene.id == null) return;

            var sceneToLoad = scene;

#if UNITY_EDITOR
            if (debugScenes && sceneToLoad.basePosition.ToString() != debugSceneCoords.ToString())
            {
                SendSceneReady(sceneToLoad.id);
                return;
            }
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
                OnNewSceneAdded?.Invoke(newScene);

                scenesSortedByDistance.Add(newScene);

                if (!MessagingControllersManager.i.ContainsController(newScene.sceneData.id))
                    MessagingControllersManager.i.AddController(this, newScene.sceneData.id);

                newScene.OnSceneReady += OnSceneReady;

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
                OnNewSceneAdded?.Invoke(newScene);
            }

            OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_UPDATE);
        }

        public void UnloadScene(string sceneKey)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
                {type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_PARCEL, message = sceneKey};

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            MessagingControllersManager.i.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

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
                scene.Cleanup(!CommonScriptableObjects.rendererState.Get());

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
            {
                type = MessagingBus.QueuedSceneMessage.Type.LOAD_PARCEL,
                message = decentralandSceneJSON
            };

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_LOAD);

            MessagingControllersManager.i.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

            sceneSortDirty = true;

            if (VERBOSE)
                Debug.Log($"{Time.frameCount} : Load parcel scene queue {decentralandSceneJSON}");
        }

        public void UpdateParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
                {type = MessagingBus.QueuedSceneMessage.Type.UPDATE_PARCEL, message = decentralandSceneJSON};

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_UPDATE);

            MessagingControllersManager.i.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage() {type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_SCENES};

            OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            MessagingControllersManager.i.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public string SendSceneMessage(string payload)
        {
            return SendSceneMessage(payload, deferredMessagesDecoding);
        }

        private string SendSceneMessage(string payload, bool enqueue)
        {
            string[] chunks = payload.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            int count = chunks.Length;
            string lastBusId = null;

            for (int i = 0; i < count; i++)
            {
                if (CommonScriptableObjects.rendererState.Get() && enqueue)
                {
                    payloadsToDecode.Enqueue(chunks[i]);
                }
                else
                {
                    lastBusId = DecodeAndEnqueue(chunks[i]);
                }
            }

            return lastBusId;
        }

        private string DecodeAndEnqueue(string payload)
        {
            OnMessageDecodeStart?.Invoke("Misc");

            string sceneId;
            string message;
            string messageTag;
            PB_SendSceneMessage sendSceneMessage;

            if (!MessageDecoder.DecodePayloadChunk(payload, out sceneId, out message, out messageTag, out sendSceneMessage))
            {
                return null;
            }

            MessagingBus.QueuedSceneMessage_Scene queuedMessage;

            if (sceneMessagesPool.Count > 0)
                queuedMessage = sceneMessagesPool.Dequeue();
            else
                queuedMessage = new MessagingBus.QueuedSceneMessage_Scene();

            MessageDecoder.DecodeSceneMessage(sceneId, message, messageTag, sendSceneMessage, ref queuedMessage);

            EnqueueSceneMessage(queuedMessage);

            OnMessageDecodeEnds?.Invoke("Misc");

            return "";
        }

        private IEnumerator DeferredDecoding()
        {
            float start = Time.realtimeSinceStartup;
            float maxTimeForDecode;

            while (true)
            {
                maxTimeForDecode = CommonScriptableObjects.rendererState.Get() ? MAX_TIME_FOR_DECODE : float.MaxValue;

                if (payloadsToDecode.Count > 0)
                {
                    string payload = payloadsToDecode.Dequeue();

                    DecodeAndEnqueue(payload);

                    if (Time.realtimeSinceStartup - start < maxTimeForDecode)
                        continue;
                }

                yield return null;
                start = Time.unscaledTime;
            }
        }

        public void EnqueueSceneMessage(MessagingBus.QueuedSceneMessage_Scene queuedMessage)
        {
            TryGetScene(queuedMessage.sceneId, out ParcelScene scene);

            // If it doesn't exist, create messaging controller for this scene id
            if (!MessagingControllersManager.i.ContainsController(queuedMessage.sceneId))
                MessagingControllersManager.i.AddController(this, queuedMessage.sceneId);

            MessagingControllersManager.i.Enqueue(scene, queuedMessage);
        }

        public bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction)
        {
            string sceneId = msgObject.sceneId;
            string method = msgObject.method;

            yieldInstruction = null;

            ParcelScene scene;
            bool res = false;

            if (loadedScenes.TryGetValue(sceneId, out scene))
            {
#if UNITY_EDITOR
                if (debugScenes && scene is GlobalScene && ignoreGlobalScenes)
                {
                    return false;
                }
#endif
                if (!scene.gameObject.activeInHierarchy)
                {
                    return true;
                }

#if UNITY_EDITOR
                OnMessageProcessInfoStart?.Invoke(sceneId, method);
#endif
                OnMessageProcessStart?.Invoke(method);

                switch (method)
                {
                    case MessagingTypes.ENTITY_CREATE:
                    {
                        if (msgObject.payload is Protocol.CreateEntity payload)
                            scene.CreateEntity(payload.entityId);

                        break;
                    }
                    case MessagingTypes.ENTITY_REPARENT:
                    {
                        if (msgObject.payload is Protocol.SetEntityParent payload)
                            scene.SetEntityParent(payload.entityId, payload.parentId);

                        break;
                    }

                    case MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE:
                    {
                        if (msgObject.payload is Protocol.EntityComponentCreateOrUpdate payload)
                            scene.EntityComponentCreateOrUpdate(payload.entityId, (CLASS_ID_COMPONENT) payload.classId, payload.json, out yieldInstruction);

                        break;
                    }

                    case MessagingTypes.ENTITY_COMPONENT_DESTROY:
                    {
                        if (msgObject.payload is Protocol.EntityComponentDestroy payload)
                            scene.EntityComponentRemove(payload.entityId, payload.name);

                        break;
                    }

                    case MessagingTypes.SHARED_COMPONENT_ATTACH:
                    {
                        if (msgObject.payload is Protocol.SharedComponentAttach payload)
                            scene.SharedComponentAttach(payload.entityId, payload.id);

                        break;
                    }

                    case MessagingTypes.SHARED_COMPONENT_CREATE:
                    {
                        if (msgObject.payload is Protocol.SharedComponentCreate payload)
                            scene.SharedComponentCreate(payload.id, payload.classId);

                        break;
                    }

                    case MessagingTypes.SHARED_COMPONENT_DISPOSE:
                    {
                        if (msgObject.payload is Protocol.SharedComponentDispose payload)
                            scene.SharedComponentDispose(payload.id);
                        break;
                    }

                    case MessagingTypes.SHARED_COMPONENT_UPDATE:
                    {
                        if (msgObject.payload is Protocol.SharedComponentUpdate payload)
                            scene.SharedComponentUpdate(payload.componentId, payload.json, out yieldInstruction);
                        break;
                    }

                    case MessagingTypes.ENTITY_DESTROY:
                    {
                        if (msgObject.payload is Protocol.RemoveEntity payload)
                            scene.RemoveEntity(payload.entityId);
                        break;
                    }

                    case MessagingTypes.INIT_DONE:
                    {
                        scene.SetInitMessagesDone();
                        break;
                    }

                    case MessagingTypes.QUERY:
                    {
                        if (msgObject.payload is QueryMessage queryMessage)
                            ParseQuery(queryMessage.payload, scene.sceneData.id);
                        break;
                    }

                    case MessagingTypes.OPEN_EXTERNAL_URL:
                    {
                        if (msgObject.payload is Protocol.OpenExternalUrl payload)
                            OnOpenExternalUrlRequest?.Invoke(scene, payload.url);
                        break;
                    }

                    case MessagingTypes.OPEN_NFT_DIALOG:
                    {
                        if (msgObject.payload is Protocol.OpenNftDialog payload)
                            OnOpenNFTDialogRequest?.Invoke(payload.contactAddress, payload.tokenId, payload.comment);
                        break;
                    }

                    default:
                        Debug.LogError($"Unknown method {method}");
                        return true;
                }

                OnMessageProcessEnds?.Invoke(method);

#if UNITY_EDITOR
                OnMessageProcessInfoEnds?.Invoke(sceneId, method);
#endif

                res = true;
            }

            else
            {
                res = false;
            }

            sceneMessagesPool.Enqueue(msgObject);

            return res;
        }

        public Vector3 ConvertUnityToScenePosition(Vector3 pos, ParcelScene scene = null)
        {
            if (scene == null)
            {
                string sceneId = currentSceneId;

                if (!string.IsNullOrEmpty(sceneId) && loadedScenes.ContainsKey(sceneId))
                    scene = loadedScenes[currentSceneId];
                else
                    return pos;
            }

            Vector3 worldPosition = DCLCharacterController.i.characterPosition.UnityToWorldPosition(pos);
            return worldPosition - Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
        }

        public void ParseQuery(object payload, string sceneId)
        {
            ParcelScene scene = loadedScenes[sceneId];

            if (!(payload is RaycastQuery raycastQuery))
                return;

            Vector3 worldOrigin = raycastQuery.ray.origin + Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);

            raycastQuery.ray.unityOrigin = DCLCharacterController.i.characterPosition.WorldToUnityPosition(worldOrigin);
            raycastQuery.sceneId = sceneId;
            PhysicsCast.i.Query(raycastQuery);
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

            if (data.parcels == null)
            {
                data.parcels = new Vector2Int[] {data.basePosition};
            }

            if (string.IsNullOrEmpty(data.id))
            {
                data.id = $"(test):{data.basePosition.x},{data.basePosition.y}";
            }

            if (loadedScenes.ContainsKey(data.id))
            {
                Debug.LogWarning($"Scene {data.id} is already loaded.");
                return loadedScenes[data.id];
            }

            var go = new GameObject();
            var newScene = go.AddComponent<ParcelScene>();
            newScene.ownerController = this;
            newScene.isTestScene = true;
            newScene.isPersistent = true;
            newScene.useBlockers = false;
            newScene.SetData(data);

            if (DCLCharacterController.i != null)
                newScene.InitializeDebugPlane();

            scenesSortedByDistance.Add(newScene);

            if (!MessagingControllersManager.i.ContainsController(data.id))
                MessagingControllersManager.i.AddController(this, data.id);

            loadedScenes.Add(data.id, newScene);
            OnNewSceneAdded?.Invoke(newScene);

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

        public bool TryGetScene(string id, out ParcelScene scene)
        {
            scene = null;

            if (!loadedScenes.ContainsKey(id))
                return false;

            scene = loadedScenes[id];
            return true;
        }

        public void BuilderReady()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}