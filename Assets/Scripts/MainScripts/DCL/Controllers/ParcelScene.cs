using DCL.Components;
using DCL.Configuration;
using DCL.Models;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Controllers
{
    public class ParcelScene : MonoBehaviour
    {
        public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();
        public Dictionary<string, BaseDisposable> disposableComponents = new Dictionary<string, BaseDisposable>();

        public LoadParcelScenesMessage.UnityParcelScene sceneData { get; private set; }
        public SceneController ownerController;

        public void SetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            this.sceneData = data;

            this.name = gameObject.name = $"scene:{data.id}";

            gameObject.transform.position = Helpers.Utils.GridToWorldPosition(data.basePosition.x, data.basePosition.y);

            if (Environment.DEBUG)
            {
                for (int j = 0; j < data.parcels.Length; j++)
                {
                    var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

                    Object.Destroy(plane.GetComponent<MeshCollider>());

                    plane.name = $"parcel:{data.parcels[j].x},{data.parcels[j].y}";
                    plane.transform.SetParent(gameObject.transform);

                    var position = Helpers.Utils.GridToWorldPosition(data.parcels[j].x, data.parcels[j].y);
                    // SET TO A POSITION RELATIVE TO basePosition

                    position.Set(position.x + ParcelSettings.PARCEL_SIZE / 2, ParcelSettings.DEBUG_FLOOR_HEIGHT, position.z + ParcelSettings.PARCEL_SIZE / 2);

                    plane.transform.position = position;
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

        private CreateEntityMessage tmpCreateEntityMessage = new CreateEntityMessage();

        public void CreateEntity(string json)
        {
            tmpCreateEntityMessage.FromJSON(json);
            if (!entities.ContainsKey(tmpCreateEntityMessage.id))
            {
                var newEntity = new DecentralandEntity();
                newEntity.entityId = tmpCreateEntityMessage.id;
                newEntity.gameObject = new GameObject();
                newEntity.gameObject.transform.SetParent(gameObject.transform);
                newEntity.gameObject.name = "ENTITY_" + tmpCreateEntityMessage.id;

                entities.Add(tmpCreateEntityMessage.id, newEntity);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                throw new UnityException($"Couldn't create entity with ID: {tmpCreateEntityMessage.id} as it already exists.");
            }
#endif
        }

        private RemoveEntityMessage tmpRemoveEntityMessage = new RemoveEntityMessage();

        public void RemoveEntity(string json)
        {
            tmpRemoveEntityMessage.FromJSON(json);

            if (entities.ContainsKey(tmpRemoveEntityMessage.id))
            {
                Object.Destroy(entities[tmpRemoveEntityMessage.id].gameObject);
                entities.Remove(tmpRemoveEntityMessage.id);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                throw new UnityException($"Couldn't remove entity with ID: {tmpRemoveEntityMessage.id} as it doesn't exist.");
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
            tmpParentMessage.FromJSON(json);

            if (tmpParentMessage.entityId != tmpParentMessage.parentId)
            {
                GameObject rootGameObject = null;

                if (tmpParentMessage.parentId == "0")
                {
                    rootGameObject = gameObject;
                }
                else
                {
                    DecentralandEntity decentralandEntity = GetEntityForUpdate(tmpParentMessage.parentId);
                    if (decentralandEntity != null)
                    {
                        rootGameObject = decentralandEntity.gameObject;
                    }
                }

                if (rootGameObject != null)
                {
                    DecentralandEntity decentralandEntity = GetEntityForUpdate(tmpParentMessage.entityId);

                    if (decentralandEntity != null)
                    {
                        decentralandEntity.gameObject.transform.SetParent(rootGameObject.transform);
                    }
                }
            }
        }

        SharedComponentAttachMessage attachSharedComponentMessage = new SharedComponentAttachMessage();

        /**
          * This method is called when we need to attach a disposable component to the entity
          */
        public void SharedComponentAttach(string json)
        {
            attachSharedComponentMessage.FromJSON(json);

            DecentralandEntity decentralandEntity = GetEntityForUpdate(attachSharedComponentMessage.entityId);

            if (decentralandEntity == null)
            {
                return;
            }

            BaseDisposable disposableComponent;

            RemoveEntityComponent(decentralandEntity, attachSharedComponentMessage.name);

            if (disposableComponents.TryGetValue(attachSharedComponentMessage.id, out disposableComponent) && disposableComponent != null)
            {
                disposableComponent.AttachTo(decentralandEntity);
            }
        }

        UUIDCallbackMessage uuidMessage = new UUIDCallbackMessage();
        EntityComponentCreateMessage createEntityComponentMessage = new EntityComponentCreateMessage();

        public T CreateAndInitComponent<T>(DecentralandEntity entity, EntityComponentCreateMessage message) where T : BaseComponent
        {
            var component = Utils.GetOrCreateComponent<T>(entity.gameObject);
            component.scene = this;
            component.entity = entity;
            component.UpdateFromJSON(createEntityComponentMessage.json);
            return component;
        }

        public BaseComponent EntityComponentCreate(string json)
        {
            createEntityComponentMessage.FromJSON(json);

            DecentralandEntity entity = GetEntityForUpdate(createEntityComponentMessage.entityId);
            if (entity == null)
                return null;

            DCLComponentFactory factory = ownerController.componentFactory;
            CLASS_ID_COMPONENT classId = (CLASS_ID_COMPONENT)createEntityComponentMessage.classId;
            BaseComponent newComponent = null;

            if (!entity.components.ContainsKey(classId))
            {
                newComponent = factory.CreateItemFromId<BaseComponent>(classId);

                if (newComponent != null)
                {
                    newComponent.scene = this;
                    newComponent.entity = entity;
                    entity.components.Add(classId, newComponent);
                    newComponent.gameObject.transform.parent = entity.gameObject.transform;
                    newComponent.UpdateFromJSON(createEntityComponentMessage.json);
                }
            }
            else
            {
                newComponent = entity.components[classId];
                newComponent.UpdateFromJSON(createEntityComponentMessage.json);
            }

            return newComponent;
        }

        SharedComponentCreateMessage sharedComponentCreatedMessage = new SharedComponentCreateMessage();

        public BaseDisposable SharedComponentCreate(string json)
        {
            sharedComponentCreatedMessage.FromJSON(json);

            BaseDisposable disposableComponent;
            if (disposableComponents.TryGetValue(sharedComponentCreatedMessage.id, out disposableComponent))
            {
                if (disposableComponent != null)
                {
                    disposableComponent.Dispose();
                }
                disposableComponents.Remove(sharedComponentCreatedMessage.id);
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
                default:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    throw new UnityException($"Unknown classId {json}");
#else
                    // Ignore errors outside of the editor
                    break;
#endif
            }

            if ( newComponent != null )
                disposableComponents.Add(sharedComponentCreatedMessage.id, newComponent);

            return newComponent;
        }

        SharedComponentDisposeMessage sharedComponentDisposedMessage = new SharedComponentDisposeMessage();
        public void SharedComponentDispose(string json)
        {
            sharedComponentDisposedMessage.FromJSON(json);

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
            entityComponentRemovedMessage.FromJSON(json);

            DecentralandEntity decentralandEntity = GetEntityForUpdate(entityComponentRemovedMessage.entityId);
            if (decentralandEntity == null) return;

            RemoveEntityComponent(decentralandEntity, entityComponentRemovedMessage.name);
        }

        private void RemoveComponentType<T>(DecentralandEntity entity, CLASS_ID_COMPONENT classId) where T : MonoBehaviour
        {
            var component = entity.components[classId].GetComponent<T>();

            if (component != null)
            {
#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(component);
#else
                UnityEngine.Object.Destroy(component);
#endif
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
                case "transform":
                    RemoveComponentType<DCLTransform>(entity, CLASS_ID_COMPONENT.TRANSFORM);
                    return;
            }
        }

        SharedComponentUpdateMessage sharedComponentUpdatedMessage = new SharedComponentUpdateMessage();

        public BaseDisposable SharedComponentUpdate(string json)
        {
            sharedComponentUpdatedMessage.FromJSON(json);

            BaseDisposable disposableComponent;

            if (disposableComponents.TryGetValue(sharedComponentUpdatedMessage.id, out disposableComponent) && disposableComponent != null)
            {
                disposableComponent.UpdateFromJSON(sharedComponentUpdatedMessage.json);
                return disposableComponent;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                throw new UnityException($"Unknown disposableComponent {sharedComponentUpdatedMessage.id}");
            }
#else
            return null;
#endif
        }


        public BaseDisposable GetSharedComponent(string componentId)
        {
            BaseDisposable result;

            if (!disposableComponents.TryGetValue(componentId, out result))
                return null;

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
    }
}
