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
    public interface ISceneController : IMessageProcessHandler, IMessageQueueHandler
    {
        bool enabled { get; set; }
        bool deferredMessagesDecoding { get; set; }
        bool prewarmSceneMessagesPool { get; set; }
        bool prewarmEntitiesPool { get; set; }
        IRuntimeComponentFactory componentFactory { get; }
        void Initialize(IRuntimeComponentFactory componentFactory);
        void Start();
        void Dispose();
        void Update();
        void LateUpdate();

        void EnsureEntityPool(); // TODO: Move to PoolManagerFactory

#if UNITY_EDITOR
        event SceneController.ProcessDelegate OnMessageProcessInfoStart;
        event SceneController.ProcessDelegate OnMessageProcessInfoEnds;
#endif
        void ParseQuery(object payload, string sceneId);
        void SendSceneMessage(string payload);
        event Action<string> OnReadyScene;
        ParcelScene CreateTestScene(LoadParcelScenesMessage.UnityParcelScene data = null);
        void SendSceneReady(string sceneId);
        void ActivateBuilderInWorldEditScene();
        void DeactivateBuilderInWorldEditScene();
        void SortScenesByDistance();
        void UpdateParcelScenesExecute(LoadParcelScenesMessage.UnityParcelScene scene);
        void UnloadScene(string sceneKey);
        void LoadParcelScenes(string decentralandSceneJSON);
        void UpdateParcelScenes(string decentralandSceneJSON);
        void UnloadAllScenesQueued();
        void CreateUIScene(string json);
        void IsolateScene(ParcelScene sceneToActive);
        void ReIntegrateIsolatedScene();
        event Action OnSortScenes;
        event Action<ParcelScene, string> OnOpenExternalUrlRequest;
        event Action<ParcelScene> OnNewSceneAdded;
        event SceneController.OnOpenNFTDialogDelegate OnOpenNFTDialogRequest;
    }

    public class SceneController : ISceneController
    {
        public static bool VERBOSE = false;

        public IRuntimeComponentFactory componentFactory { get; private set; }

        public bool enabled { get; set; } = true;

        private Coroutine deferredDecodingCoroutine;

        public void Initialize(IRuntimeComponentFactory componentFactory)
        {
            sceneSortDirty = true;
            positionDirty = true;
            lastSortFrame = 0;
            enabled = true;
            this.componentFactory = componentFactory;

            Environment.i.platform.debugController.OnDebugModeSet += OnDebugModeSet;

            // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
            WebInterface.StartDecentraland();

            if (deferredMessagesDecoding) // We should be able to delete this code
                deferredDecodingCoroutine = CoroutineStarter.Start(DeferredDecoding()); //

            DCLCharacterController.OnCharacterMoved += SetPositionDirty;

            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneIdChange;

            //TODO(Brian): Move those subscriptions elsewhere.
            PoolManager.i.OnGet -= Environment.i.platform.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet += Environment.i.platform.physicsSyncController.MarkDirty;

            PoolManager.i.OnGet -= Environment.i.platform.cullingController.objectsTracker.MarkDirty;
            PoolManager.i.OnGet += Environment.i.platform.cullingController.objectsTracker.MarkDirty;
        }

        private void OnDebugModeSet()
        {
            if (Environment.i.world == null)
            {
                //NOTE(Brian): Added this here to prevent the SetDebug() before Awake()
                //             case. Calling Initialize multiple times in a row is safe.
                Environment.SetupWithBuilders();
            }

            Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_RedFlicker());
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

            // Warmup some shader variants
            Resources.Load<ShaderVariantCollection>("ShaderVariantCollections/shaderVariants-selected").WarmUp();
        }

        public void Dispose()
        {
            PoolManager.i.OnGet -= Environment.i.platform.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet -= Environment.i.platform.cullingController.objectsTracker.MarkDirty;
            DCLCharacterController.OnCharacterMoved -= SetPositionDirty;
            Environment.i.platform.debugController.OnDebugModeSet -= OnDebugModeSet;

            UnloadAllScenes(includePersistent: true);

            if (deferredDecodingCoroutine != null)
                CoroutineStarter.Stop(deferredDecodingCoroutine);
        }


        public void Update()
        {
            if (!enabled)
                return;

            InputController_Legacy.i.Update();

            Environment.i.world.pointerEventsController.Update();

            if (lastSortFrame != Time.frameCount && sceneSortDirty)
            {
                lastSortFrame = Time.frameCount;
                sceneSortDirty = false;
                SortScenesByDistance();
            }
        }

        public void LateUpdate()
        {
            if (!enabled)
                return;

            Environment.i.platform.physicsSyncController.Sync();
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

        #region MESSAGES_HANDLING

        //======================================================================


#if UNITY_EDITOR
        public delegate void ProcessDelegate(string sceneId, string method);

        public event ProcessDelegate OnMessageProcessInfoStart;
        public event ProcessDelegate OnMessageProcessInfoEnds;
#endif
        public bool deferredMessagesDecoding { get; set; } = false;

        Queue<string> payloadsToDecode = new Queue<string>();
        const float MAX_TIME_FOR_DECODE = 0.005f;

        public bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction)
        {
            string sceneId = msgObject.sceneId;
            string method = msgObject.method;

            yieldInstruction = null;

            ParcelScene scene;
            bool res = false;
            IWorldState worldState = Environment.i.world.state;
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

                ProcessMessage(scene, method, msgObject.payload, out yieldInstruction);

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

        private void ProcessMessage(ParcelScene scene, string method, object msgPayload,
            out CleanableYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;

            try
            {
                switch (method)
                {
                    case MessagingTypes.ENTITY_CREATE:
                    {
                        if (msgPayload is Protocol.CreateEntity payload)
                            scene.CreateEntity(payload.entityId);

                        break;
                    }
                    case MessagingTypes.ENTITY_REPARENT:
                    {
                        if (msgPayload is Protocol.SetEntityParent payload)
                            scene.SetEntityParent(payload.entityId, payload.parentId);

                        break;
                    }

                    case MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE:
                    {
                        if (msgPayload is Protocol.EntityComponentCreateOrUpdate payload)
                            scene.EntityComponentCreateOrUpdate(payload.entityId,
                                (CLASS_ID_COMPONENT)payload.classId, payload.json, out yieldInstruction);

                        break;
                    }

                    case MessagingTypes.ENTITY_COMPONENT_DESTROY:
                    {
                        if (msgPayload is Protocol.EntityComponentDestroy payload)
                            scene.EntityComponentRemove(payload.entityId, payload.name);

                        break;
                    }

                    case MessagingTypes.SHARED_COMPONENT_ATTACH:
                    {
                        if (msgPayload is Protocol.SharedComponentAttach payload)
                            scene.SharedComponentAttach(payload.entityId, payload.id);

                        break;
                    }

                    case MessagingTypes.SHARED_COMPONENT_CREATE:
                    {
                        if (msgPayload is Protocol.SharedComponentCreate payload)
                            scene.SharedComponentCreate(payload.id, payload.classId);

                        break;
                    }

                    case MessagingTypes.SHARED_COMPONENT_DISPOSE:
                    {
                        if (msgPayload is Protocol.SharedComponentDispose payload)
                            scene.SharedComponentDispose(payload.id);
                        break;
                    }

                    case MessagingTypes.SHARED_COMPONENT_UPDATE:
                    {
                        if (msgPayload is Protocol.SharedComponentUpdate payload)
                            scene.SharedComponentUpdate(payload.componentId, payload.json, out yieldInstruction);
                        break;
                    }

                    case MessagingTypes.ENTITY_DESTROY:
                    {
                        if (msgPayload is Protocol.RemoveEntity payload)
                            scene.RemoveEntity(payload.entityId);
                        break;
                    }

                    case MessagingTypes.INIT_DONE:
                    {
                        scene.sceneLifecycleHandler.SetInitMessagesDone();
                        break;
                    }

                    case MessagingTypes.QUERY:
                    {
                        if (msgPayload is QueryMessage queryMessage)
                            ParseQuery(queryMessage.payload, scene.sceneData.id);
                        break;
                    }

                    case MessagingTypes.OPEN_EXTERNAL_URL:
                    {
                        if (msgPayload is Protocol.OpenExternalUrl payload)
                            OnOpenExternalUrlRequest?.Invoke(scene, payload.url);
                        break;
                    }

                    case MessagingTypes.OPEN_NFT_DIALOG:
                    {
                        if (msgPayload is Protocol.OpenNftDialog payload)
                            OnOpenNFTDialogRequest?.Invoke(payload.contactAddress, payload.tokenId,
                                payload.comment);
                        break;
                    }

                    default:
                        Debug.LogError($"Unknown method {method}");
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Scene message error. scene: {scene.sceneData.id} method: {method} payload: {JsonUtility.ToJson(msgPayload)} {e}");
            }
        }

        public void ParseQuery(object payload, string sceneId)
        {
            ParcelScene scene = Environment.i.world.state.loadedScenes[sceneId];

            if (!(payload is RaycastQuery raycastQuery))
                return;

            Vector3 worldOrigin = raycastQuery.ray.origin + Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);

            raycastQuery.ray.unityOrigin = PositionUtils.WorldToUnityPosition(worldOrigin);
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
            Environment.i.world.state.TryGetScene(message.sceneId, out ParcelScene scene);

            Environment.i.messaging.manager.AddControllerIfNotExists(this, message.sceneId);

            Environment.i.messaging.manager.Enqueue(scene, message);
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

            if (Environment.i.world.state.loadedScenes.ContainsKey(data.id))
            {
                Debug.LogWarning($"Scene {data.id} is already loaded.");
                return Environment.i.world.state.loadedScenes[data.id];
            }

            var go = new GameObject();
            var newScene = go.AddComponent<ParcelScene>();
            newScene.ownerController = this;
            newScene.isTestScene = true;
            newScene.isPersistent = true;
            newScene.SetData(data);

            if (DCLCharacterController.i != null)
                newScene.InitializeDebugPlane();

            Environment.i.world.state.scenesSortedByDistance.Add(newScene);

            Environment.i.messaging.manager.AddControllerIfNotExists(this, data.id);

            Environment.i.world.state.loadedScenes.Add(data.id, newScene);
            OnNewSceneAdded?.Invoke(newScene);

            return newScene;
        }

        public void SendSceneReady(string sceneId)
        {
            Environment.i.world.state.readyScenes.Add(sceneId);

            Environment.i.messaging.manager.SetSceneReady(sceneId);

            WebInterface.ReportControlEvent(new WebInterface.SceneReady(sceneId));

            Environment.i.world.blockersController.SetupWorldBlockers();

            OnReadyScene?.Invoke(sceneId);
        }

        public void ActivateBuilderInWorldEditScene()
        {
            Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_RedFlicker());
        }

        public void DeactivateBuilderInWorldEditScene()
        {
            Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple());
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

            IWorldState worldState = Environment.i.world.state;

            worldState.currentSceneId = null;
            worldState.scenesSortedByDistance.Sort(SortScenesByDistanceMethod);

            using (var iterator = Environment.i.world.state.scenesSortedByDistance.GetEnumerator())
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

            if (!DataStore.debugConfig.isDebugMode && string.IsNullOrEmpty(worldState.currentSceneId))
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
            if (Environment.i.world.state.TryGetScene(newSceneId, out ParcelScene newCurrentScene) && !newCurrentScene.sceneLifecycleHandler.isReady)
            {
                CommonScriptableObjects.rendererState.AddLock(newCurrentScene);

                newCurrentScene.sceneLifecycleHandler.OnSceneReady += (readyScene) => { CommonScriptableObjects.rendererState.RemoveLock(readyScene); };
            }
        }

        public void LoadParcelScenesExecute(string scenePayload)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_LOAD);
            scene = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(scenePayload);
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

            IWorldState worldState = Environment.i.world.state;

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

                Environment.i.messaging.manager.AddControllerIfNotExists(this, newScene.sceneData.id);

                if (VERBOSE)
                    Debug.Log($"{Time.frameCount} : Load parcel scene {newScene.sceneData.basePosition}");
            }

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_LOAD);
        }


        public void UpdateParcelScenesExecute(string sceneId)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            scene = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(sceneId);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_UPDATE);

            if (Environment.i.world.state.loadedScenes.ContainsKey(scene.id))
                Environment.i.world.state.loadedScenes[scene.id].SetUpdateData(scene);
            else
                LoadParcelScenesExecute(sceneId);
        }

        public void UpdateParcelScenesExecute(LoadParcelScenesMessage.UnityParcelScene scene)
        {
            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;

            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            Environment.i.world.state.loadedScenes[sceneToLoad.id].SetUpdateData(sceneToLoad);
            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_UPDATE);
        }

        public void UnloadScene(string sceneKey)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
                {type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_PARCEL, message = sceneKey};

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

            Environment.i.messaging.manager.RemoveController(sceneKey);
        }

        public void UnloadParcelSceneExecute(string sceneId)
        {
            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_DESTROY);

            IWorldState worldState = Environment.i.world.state;

            if (!worldState.loadedScenes.ContainsKey(sceneId) || worldState.loadedScenes[sceneId].isPersistent)
            {
                return;
            }

            var scene = worldState.loadedScenes[sceneId];

            worldState.loadedScenes.Remove(sceneId);

            // Remove the scene id from the msg. priorities list
            worldState.scenesSortedByDistance.Remove(scene);

            // Remove messaging controller for unloaded scene
            Environment.i.messaging.manager.RemoveController(scene.sceneData.id);

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

        public void UnloadAllScenes(bool includePersistent = false)
        {
            var worldState = Environment.i.world.state;

            if (includePersistent)
            {
                var persistentScenes = worldState.loadedScenes.Where(x => x.Value.isPersistent);

                foreach (var kvp in persistentScenes)
                {
                    kvp.Value.isPersistent = false;
                }
            }

            var list = worldState.loadedScenes.ToArray();

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

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

            if (VERBOSE)
                Debug.Log($"{Time.frameCount} : Load parcel scene queue {decentralandSceneJSON}");
        }

        public void UpdateParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage()
                {type = MessagingBus.QueuedSceneMessage.Type.UPDATE_PARCEL, message = decentralandSceneJSON};

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_UPDATE);

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new MessagingBus.QueuedSceneMessage() {type = MessagingBus.QueuedSceneMessage.Type.UNLOAD_SCENES};

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
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

            IWorldState worldState = Environment.i.world.state;

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

            Environment.i.messaging.manager.AddControllerIfNotExists(this, worldState.globalSceneId, isGlobal: true);

            if (VERBOSE)
            {
                Debug.Log($"Creating UI scene {uiSceneId}");
            }
        }

        public void IsolateScene(ParcelScene sceneToActive)
        {
            foreach (ParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
            {
                if (scene != sceneToActive) scene.gameObject.SetActive(false);
            }
        }

        public void ReIntegrateIsolatedScene()
        {
            foreach (ParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
            {
                scene.gameObject.SetActive(true);
            }
        }

        //======================================================================

        #endregion

        //======================================================================

        public Queue<MessagingBus.QueuedSceneMessage_Scene> sceneMessagesPool { get; } = new Queue<MessagingBus.QueuedSceneMessage_Scene>();

        public bool prewarmSceneMessagesPool { get; set; } = true;
        public bool prewarmEntitiesPool { get; set; } = true;

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