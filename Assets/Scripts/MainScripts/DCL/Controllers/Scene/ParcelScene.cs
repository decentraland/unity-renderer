using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Controllers
{
    public class ParcelScene : MonoBehaviour
    {
        public static bool VERBOSE = false;

        public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();
        public Dictionary<string, BaseDisposable> disposableComponents = new Dictionary<string, BaseDisposable>();
        public LoadParcelScenesMessage.UnityParcelScene sceneData { get; private set; }
        public SceneController ownerController;
        public SceneMetricsController metricsController;
        public UIScreenSpace uiScreenSpace;

        public event System.Action<DecentralandEntity> OnEntityAdded;
        public event System.Action<DecentralandEntity> OnEntityRemoved;

        public void Start()
        {
            metricsController = new SceneMetricsController(this);
            metricsController.Enable();
        }

        bool flaggedToUnload = false;

        [System.NonSerialized]
        public bool isTestScene = false;

        [System.NonSerialized]
        public bool isPersistent = false;

        [System.NonSerialized]
        public bool unloadWithDistance = true;

        private void Update()
        {
            SendMetricsEvent();

            if (unloadWithDistance)
            {
                if (!isTestScene && !flaggedToUnload && sceneData != null && DCLCharacterController.i != null)
                {
                    Vector3 position = GridToWorldPosition(sceneData.parcels[0].x, sceneData.parcels[0].y);

                    if (Vector3.Distance(DCLCharacterController.i.transform.position, position) >
                        ParcelSettings.UNLOAD_DISTANCE)
                    {
                        flaggedToUnload = true;
                        SceneController.i.UnloadScene(sceneData.id);
                    }
                }
            }
        }

        public void SetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            this.sceneData = data;
            this.sceneData.BakeHashes();

            this.name = gameObject.name = $"scene:{data.id}";

            gameObject.transform.position = GridToWorldPosition(data.basePosition.x, data.basePosition.y);
        }

        public void InitializeDebugPlane()
        {
            if (Environment.DEBUG && sceneData.parcels != null)
            {
                for (int j = 0; j < sceneData.parcels.Length; j++)
                {
                    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

                    Object.Destroy(plane.GetComponent<MeshCollider>());

                    plane.name = $"parcel:{sceneData.parcels[j].x},{sceneData.parcels[j].y}";

                    plane.transform.SetParent(gameObject.transform);

                    // the plane mesh with scale 1 occupies a 10 units space
                    plane.transform.localScale = new Vector3(ParcelSettings.PARCEL_SIZE * 0.1f, 1f,
                        ParcelSettings.PARCEL_SIZE * 0.1f);

                    Vector3 position = GridToWorldPosition(sceneData.parcels[j].x, sceneData.parcels[j].y);
                    // SET TO A POSITION RELATIVE TO basePosition

                    position.Set(position.x + ParcelSettings.PARCEL_SIZE / 2, ParcelSettings.DEBUG_FLOOR_HEIGHT,
                        position.z + ParcelSettings.PARCEL_SIZE / 2);

                    plane.transform.position = position;

                    if (Configuration.ParcelSettings.VISUAL_LOADING_ENABLED)
                    {
                        Material finalMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
                        var matTransition = plane.AddComponent<MaterialTransitionController>();
                        matTransition.delay = 0;
                        matTransition.useHologram = false;
                        matTransition.fadeThickness = 20;
                        matTransition.OnDidFinishLoading(finalMaterial);
                    }
                    else
                    {
                        plane.GetComponent<MeshRenderer>().sharedMaterial =
                            Utils.EnsureResourcesMaterial("Materials/Default");
                    }
                }
            }
        }

        void OnDestroy()
        {
            foreach (var entity in entities)
            {
                Destroy(entity.Value.gameObject);
            }

            entities.Clear();
        }

        public override string ToString()
        {
            return "gameObjectReference: " + this.ToString() + "\n" + sceneData.ToString();
        }

        public bool IsInsideSceneBoundaries(Vector3 worldPosition)
        {
            return IsInsideSceneBoundaries(WorldToGridPosition(worldPosition));
        }

        public virtual bool IsInsideSceneBoundaries(Vector2 gridPosition)
        {
            for (int i = 0; i < sceneData.parcels.Length; i++)
            {
                if (sceneData.parcels[i] == gridPosition)
                {
                    return true;
                }
            }

            return false;
        }

        private CreateEntityMessage tmpCreateEntityMessage = new CreateEntityMessage();

        public DecentralandEntity CreateEntity(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("CreateEntity");

            tmpCreateEntityMessage.id = json.Substring(7).TrimEnd('"', '}');

            SceneController.i.OnMessageDecodeEnds?.Invoke("CreateEntity");
            if (entities.ContainsKey(tmpCreateEntityMessage.id))
            {
                return entities[tmpCreateEntityMessage.id];
            }

            var newEntity = new DecentralandEntity();
            newEntity.entityId = tmpCreateEntityMessage.id;
            newEntity.gameObject = new GameObject("ENTITY_" + tmpCreateEntityMessage.id);
            newEntity.gameObject.transform.SetParent(gameObject.transform, false);
            newEntity.scene = this;

            entities.Add(tmpCreateEntityMessage.id, newEntity);

            OnEntityAdded?.Invoke(newEntity);

            return newEntity;
        }

        private RemoveEntityMessage tmpRemoveEntityMessage = new RemoveEntityMessage();

        public void RemoveEntity(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("RemoveEntity");
            tmpRemoveEntityMessage.FromJSON(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("RemoveEntity");

            if (entities.ContainsKey(tmpRemoveEntityMessage.id))
            {
                OnEntityRemoved?.Invoke(entities[tmpRemoveEntityMessage.id]);
                entities[tmpRemoveEntityMessage.id].OnRemoved?.Invoke(entities[tmpRemoveEntityMessage.id]);

                Object.Destroy(entities[tmpRemoveEntityMessage.id].gameObject);
                entities.Remove(tmpRemoveEntityMessage.id);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError($"Couldn't remove entity with ID: {tmpRemoveEntityMessage.id} as it doesn't exist.");
            }
#endif
        }

        public void RemoveAllEntities()
        {
            var list = entities.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                RemoveEntity(list[i].Key);
            }
        }

        private SetEntityParentMessage tmpParentMessage = new SetEntityParentMessage();

        public void SetEntityParent(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("SetEntityParent");

            tmpParentMessage.FromJSON(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("SetEntityParent");

            if (tmpParentMessage.entityId == tmpParentMessage.parentId)
            {
                return;
            }

            DecentralandEntity me = GetEntityForUpdate(tmpParentMessage.entityId);
            DecentralandEntity myParent = GetEntityForUpdate(tmpParentMessage.parentId);

            if (me != null && myParent != null)
            {
                me.gameObject.transform.SetParent(myParent.gameObject.transform, false);
            }
        }

        SharedComponentAttachMessage attachSharedComponentMessage = new SharedComponentAttachMessage();

        /**
          * This method is called when we need to attach a disposable component to the entity
          */
        public void SharedComponentAttach(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("AttachEntityComponent");
            attachSharedComponentMessage.FromJSON(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("AttachEntityComponent");

            DecentralandEntity decentralandEntity = GetEntityForUpdate(attachSharedComponentMessage.entityId);

            if (decentralandEntity == null)
            {
                return;
            }

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(attachSharedComponentMessage.id, out disposableComponent)
                && disposableComponent != null)
            {
                disposableComponent.AttachTo(decentralandEntity);
            }
        }

        UUIDCallbackMessage uuidMessage = new UUIDCallbackMessage();
        EntityComponentCreateMessage createEntityComponentMessage = new EntityComponentCreateMessage();

        public BaseComponent EntityComponentCreate(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("UpdateEntityComponent");
            createEntityComponentMessage.FromJSON(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("UpdateEntityComponent");

            DecentralandEntity entity = GetEntityForUpdate(createEntityComponentMessage.entityId);

            if (entity == null)
            {
                Debug.LogError($"scene '{sceneData.id}': Can't create entity component if the entity {createEntityComponentMessage.entityId} doesn't exist!");
                return null;
            }

            CLASS_ID_COMPONENT classId = (CLASS_ID_COMPONENT)createEntityComponentMessage.classId;

            if (classId == CLASS_ID_COMPONENT.TRANSFORM)
            {
                JsonUtility.FromJsonOverwrite(createEntityComponentMessage.json, DCLTransform.model);
                entity.gameObject.transform.localPosition = DCLTransform.model.position;
                entity.gameObject.transform.localRotation = DCLTransform.model.rotation;
                entity.gameObject.transform.localScale = DCLTransform.model.scale;
                return null;
            }

            BaseComponent newComponent = null;
            DCLComponentFactory factory = ownerController.componentFactory;
            Assert.IsNotNull(factory, "Factory is null?");

            if (!entity.components.ContainsKey(classId))
            {
                newComponent = factory.CreateItemFromId<BaseComponent>(classId);

                if (newComponent != null)
                {
                    newComponent.scene = this;
                    newComponent.entity = entity;
                    entity.components.Add(classId, newComponent);

                    newComponent.transform.SetParent(entity.gameObject.transform, false);
                    newComponent.UpdateFromJSON(createEntityComponentMessage.json);
                }
            }
            else
            {
                newComponent = EntityComponentUpdate(entity, classId, createEntityComponentMessage.json);
            }

            if (classId == CLASS_ID_COMPONENT.UUID_CALLBACK && (newComponent as UUIDComponent).model.type == "onClick")
            {
                newComponent = entity.gameObject.GetComponent<OnClickComponent>();
            }

            return newComponent;
        }

        // The EntityComponentUpdate() parameters differ from other similar methods because there is no EntityComponentUpdate protocol message yet.
        public BaseComponent EntityComponentUpdate(DecentralandEntity entity, CLASS_ID_COMPONENT classId,
            string componentJson)
        {
            if (entity == null)
            {
                Debug.LogError($"Can't update the {classId} component of a nonexistent entity!", this);
                return null;
            }

            if (!entity.components.ContainsKey(classId))
            {
                Debug.LogError($"Entity {entity.entityId} doesn't have a {classId} component to update!", this);
                return null;
            }

            BaseComponent targetComponent = entity.components[classId];
            targetComponent.UpdateFromJSON(componentJson);

            return targetComponent;
        }

        SharedComponentCreateMessage sharedComponentCreatedMessage = new SharedComponentCreateMessage();

        public BaseDisposable SharedComponentCreate(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentCreated");
            sharedComponentCreatedMessage.FromJSON(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentCreated");

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(sharedComponentCreatedMessage.id, out disposableComponent))
            {
                return disposableComponent;
            }

            BaseDisposable newComponent = null;

            switch ((CLASS_ID)sharedComponentCreatedMessage.classId)
            {
                case CLASS_ID.BOX_SHAPE:
                    {
                        newComponent = new BoxShape(this);
                        break;
                    }

                case CLASS_ID.SPHERE_SHAPE:
                    {
                        newComponent = new SphereShape(this);
                        break;
                    }

                case CLASS_ID.CONE_SHAPE:
                    {
                        newComponent = new ConeShape(this);
                        break;
                    }

                case CLASS_ID.CYLINDER_SHAPE:
                    {
                        newComponent = new CylinderShape(this);
                        break;
                    }

                case CLASS_ID.PLANE_SHAPE:
                    {
                        newComponent = new PlaneShape(this);
                        break;
                    }

                case CLASS_ID.GLTF_SHAPE:
                    {
                        newComponent = new GLTFShape(this);
                        break;
                    }

                case CLASS_ID.OBJ_SHAPE:
                    {
                        newComponent = new OBJShape(this);
                        break;
                    }

                case CLASS_ID.BASIC_MATERIAL:
                    {
                        newComponent = new BasicMaterial(this);
                        break;
                    }

                case CLASS_ID.PBR_MATERIAL:
                    {
                        newComponent = new PBRMaterial(this);
                        break;
                    }

                case CLASS_ID.AUDIO_CLIP:
                    {
                        newComponent = new DCLAudioClip(this);
                        break;
                    }

                case CLASS_ID.TEXTURE:
                    {
                        newComponent = new DCLTexture(this);
                        break;
                    }

                case CLASS_ID.UI_INPUT_TEXT_SHAPE:
                    {
                        newComponent = new UIInputText(this);
                        break;
                    }

                case CLASS_ID.UI_FULLSCREEN_SHAPE:
                case CLASS_ID.UI_SCREEN_SPACE_SHAPE:
                    {
                        if (uiScreenSpace == null)
                        {
                            newComponent = new UIScreenSpace(this);
                        }

                        break;
                    }

                case CLASS_ID.UI_CONTAINER_RECT:
                    {
                        newComponent = new UIContainerRect(this);
                        break;
                    }

                case CLASS_ID.UI_SLIDER_SHAPE:
                    {
                        newComponent = new UIScrollRect(this);
                        break;
                    }

                case CLASS_ID.UI_CONTAINER_STACK:
                    {
                        newComponent = new UIContainerStack(this);
                        break;
                    }

                case CLASS_ID.UI_IMAGE_SHAPE:
                    {
                        newComponent = new UIImage(this);
                        break;
                    }

                case CLASS_ID.UI_TEXT_SHAPE:
                    {
                        newComponent = new UIText(this);
                        break;
                    }

                default:
                    Debug.LogError($"Unknown classId {json}");
                    break;
            }

            if (newComponent != null)
            {
                newComponent.id = sharedComponentCreatedMessage.id;
                disposableComponents.Add(sharedComponentCreatedMessage.id, newComponent);
            }

            return newComponent;
        }

        SharedComponentDisposeMessage sharedComponentDisposedMessage = new SharedComponentDisposeMessage();

        public void SharedComponentDispose(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentDisposed");
            sharedComponentDisposedMessage.FromJSON(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentDisposed");

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(sharedComponentDisposedMessage.id, out disposableComponent))
            {
                if (disposableComponent != null)
                {
                    disposableComponent.Dispose();
                }

                disposableComponents.Remove(sharedComponentDisposedMessage.id);
            }
        }

        EntityComponentRemoveMessage entityComponentRemovedMessage = new EntityComponentRemoveMessage();

        public void EntityComponentRemove(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentRemoved");
            entityComponentRemovedMessage.FromJSON(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentRemoved");

            DecentralandEntity decentralandEntity = GetEntityForUpdate(entityComponentRemovedMessage.entityId);
            if (decentralandEntity == null)
            {
                return;
            }

            RemoveEntityComponent(decentralandEntity, entityComponentRemovedMessage.name);
        }

        private void RemoveComponentType<T>(DecentralandEntity entity, CLASS_ID_COMPONENT classId)
            where T : MonoBehaviour
        {
            var component = entity.components[classId].GetComponent<T>();

            if (component != null)
            {
                Utils.SafeDestroy(component);
            }
        }

        private void RemoveEntityComponent(DecentralandEntity entity, string componentName)
        {
            switch (componentName)
            {
                case "shape":
                    if (entity.currentShape != null)
                    {
                        entity.currentShape.DetachFrom(entity);
                    }

                    return;
                case "onClick":
                    RemoveComponentType<OnClickComponent>(entity, CLASS_ID_COMPONENT.UUID_CALLBACK);
                    return;
            }
        }

        SharedComponentUpdateMessage sharedComponentUpdatedMessage = new SharedComponentUpdateMessage();

        public BaseDisposable SharedComponentUpdate(string json, out Coroutine routine)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentUpdated");
            BaseDisposable result = SharedComponentUpdate(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentUpdated");

            if (result != null)
            {
                routine = result.routine;
            }
            else
            {
                routine = null;
            }

            return result;
        }

        public BaseDisposable SharedComponentUpdate(string json)
        {
            SceneController.i.OnMessageDecodeStart?.Invoke("ComponentUpdated");
            sharedComponentUpdatedMessage.FromJSON(json);
            SceneController.i.OnMessageDecodeEnds?.Invoke("ComponentUpdated");

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(sharedComponentUpdatedMessage.id, out disposableComponent) &&
                disposableComponent != null)
            {
                disposableComponent.UpdateFromJSON(sharedComponentUpdatedMessage.json);
                return disposableComponent;
            }
            else
            {
                Debug.LogError($"Unknown disposableComponent {sharedComponentUpdatedMessage.id}");
            }

            return null;
        }

        protected virtual void SendMetricsEvent()
        {
            if (Time.frameCount % 10 == 0)
                metricsController.SendEvent();
        }

        public bool HasTestSchema(string url)
        {
#if UNITY_EDITOR
            if (url.StartsWith("file://"))
            {
                return true;
            }

            if (url.StartsWith(TestHelpers.GetTestsAssetsPath()))
            {
                return true;
            }
#endif
            return false;
        }

        public virtual bool HasContentsUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

#if UNITY_EDITOR
            if (HasTestSchema(url))
            {
                return true;
            }
#endif
            if (sceneData.fileToHash == null)
            {
                return false;
            }

            return sceneData.fileToHash.ContainsKey(url.ToLower());
        }

        public string GetContentsUrl(string url)
        {
            string result = "";

            if (TryGetContentsUrl(url, out result))
            {
                return result;
            }

            return null;
        }

        public virtual bool TryGetContentsUrl(string url, out string result)
        {
            url = url.ToLower();
            result = url;

            if (HasTestSchema(url))
            {
                return true;
            }

            if (sceneData.fileToHash != null)
            {
                if (!sceneData.fileToHash.ContainsKey(url))
                {
                    Debug.LogError(string.Format("GetContentsUrl >>> File {0} not found!!!", url));
                    return false;
                }

                result = sceneData.baseUrl + sceneData.fileToHash[url];
            }
            else
            {
                result = sceneData.baseUrl + "/" + url;
            }

            if (VERBOSE)
            {
                Debug.Log($">>> GetContentsURL from ... {url} ... RESULTING URL... = {result}");
            }

            return true;
        }

        public BaseDisposable GetSharedComponent(string componentId)
        {
            BaseDisposable result;

            if (!disposableComponents.TryGetValue(componentId, out result))
            {
                return null;
            }

            return result;
        }

        private DecentralandEntity GetEntityForUpdate(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                Debug.LogError("Null or empty entityId");
                return null;
            }

            DecentralandEntity decentralandEntity;

            if (!entities.TryGetValue(entityId, out decentralandEntity))
            {
                return null;
            }

            //NOTE(Brian): This is for removing stray null references? This should never happen.
            //             Maybe move to a different 'clean-up' method to make this method have a single responsibility?.
            if (decentralandEntity == null || decentralandEntity.gameObject == null)
            {
                entities.Remove(entityId);
                return null;
            }

            return decentralandEntity;
        }

        /**
         * Transforms a grid position into a world-relative 3d position
         */
        public static Vector3 GridToWorldPosition(float xGridPosition, float yGridPosition)
        {
            return new Vector3(
                x: xGridPosition * ParcelSettings.PARCEL_SIZE,
                y: 0f,
                z: yGridPosition * ParcelSettings.PARCEL_SIZE
            );
        }

        /**
         * Transforms a world position into a grid position
         */
        public static Vector2 WorldToGridPosition(Vector3 vector)
        {
            return new Vector2(
                Mathf.Floor(vector.x / ParcelSettings.PARCEL_SIZE),
                Mathf.Floor(vector.z / ParcelSettings.PARCEL_SIZE)
            );
        }
    }
}
