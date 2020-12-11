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
using Newtonsoft.Json;
using UnityEngine;

namespace DCL
{
    public class SceneController : IMessageProcessHandler, IMessageQueueHandler
    {
        public static bool VERBOSE = false;

        //======================================================================

        #region PROJECT_ENTRYPOINT

        //======================================================================
        private EntryPoint_World worldEntryPoint;

        public DCLComponentFactory componentFactory => Main.i.componentFactory;

        public bool enabled = true;
        public bool initialized = false;

        public void Initialize()
        {
            if (initialized)
                return;

            sceneSortDirty = true;
            positionDirty = true;
            lastSortFrame = 0;
            enabled = true;
            initialized = true;

            Environment.i.debugController.OnDebugModeSet += OnDebugModeSet;

            // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
            WebInterface.StartDecentraland();

            Environment.i.parcelScenesCleaner.Start();

            if (deferredMessagesDecoding) // We should be able to delete this code
                CoroutineStarter.Start(DeferredDecoding()); //

            DCLCharacterController.OnCharacterMoved += SetPositionDirty;

            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneIdChange;

            //TODO(Brian): Move those suscriptions elsewhere.
            PoolManager.i.OnGet -= Environment.i.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet += Environment.i.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet -= Environment.i.cullingController.objectsTracker.MarkDirty;
            PoolManager.i.OnGet += Environment.i.cullingController.objectsTracker.MarkDirty;

#if !UNITY_EDITOR
            worldEntryPoint = new EntryPoint_World(this); // independent subsystem => put at entrypoint but not at environment
#endif

            // TODO(Brian): This should be fixed when we do the proper initialization layer
            if (!EnvironmentSettings.RUNNING_TESTS)
            {
                Environment.i.cullingController.Start();
            }
        }

        private void OnDebugModeSet()
        {
            //NOTE(Brian): Added this here to prevent the SetDebug() before Awake()
            //             case. Calling Initialize multiple times in a row is safe.
            Environment.i.Initialize();
            Environment.i.worldBlockersController.SetEnabled(false);
            Environment.i.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_RedFlicker());
        }

        public void Start()
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
        }

        public void OnDestroy()
        {
            initialized = false;
            PoolManager.i.OnGet -= Environment.i.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet -= Environment.i.cullingController.objectsTracker.MarkDirty;
            DCLCharacterController.OnCharacterMoved -= SetPositionDirty;
            Environment.i.debugController.OnDebugModeSet -= OnDebugModeSet;

            Environment.i.parcelScenesCleaner.Stop();
            Environment.i.cullingController.Stop();
        }


        public void Update()
        {
            if (!enabled)
                return;

            InputController_Legacy.i.Update();

            Environment.i.pointerEventsController.Update();

            if (lastSortFrame != Time.frameCount && sceneSortDirty)
            {
                lastSortFrame = Time.frameCount;
                sceneSortDirty = false;
                SortScenesByDistance();
            }

            Environment.i.performanceMetricsController?.Update();
        }

        public void LateUpdate()
        {
            if (!enabled)
                return;

            Environment.i.physicsSyncController.Sync();
        }

        public void EnsureEntityPool() // TODO: Move to PoolManagerFactory
        {
            if (PoolManager.i.ContainsPool(EMPTY_GO_POOL_NAME))
                return;

            GameObject go = new GameObject();
            Pool pool = PoolManager.i.AddPool(EMPTY_GO_POOL_NAME, go, maxPrewarmCount: 2000, isPersistent: true);

            if (prewarmEntitiesPool)
                pool.ForcePrewarm();
        }

        //======================================================================

        #endregion

        //======================================================================


        //======================================================================

        #region MESSAGES_HANDLING

        //======================================================================


#if UNITY_EDITOR
        public delegate void ProcessDelegate(string sceneId, string method);

        public event ProcessDelegate OnMessageProcessInfoStart;
        public event ProcessDelegate OnMessageProcessInfoEnds;
