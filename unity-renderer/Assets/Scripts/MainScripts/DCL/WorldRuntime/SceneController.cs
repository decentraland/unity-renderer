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
    public class SceneController : ISceneController
    {
        public static bool VERBOSE = false;
        const int SCENE_MESSAGES_PREWARM_COUNT = 100000;

        public bool enabled { get; set; } = true;

        //TODO(Brian): Move to WorldRuntimePlugin later
        private LoadingFeedbackController loadingFeedbackController;

        private Coroutine deferredDecodingCoroutine;

        public void Initialize()
        {
            sceneSortDirty = true;
            positionDirty = true;
            lastSortFrame = 0;
            enabled = true;

            loadingFeedbackController = new LoadingFeedbackController();

            DataStore.i.debugConfig.isDebugMode.OnChange += OnDebugModeSet;

            if (deferredMessagesDecoding) // We should be able to delete this code
                deferredDecodingCoroutine = CoroutineStarter.Start(DeferredDecoding());

            DCLCharacterController.OnCharacterMoved += SetPositionDirty;

            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneIdChange;

            // TODO(Brian): Move this later to Main.cs
            if ( !EnvironmentSettings.RUNNING_TESTS )
            {
                if (prewarmSceneMessagesPool)
                {
                    for (int i = 0; i < SCENE_MESSAGES_PREWARM_COUNT; i++)
                    {
                        sceneMessagesPool.Enqueue(new QueuedSceneMessage_Scene());
                    }
                }

                if (prewarmEntitiesPool)
                {
                    PoolManagerFactory.EnsureEntityPool(prewarmEntitiesPool);
                }
            }

            DCL.Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            DCL.Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        }

        private void OnDebugModeSet(bool current, bool previous)
        {
            if (current == previous)
                return;

            if (current)
            {
                Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_RedBox());
            }
            else
            {
                Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple());
            }
        }

        public void Dispose()
        {
            loadingFeedbackController.Dispose();

            DCL.Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
            DCL.Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

            PoolManager.i.OnGet -= Environment.i.platform.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet -= Environment.i.platform.cullingController.objectsTracker.MarkDirty;

            DCLCharacterController.OnCharacterMoved -= SetPositionDirty;
            DataStore.i.debugConfig.isDebugMode.OnChange -= OnDebugModeSet;

            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneIdChange;

            UnloadAllScenes(includePersistent: true);

            if (deferredDecodingCoroutine != null)
                CoroutineStarter.Stop(deferredDecodingCoroutine);
        }

        public void Update()
        {
            if (!enabled)
                return;

            InputController_Legacy.i.Update();

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

        public bool ProcessMessage(QueuedSceneMessage_Scene msgObject, out CustomYieldInstruction yieldInstruction)
        {
            string sceneId = msgObject.sceneId;
            string method = msgObject.method;

            yieldInstruction = null;

            IParcelScene scene;
            bool res = false;
            IWorldState worldState = Environment.i.world.state;
            DebugConfig debugConfig = DataStore.i.debugConfig;

            if (worldState.loadedScenes.TryGetValue(sceneId, out scene))
            {
#if UNITY_EDITOR
                if (debugConfig.soloScene && scene is GlobalScene && debugConfig.ignoreGlobalScenes)
                {
                    return false;
                }
#endif
                if (!scene.GetSceneTransform().gameObject.activeInHierarchy)
                {
                    return true;
                }

#if UNITY_EDITOR
                OnMessageProcessInfoStart?.Invoke(sceneId, method);
#endif
                ProfilingEvents.OnMessageProcessStart?.Invoke(method);

                ProcessMessage(scene as ParcelScene, method, msgObject.payload, out yieldInstruction);

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
            out CustomYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;
            IDelayedComponent delayedComponent = null;

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
                            {
                                delayedComponent = scene.EntityComponentCreateOrUpdate(payload.entityId,
                                    (CLASS_ID_COMPONENT) payload.classId, payload.json) as IDelayedComponent;
                            }

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
                                delayedComponent = scene.SharedComponentUpdate(payload.componentId, payload.json) as IDelayedComponent;

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
                                DataStore.i.common.onOpenNFTPrompt.Set(new NFTPromptModel(payload.contactAddress, payload.tokenId,
                                    payload.comment), true);
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

            if (delayedComponent != null)
            {
                if (delayedComponent.isRoutineRunning)
                    yieldInstruction = delayedComponent.yieldInstruction;
            }
        }

        public void ParseQuery(object payload, string sceneId)
        {
            IParcelScene scene = Environment.i.world.state.loadedScenes[sceneId];

            if (!(payload is RaycastQuery raycastQuery))
                return;

            Vector3 worldOrigin = raycastQuery.ray.origin + Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);

            raycastQuery.ray.unityOrigin = PositionUtils.WorldToUnityPosition(worldOrigin);
            raycastQuery.sceneId = sceneId;
            PhysicsCast.i.Query(raycastQuery);
        }

        public void SendSceneMessage(string payload) { SendSceneMessage(payload, deferredMessagesDecoding); }

        private void SendSceneMessage(string payload, bool enqueue)
        {
            string[] chunks = payload.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
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

            QueuedSceneMessage_Scene queuedMessage;

            if (sceneMessagesPool.Count > 0)
                queuedMessage = sceneMessagesPool.Dequeue();
            else
                queuedMessage = new QueuedSceneMessage_Scene();

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

        public void EnqueueSceneMessage(QueuedSceneMessage_Scene message)
        {
            bool isGlobalScene = WorldStateUtils.IsGlobalScene(message.sceneId);
            Environment.i.messaging.manager.AddControllerIfNotExists(this, message.sceneId);
            Environment.i.messaging.manager.Enqueue(isGlobalScene, message);
        }

        //======================================================================

        #endregion

        //======================================================================

        //======================================================================

        #region SCENES_MANAGEMENT

        //======================================================================
        public event Action<string> OnReadyScene;

        public IParcelScene CreateTestScene(LoadParcelScenesMessage.UnityParcelScene data = null)
        {
            IParcelScene result = WorldStateUtils.CreateTestScene(data);
            Environment.i.messaging.manager.AddControllerIfNotExists(this, data.id);
            OnNewSceneAdded?.Invoke(result);
            return result;
        }

        public void SendSceneReady(string sceneId)
        {
            Environment.i.world.state.readyScenes.Add(sceneId);

            Environment.i.messaging.manager.SetSceneReady(sceneId);

            WebInterface.ReportControlEvent(new WebInterface.SceneReady(sceneId));
            WebInterface.ReportCameraChanged(CommonScriptableObjects.cameraMode.Get(), sceneId);

            Environment.i.world.blockersController.SetupWorldBlockers();

            OnReadyScene?.Invoke(sceneId);
        }

        public void ActivateBuilderInWorldEditScene() { Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_BIW()); }

        public void DeactivateBuilderInWorldEditScene() { Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_Simple()); }

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
            if (DCLCharacterController.i == null)
                return;

            IWorldState worldState = Environment.i.world.state;

            worldState.currentSceneId = null;
            worldState.scenesSortedByDistance.Sort(SortScenesByDistanceMethod);

            using (var iterator = Environment.i.world.state.scenesSortedByDistance.GetEnumerator())
            {
                IParcelScene scene;
                bool characterIsInsideScene;

                while (iterator.MoveNext())
                {
                    scene = iterator.Current;

                    if (scene == null)
                        continue;

                    characterIsInsideScene = WorldStateUtils.IsCharacterInsideScene(scene);

                    if (!worldState.globalSceneIds.Contains(scene.sceneData.id) && characterIsInsideScene)
                    {
                        worldState.currentSceneId = scene.sceneData.id;
                        break;
                    }
                }
            }

            if (!DataStore.i.debugConfig.isDebugMode.Get() && string.IsNullOrEmpty(worldState.currentSceneId))
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

        private int SortScenesByDistanceMethod(IParcelScene sceneA, IParcelScene sceneB)
        {
            sortAuxiliaryVector = sceneA.sceneData.basePosition - currentGridSceneCoordinate;
            int dist1 = sortAuxiliaryVector.sqrMagnitude;

            sortAuxiliaryVector = sceneB.sceneData.basePosition - currentGridSceneCoordinate;
            int dist2 = sortAuxiliaryVector.sqrMagnitude;

            return dist1 - dist2;
        }

        private void OnCurrentSceneIdChange(string newSceneId, string prevSceneId)
        {
            if (Environment.i.world.state.TryGetScene(newSceneId, out IParcelScene newCurrentScene)
                && !(newCurrentScene as ParcelScene).sceneLifecycleHandler.isReady)
            {
                CommonScriptableObjects.rendererState.AddLock(newCurrentScene);

                (newCurrentScene as ParcelScene).sceneLifecycleHandler.OnSceneReady += (readyScene) => { CommonScriptableObjects.rendererState.RemoveLock(readyScene); };
            }
        }

        public void LoadParcelScenesExecute(string scenePayload)
        {
            LoadParcelScenesMessage.UnityParcelScene scene;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_LOAD);
            scene = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(scenePayload);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_LOAD);

            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;


            DebugConfig debugConfig = DataStore.i.debugConfig;
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

                if (debugConfig.isDebugMode.Get())
                {
                    newScene.InitializeDebugPlane();
                }

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
            LoadParcelScenesMessage.UnityParcelScene sceneData;

            ProfilingEvents.OnMessageDecodeStart?.Invoke(MessagingTypes.SCENE_UPDATE);
            sceneData = Utils.SafeFromJson<LoadParcelScenesMessage.UnityParcelScene>(sceneId);
            ProfilingEvents.OnMessageDecodeEnds?.Invoke(MessagingTypes.SCENE_UPDATE);

            IWorldState worldState = Environment.i.world.state;

            if (worldState.TryGetScene(sceneData.id, out IParcelScene sceneInterface))
            {
                ParcelScene scene = sceneInterface as ParcelScene;
                scene.SetUpdateData(sceneData);
            }
            else
            {
                LoadParcelScenesExecute(sceneId);
            }
        }

        public void UpdateParcelScenesExecute(LoadParcelScenesMessage.UnityParcelScene scene)
        {
            if (scene == null || scene.id == null)
                return;

            var sceneToLoad = scene;

            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_UPDATE);

            ParcelScene parcelScene = Environment.i.world.state.GetScene(sceneToLoad.id) as ParcelScene;

            if (parcelScene != null)
                parcelScene.SetUpdateData(sceneToLoad);

            ProfilingEvents.OnMessageProcessEnds?.Invoke(MessagingTypes.SCENE_UPDATE);
        }

        public void UnloadScene(string sceneKey)
        {
            var queuedMessage = new QueuedSceneMessage()
                { type = QueuedSceneMessage.Type.UNLOAD_PARCEL, message = sceneKey };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

            Environment.i.messaging.manager.RemoveController(sceneKey);

            IWorldState worldState = Environment.i.world.state;

            if (worldState.loadedScenes.ContainsKey(sceneKey))
            {
                ParcelScene sceneToUnload = worldState.GetScene(sceneKey) as ParcelScene;
                sceneToUnload.isPersistent = false;

                if (sceneToUnload is GlobalScene globalScene && globalScene.isPortableExperience)
                    OnNewPortableExperienceSceneRemoved?.Invoke(sceneKey);
            }
        }

        public void UnloadParcelSceneExecute(string sceneId)
        {
            ProfilingEvents.OnMessageProcessStart?.Invoke(MessagingTypes.SCENE_DESTROY);

            IWorldState worldState = Environment.i.world.state;

            if (!worldState.Contains(sceneId) || worldState.loadedScenes[sceneId].isPersistent)
            {
                return;
            }

            var scene = worldState.loadedScenes[sceneId] as ParcelScene;

            if (scene == null)
                return;

            worldState.loadedScenes.Remove(sceneId);
            worldState.globalSceneIds.Remove(sceneId);

            // Remove the scene id from the msg. priorities list
            worldState.scenesSortedByDistance.Remove(scene);

            // Remove messaging controller for unloaded scene
            Environment.i.messaging.manager.RemoveController(scene.sceneData.id);

            scene.Cleanup(!CommonScriptableObjects.rendererState.Get());


            if (VERBOSE)
            {
                Debug.Log($"{Time.frameCount} : Destroying scene {scene.sceneData.basePosition}");
            }

            Environment.i.world.blockersController.SetupWorldBlockers();

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
                    if (kvp.Value is ParcelScene scene)
                    {
                        scene.isPersistent = false;
                    }
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
            var queuedMessage = new QueuedSceneMessage()
            {
                type = QueuedSceneMessage.Type.LOAD_PARCEL,
                message = decentralandSceneJSON
            };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_LOAD);

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);

            if (VERBOSE)
                Debug.Log($"{Time.frameCount} : Load parcel scene queue {decentralandSceneJSON}");
        }

        public void UpdateParcelScenes(string decentralandSceneJSON)
        {
            var queuedMessage = new QueuedSceneMessage()
                { type = QueuedSceneMessage.Type.UPDATE_PARCEL, message = decentralandSceneJSON };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_UPDATE);

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public void UnloadAllScenesQueued()
        {
            var queuedMessage = new QueuedSceneMessage() { type = QueuedSceneMessage.Type.UNLOAD_SCENES };

            ProfilingEvents.OnMessageWillQueue?.Invoke(MessagingTypes.SCENE_DESTROY);

            Environment.i.messaging.manager.ForceEnqueueToGlobal(MessagingBusType.INIT, queuedMessage);
        }

        public void CreateGlobalScene(string json)
        {
#if UNITY_EDITOR
            DebugConfig debugConfig = DataStore.i.debugConfig;

            if (debugConfig.soloScene && debugConfig.ignoreGlobalScenes)
                return;
#endif
            CreateGlobalSceneMessage globalScene = Utils.SafeFromJson<CreateGlobalSceneMessage>(json);

            string newGlobalSceneId = globalScene.id;

            IWorldState worldState = Environment.i.world.state;

            if (worldState.loadedScenes.ContainsKey(newGlobalSceneId))
                return;

            var newGameObject = new GameObject("Global Scene - " + newGlobalSceneId);

            var newScene = newGameObject.AddComponent<GlobalScene>();
            newScene.unloadWithDistance = false;
            newScene.isPersistent = true;
            newScene.sceneName = globalScene.name;
            newScene.isPortableExperience = globalScene.isPortableExperience;

            LoadParcelScenesMessage.UnityParcelScene data = new LoadParcelScenesMessage.UnityParcelScene
            {
                id = newGlobalSceneId,
                basePosition = new Vector2Int(0, 0),
                baseUrl = globalScene.baseUrl,
                contents = globalScene.contents
            };

            newScene.SetData(data);

            if (!string.IsNullOrEmpty(globalScene.icon))
            {
                newScene.iconUrl = newScene.contentProvider.GetContentsUrl(globalScene.icon);
            }

            worldState.loadedScenes.Add(newGlobalSceneId, newScene);
            OnNewSceneAdded?.Invoke(newScene);

            if (newScene.isPortableExperience)
                OnNewPortableExperienceSceneAdded?.Invoke(newScene);

            worldState.globalSceneIds.Add(newGlobalSceneId);

            Environment.i.messaging.manager.AddControllerIfNotExists(this, newGlobalSceneId, isGlobal: true);

            if (VERBOSE)
            {
                Debug.Log($"Creating Global scene {newGlobalSceneId}");
            }
        }

        public void IsolateScene(IParcelScene sceneToActive)
        {
            foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
            {
                if (scene != sceneToActive)
                    scene.GetSceneTransform().gameObject.SetActive(false);
            }
        }

        public void ReIntegrateIsolatedScene()
        {
            foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
            {
                scene.GetSceneTransform().gameObject.SetActive(true);
            }
        }

        //======================================================================

        #endregion

        //======================================================================

        public Queue<QueuedSceneMessage_Scene> sceneMessagesPool { get; } = new Queue<QueuedSceneMessage_Scene>();

        public bool prewarmSceneMessagesPool { get; set; } = true;
        public bool prewarmEntitiesPool { get; set; } = true;

        private bool sceneSortDirty = false;
        private bool positionDirty = true;
        private int lastSortFrame = 0;

        public event Action OnSortScenes;
        public event Action<IParcelScene, string> OnOpenExternalUrlRequest;
        public event Action<IParcelScene> OnNewSceneAdded;
        public event Action<IParcelScene> OnNewPortableExperienceSceneAdded;
        public event Action<string> OnNewPortableExperienceSceneRemoved;

        private Vector2Int currentGridSceneCoordinate = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
        private Vector2Int sortAuxiliaryVector = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
    }
}