#endif
        [NonSerialized]
        public bool deferredMessagesDecoding = false;

        Queue<string> payloadsToDecode = new Queue<string>();
        const float MAX_TIME_FOR_DECODE = 0.005f;

        public bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction)
        {
            string sceneId = msgObject.sceneId;
            string method = msgObject.method;

            yieldInstruction = null;

            ParcelScene scene;
            bool res = false;
            WorldState worldState = Environment.i.worldState;
            DebugConfig debugConfig = DataStore.debugConfig;

            if (worldState.loadedScenes.TryGetValue(sceneId, out scene))
            {
#if UNITY_EDITOR
                if (debugConfig.soloScene && scene is GlobalScene && debugConfig.ignoreGlobalScenes)
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
                ProfilingEvents.OnMessageProcessStart?.Invoke(method);

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

                ProfilingEvents.OnMessageProcessEnds?.Invoke(method);

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

        public void ParseQuery(object payload, string sceneId)
        {
            ParcelScene scene = Environment.i.worldState.loadedScenes[sceneId];

            if (!(payload is RaycastQuery raycastQuery))
                return;

            Vector3 worldOrigin = raycastQuery.ray.origin + Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);

            raycastQuery.ray.unityOrigin = DCLCharacterController.i.characterPosition.WorldToUnityPosition(worldOrigin);
            raycastQuery.sceneId = sceneId;
            PhysicsCast.i.Query(raycastQuery);
        }

        public void SendSceneMessage(string payload)
        {
            SendSceneMessage(payload, deferredMessagesDecoding);
        }

        private void SendSceneMessage(string payload, bool enqueue)
        {
            string[] chunks = payload.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            int count = chunks.Length;

            for (int i = 0; i < count; i++)
            {
                if (CommonScriptableObjects.rendererState.Get() && enqueue)
                {
                    payloadsToDecode.Enqueue(chunks[i]);
                }
                else
                {
                    DecodeAndEnqueue(chunks[i]);
                }
            }
        }

        private void DecodeAndEnqueue(string payload)
        {
            ProfilingEvents.OnMessageDecodeStart?.Invoke("Misc");

            string sceneId;
            string message;
            string messageTag;
            PB_SendSceneMessage sendSceneMessage;

            if (!MessageDecoder.DecodePayloadChunk(payload, out sceneId, out message, out messageTag, out sendSceneMessage))
            {
                return;
            }

            MessagingBus.QueuedSceneMessage_Scene queuedMessage;

            if (sceneMessagesPool.Count > 0)
                queuedMessage = sceneMessagesPool.Dequeue();
            else
                queuedMessage = new MessagingBus.QueuedSceneMessage_Scene();

            MessageDecoder.DecodeSceneMessage(sceneId, message, messageTag, sendSceneMessage, ref queuedMessage);

            EnqueueSceneMessage(queuedMessage);

            ProfilingEvents.OnMessageDecodeEnds?.Invoke("Misc");
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

        public void EnqueueSceneMessage(MessagingBus.QueuedSceneMessage_Scene message)
        {
            Environment.i.worldState.TryGetScene(message.sceneId, out ParcelScene scene);

            Environment.i.messagingControllersManager.AddControllerIfNotExists(this, message.sceneId);

            Environment.i.messagingControllersManager.Enqueue(scene, message);
        }

        //======================================================================

        #endregion

        //======================================================================


        //======================================================================

        #region SCENES_MANAGEMENT

        //======================================================================
        public event Action<string> OnReadyScene;

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

            if (Environment.i.worldState.loadedScenes.ContainsKey(data.id))
            {
                Debug.LogWarning($"Scene {data.id} is already loaded.");
                return Environment.i.worldState.loadedScenes[data.id];
            }

            var go = new GameObject();
            var newScene = go.AddComponent<ParcelScene>();
            newScene.ownerController = this;
            newScene.isTestScene = true;
            newScene.isPersistent = true;
            newScene.SetData(data);

            if (DCLCharacterController.i != null)
                newScene.InitializeDebugPlane();

            Environment.i.worldState.scenesSortedByDistance.Add(newScene);

            Environment.i.messagingControllersManager.AddControllerIfNotExists(this, data.id);

            Environment.i.worldState.loadedScenes.Add(data.id, newScene);
            OnNewSceneAdded?.Invoke(newScene);

            return newScene;
        }

        public void SendSceneReady(string sceneId)
        {
            Environment.i.worldState.readyScenes.Add(sceneId);

            Environment.i.messagingControllersManager.SetSceneReady(sceneId);

            WebInterface.ReportControlEvent(new WebInterface.SceneReady(sceneId));

            Environment.i.worldBlockersController.SetupWorldBlockers();

            OnReadyScene?.Invoke(sceneId);
        }


        public void ActivateBuilderInWorldEditScene()
        {
            Environment.i.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_RedFlicker());
        }

        public void DeactivateBuilderInWorldEditScene()
        {
            Environment.i.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple());
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

                // Since the first position for the character is not sent from Kernel until just-before calling
                // the rendering activation from Kernel, we need to sort the scenes to get the current scene id
                // to lock the rendering accordingly...
                if (!CommonScriptableObjects.rendererState.Get())
                {
                    SortScenesByDistance();
                }
            }
        }

        public void SortScenesByDistance()
        {
            if (DCLCharacterController.i == null) return;

            WorldState worldState = Environment.i.worldState;

            worldState.currentSceneId = null;
            worldState.scenesSortedByDistance.Sort(SortScenesByDistanceMethod);
            Debug.Log("SortScenesByDistance()");

            using (var iterator = Environment.i.worldState.scenesSortedByDistance.GetEnumerator())
            {
                ParcelScene scene;
                bool characterIsInsideScene;

                while (iterator.MoveNext())
                {
                    scene = iterator.Current;

                    if (scene == null) continue;

                    characterIsInsideScene = scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition);

                    if (scene.sceneData.id != worldState.globalSceneId && characterIsInsideScene)
                    {
                        worldState.currentSceneId = scene.sceneData.id;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(worldState.currentSceneId))
            {
                // When we don't know the current scene yet, we must lock the rendering from enabling until it is set
                CommonScriptableObjects.rendererState.AddLock(this);
            }
            else
            {
                // 1. Set current scene id
                CommonScriptableObjects.sceneID.Set(worldState.currentSceneId);

                // 2. Attempt to remove SceneController's lock on rendering
                CommonScriptableObjects.rendererState.RemoveLock(this);
            }

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

        private void OnCurrentSceneIdChange(string newSceneId, string prevSceneId)
        {
            if (Environment.i.worldState.TryGetScene(newSceneId, out ParcelScene newCurrentScene) && !newCurrentScene.isReady)
            {
                CommonScriptableObjects.rendererState.AddLock(newCurrentScene);

                newCurrentScene.OnSceneReady += (readyScene) => { CommonScriptableObjects.rendererState.RemoveLock(readyScene); };
            }
        }

        public void LoadParcelScenesExecute(string decentralandSceneJSON)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_LOAD);
            scene = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(decentralandSceneJSON);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_LOAD);

            if (scene == null || scene.id == null) return;

            var sceneToLoad = scene;


            DebugConfig debugConfig = DataStore.debugConfig;
#if UNITY_EDITOR
            if (debugConfig.soloScene && sceneToLoad.basePosition.ToString() != debugConfig.soloSceneCoords.ToString())
            {
                SendSceneReady(sceneToLoad.id);
                return;
            }
#endif

            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_LOAD);

            WorldState worldState = Environment.i.worldState;

            if (!worldState.loadedScenes.ContainsKey(sceneToLoad.id))
            {
                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                newScene.SetData(sceneToLoad);

                if (debugConfig.isDebugMode)
                {
                    newScene.InitializeDebugPlane();
                }

                newScene.ownerController = this;
                worldState.loadedScenes.Add(sceneToLoad.id, newScene);
                worldState.scenesSortedByDistance.Add(newScene);

                sceneSortDirty = true;

                OnNewSceneAdded?.Invoke(newScene);

                Environment.i.messagingControllersManager.AddControllerIfNotExists(this, newScene.sceneData.id);

                if (VERBOSE)
                    Debug.Log($"{Time.frameCount} : Load parcel scene {newScene.sceneData.basePosition}");
            }

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_LOAD);
        }


        public void UpdateParcelScenesExecute(string decentralandSceneJSON)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            scene = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(decentralandSceneJSON);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_UPDATE);

            if (Environment.i.worldState.loadedScenes.ContainsKey(scene.id))
                Environment.i.worldState.loadedScenes[scene.id].SetUpdateData(scene);
            else
                LoadParcelScenesExecute(decentralandSceneJSON);
        }

        public void UpdateParcelScenesExecute(LoadParcelScenesMessage.UnityParcelScene scene)
        {
            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;

            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            Environment.i.worldState.loadedScenes[sceneToLoad.id].SetUpdateData(sceneToLoad);
            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_UPDATE);
        }

        public void UnloadScene(string sceneKey)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
                {type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_PARCEL, message = sceneKey};

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            Environment.i.messagingControllersManager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

            Environment.i.messagingControllersManager.RemoveController(sceneKey);
        }

        public void UnloadParcelSceneExecute(string sceneKey)
        {
            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_DESTROY);

            WorldState worldState = Environment.i.worldState;

            if (!worldState.loadedScenes.ContainsKey(sceneKey) || worldState.loadedScenes[sceneKey].isPersistent)
            {
                return;
            }

            var scene = worldState.loadedScenes[sceneKey];

            worldState.loadedScenes.Remove(sceneKey);

            // Remove the scene id from the msg. priorities list
            worldState.scenesSortedByDistance.Remove(scene);

            // Remove messaging controller for unloaded scene
            Environment.i.messagingControllersManager.RemoveController(scene.sceneData.id);

            if (scene)
            {
                scene.Cleanup(!CommonScriptableObjects.rendererState.Get());

                if (VERBOSE)
                {
                    Debug.Log($"{Time.frameCount} : Destroying scene {scene.sceneData.basePosition}");
                }
            }

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_DESTROY);
        }

        public void UnloadAllScenes()
        {
            var list = Environment.i.worldState.loadedScenes.ToArray();
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

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_LOAD);

            Environment.i.messagingControllersManager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

            if (VERBOSE)
                Debug.Log($"{Time.frameCount} : Load parcel scene queue {decentralandSceneJSON}");
        }

        public void UpdateParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
                {type = MessagingBus.QueuedSceneMessage.Type.UPDATE_PARCEL, message = decentralandSceneJSON};

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_UPDATE);

            Environment.i.messagingControllersManager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage() {type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_SCENES};

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            Environment.i.messagingControllersManager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public void CreateUIScene(string json)
        {
#if UNITY_EDITOR
            DebugConfig debugConfig = DataStore.debugConfig;

            if (debugConfig.soloScene && debugConfig.ignoreGlobalScenes)
                return;
#endif
            CreateUISceneMessage uiScene = Utils.SafeFromJson<CreateUISceneMessage>(json);

            string uiSceneId = uiScene.id;

            WorldState worldState = Environment.i.worldState;

            if (worldState.loadedScenes.ContainsKey(uiSceneId))
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

            worldState.loadedScenes.Add(uiSceneId, newScene);
            OnNewSceneAdded?.Invoke(newScene);

            worldState.globalSceneId = uiSceneId;

            Environment.i.messagingControllersManager.AddControllerIfNotExists(this, worldState.globalSceneId, isGlobal: true);

            if (VERBOSE)
            {
                Debug.Log($"Creating UI scene {uiSceneId}");
            }
        }

        public void IsolateScene(ParcelScene sceneToActive)
        {
            foreach (ParcelScene scene in Environment.i.worldState.scenesSortedByDistance)
            {
                if (scene != sceneToActive) scene.gameObject.SetActive(false);
            }
        }

        public void ReIntegrateIsolatedScene()
        {
            foreach (ParcelScene scene in Environment.i.worldState.scenesSortedByDistance)
            {
                scene.gameObject.SetActive(true);
            }
        }

        //======================================================================

        #endregion

        //======================================================================

        public Queue<MessagingBus.QueuedSceneMessage_Scene> sceneMessagesPool { get; } = new Queue<MessagingBus.QueuedSceneMessage_Scene>();

        [System.NonSerialized]
        public bool prewarmSceneMessagesPool = true;

        [System.NonSerialized]
        public bool prewarmEntitiesPool = true;

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


        public const string EMPTY_GO_POOL_NAME = "Empty";
    }
}