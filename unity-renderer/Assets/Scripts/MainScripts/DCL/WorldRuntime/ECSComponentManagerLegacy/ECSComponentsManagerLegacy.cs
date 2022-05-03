using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using DCL.Rendering;
using UnityEngine;

namespace DCL
{
    public class ECSComponentsManagerLegacy : IECSComponentsManagerLegacy
    {
        private readonly Dictionary<string, ISharedComponent> disposableComponents = new Dictionary<string, ISharedComponent>();

        private readonly Dictionary<long, Dictionary<Type, ISharedComponent>> entitiesSharedComponents =
            new Dictionary<long, Dictionary<Type, ISharedComponent>>();

        private readonly Dictionary<long, Dictionary<CLASS_ID_COMPONENT, IEntityComponent>> entitiesComponents =
            new Dictionary<long, Dictionary<CLASS_ID_COMPONENT, IEntityComponent>>();

        private readonly IParcelScene scene;
        private readonly IRuntimeComponentFactory componentFactory;
        private readonly IParcelScenesCleaner parcelScenesCleaner;
        private readonly ISceneBoundsChecker sceneBoundsChecker;
        private readonly IPhysicsSyncController physicsSyncController;
        private readonly ICullingController cullingController;

        public event Action<string, ISharedComponent> OnAddSharedComponent;

        public ECSComponentsManagerLegacy(IParcelScene scene)
            : this(scene,
                Environment.i.world.componentFactory,
                Environment.i.platform.parcelScenesCleaner,
                Environment.i.world.sceneBoundsChecker,
                Environment.i.platform.physicsSyncController,
                Environment.i.platform.cullingController) { }

        public ECSComponentsManagerLegacy(IParcelScene scene,
            IRuntimeComponentFactory componentFactory,
            IParcelScenesCleaner parcelScenesCleaner,
            ISceneBoundsChecker sceneBoundsChecker,
            IPhysicsSyncController physicsSyncController,
            ICullingController cullingController)
        {
            this.scene = scene;
            this.componentFactory = componentFactory;
            this.parcelScenesCleaner = parcelScenesCleaner;
            this.sceneBoundsChecker = sceneBoundsChecker;
            this.physicsSyncController = physicsSyncController;
            this.cullingController = cullingController;
        }

        public void AddSharedComponent(IDCLEntity entity, Type componentType, ISharedComponent component)
        {
            if (component == null)
            {
                return;
            }

            RemoveSharedComponent(entity, componentType);

            if (!entitiesSharedComponents.TryGetValue(entity.entityId, out Dictionary<Type, ISharedComponent> entityComponents))
            {
                entityComponents = new Dictionary<Type, ISharedComponent>();
                entitiesSharedComponents.Add(entity.entityId, entityComponents);
            }

            entityComponents.Add(componentType, component);
        }

        public void RemoveSharedComponent(IDCLEntity entity, Type targetType, bool triggerDetaching = true)
        {
            if (!entitiesSharedComponents.TryGetValue(entity.entityId, out Dictionary<Type, ISharedComponent> entityComponents))
            {
                return;
            }
            if (!entityComponents.TryGetValue(targetType, out ISharedComponent component))
            {
                return;
            }
            if (component == null)
                return;

            entityComponents.Remove(targetType);

            if (entityComponents.Count == 0)
            {
                entitiesSharedComponents.Remove(entity.entityId);
            }

            if (triggerDetaching)
                component.DetachFrom(entity, targetType);
        }

        public T TryGetComponent<T>(IDCLEntity entity) where T : class
        {
            T component = null;
            if (entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents))
            {
                component = entityComponents.Values.FirstOrDefault(x => x is T) as T;

                if (component != null)
                    return component;
            }

            if (entitiesSharedComponents.TryGetValue(entity.entityId, out Dictionary<Type, ISharedComponent> entitySharedComponents))
            {
                component = entitySharedComponents.Values.FirstOrDefault(x => x is T) as T;

                if (component != null)
                    return component;
            }

            return null;
        }

        public bool TryGetBaseComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId, out IEntityComponent component)
        {
            if (entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents))
            {
                return entityComponents.TryGetValue(componentId, out component);
            }
            component = null;
            return false;
        }

        public bool TryGetSharedComponent(IDCLEntity entity, CLASS_ID componentId, out ISharedComponent component)
        {
            component = null;
            if (!entitiesSharedComponents.TryGetValue(entity.entityId, out Dictionary<Type, ISharedComponent> entityComponents))
            {
                return false;
            }

            using (var iterator = entityComponents.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.GetClassId() != (int)componentId)
                        continue;

                    component = iterator.Current.Value;
                    break;
                }
            }

            return component != null;
        }

        public ISharedComponent GetSharedComponent(IDCLEntity entity, Type targetType)
        {
            if (!entitiesSharedComponents.TryGetValue(entity.entityId, out Dictionary<Type, ISharedComponent> entityComponents))
            {
                return null;
            }

            if (entityComponents.TryGetValue(targetType, out ISharedComponent component))
            {
                return component;
            }

            return null;
        }

        public bool HasComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId)
        {
            if (entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents))
            {
                return entityComponents.ContainsKey(componentId);
            }
            return false;
        }

        public bool HasSharedComponent(IDCLEntity entity, CLASS_ID componentId)
        {
            if (TryGetSharedComponent(entity, componentId, out ISharedComponent component))
            {
                return component != null;
            }
            return false;
        }

        public void RemoveComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId)
        {
            if (entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents))
            {
                entityComponents.Remove(componentId);

                if (entityComponents.Count == 0)
                {
                    entitiesComponents.Remove(entity.entityId);
                }
            }
        }

        public void AddComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId, IEntityComponent component)
        {
            if (!entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents))
            {
                entityComponents = new Dictionary<CLASS_ID_COMPONENT, IEntityComponent>();
                entitiesComponents.Add(entity.entityId, entityComponents);
            }
            entityComponents.Add(componentId, component);
        }

        public IEntityComponent GetComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId)
        {
            if (!entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents))
            {
                return null;
            }
            if (entityComponents.TryGetValue(componentId, out IEntityComponent component))
            {
                return component;
            }
            return null;
        }

        public IEnumerator<IEntityComponent> GetComponents(IDCLEntity entity)
        {
            if (!entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents))
            {
                yield break;
            }

            using (var iterator = entityComponents.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    yield return iterator.Current.Value;
                }
            }
        }

        public IEnumerator<ISharedComponent> GetSharedComponents(IDCLEntity entity)
        {
            if (!entitiesSharedComponents.TryGetValue(entity.entityId, out Dictionary<Type, ISharedComponent> entityComponents))
            {
                yield break;
            }

            using (var iterator = entityComponents.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    yield return iterator.Current.Value;
                }
            }
        }

        public IReadOnlyDictionary<CLASS_ID_COMPONENT, IEntityComponent> GetComponentsDictionary(IDCLEntity entity)
        {
            entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents);
            return entityComponents;
        }

        public IReadOnlyDictionary<Type, ISharedComponent> GetSharedComponentsDictionary(IDCLEntity entity)
        {
            entitiesSharedComponents.TryGetValue(entity.entityId, out Dictionary<Type, ISharedComponent> entityComponents);
            return entityComponents;
        }

        public int GetComponentsCount()
        {
            int count = 0;
            using (var entityIterator = entitiesComponents.GetEnumerator())
            {
                while (entityIterator.MoveNext())
                {
                    count += entityIterator.Current.Value.Count;
                }
            }
            return count;
        }

        public void CleanComponents(IDCLEntity entity)
        {
            if (!entitiesComponents.TryGetValue(entity.entityId, out Dictionary<CLASS_ID_COMPONENT, IEntityComponent> entityComponents))
            {
                return;
            }

            using (var iterator = entityComponents.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value == null)
                        continue;

                    if (iterator.Current.Value is ICleanable cleanableComponent)
                        cleanableComponent.Cleanup();

                    if (!(iterator.Current.Value is IPoolableObjectContainer poolableContainer))
                        continue;

                    if (poolableContainer.poolableObject == null)
                        continue;

                    poolableContainer.poolableObject.Release();
                }
            }
            entitiesComponents.Remove(entity.entityId);
        }

        public bool HasSceneSharedComponent(string component)
        {
            return disposableComponents.ContainsKey(component);
        }

        public ISharedComponent GetSceneSharedComponent(string component)
        {
            disposableComponents.TryGetValue(component, out ISharedComponent sharedComponent);
            return sharedComponent;
        }

        public bool TryGetSceneSharedComponent(string component, out ISharedComponent sharedComponent)
        {
            return disposableComponents.TryGetValue(component, out sharedComponent);
        }

        public IReadOnlyDictionary<string, ISharedComponent> GetSceneSharedComponentsDictionary()
        {
            return disposableComponents;
        }

        public int GetSceneSharedComponentsCount()
        {
            return disposableComponents.Count;
        }

        public void AddSceneSharedComponent(string component, ISharedComponent sharedComponent)
        {
            disposableComponents.Add(component, sharedComponent);
            OnAddSharedComponent?.Invoke(component, sharedComponent);
        }

        public bool RemoveSceneSharedComponent(string component)
        {
            return disposableComponents.Remove(component);
        }

        public ISharedComponent SceneSharedComponentCreate(string id, int classId)
        {
            if (disposableComponents.TryGetValue(id, out ISharedComponent component))
                return component;

            IRuntimeComponentFactory factory = componentFactory;
            
            if (factory.createConditions.ContainsKey(classId))
            {
                if (!factory.createConditions[(int) classId].Invoke(scene.sceneData.id, classId))
                    return null;
            }

            ISharedComponent newComponent = componentFactory.CreateComponent(classId) as ISharedComponent;

            if (newComponent == null)
                return null;

            AddSceneSharedComponent(id, newComponent);

            newComponent.Initialize(scene, id);

            return newComponent;
        }

        public T GetSceneSharedComponent<T>() where T : class
        {
            return disposableComponents.Values.FirstOrDefault(x => x is T) as T;
        }

        /**
          * This method is called when we need to attach a disposable component to the entity
          */
        public void SceneSharedComponentAttach(long entityId, string componentId)
        {
            IDCLEntity entity = scene.GetEntityById(entityId);

            if (entity == null)
                return;

            if (disposableComponents.TryGetValue(componentId, out ISharedComponent sharedComponent))
            {
                sharedComponent.AttachTo(entity);
            }
        }

        public IEntityComponent EntityComponentCreateOrUpdate(long entityId, CLASS_ID_COMPONENT classId, object data)
        {
            IDCLEntity entity = scene.GetEntityById(entityId);

            if (entity == null)
            {
                Debug.LogError($"scene '{scene.sceneData.id}': Can't create entity component if the entity {entityId} doesn't exist!");
                return null;
            }

            IEntityComponent newComponent = null;

            var overrideCreate = Environment.i.world.componentFactory.createOverrides;
            
            if (overrideCreate.ContainsKey((int) classId))
            {
                int classIdAsInt = (int) classId;
                overrideCreate[(int) classId].Invoke(scene.sceneData.id, entityId, ref classIdAsInt, data);
                classId = (CLASS_ID_COMPONENT) classIdAsInt;
            }

            if (!HasComponent(entity, classId))
            {
                newComponent = componentFactory.CreateComponent((int) classId) as IEntityComponent;

                if (newComponent != null)
                {
                    AddComponent(entity, classId, newComponent);

                    newComponent.Initialize(scene, entity);

                    if (data is string json)
                    {
                        newComponent.UpdateFromJSON(json);
                    }
                    else
                    {
                        newComponent.UpdateFromModel(data as BaseModel);
                    }
                }
            }
            else
            {
                newComponent = EntityComponentUpdate(entity, classId, data as string);
            }

            if (newComponent != null && newComponent is IOutOfSceneBoundariesHandler)
                sceneBoundsChecker?.AddEntityToBeChecked(entity);

            physicsSyncController.MarkDirty();
            cullingController.MarkDirty();

            return newComponent;
        }

        public IEntityComponent EntityComponentUpdate(IDCLEntity entity, CLASS_ID_COMPONENT classId,
            string componentJson)
        {
            if (entity == null)
            {
                Debug.LogError($"Can't update the {classId} component of a nonexistent entity!", scene.GetSceneTransform());
                return null;
            }

            if (!HasComponent(entity, classId))
            {
                Debug.LogError($"Entity {entity.entityId} doesn't have a {classId} component to update!", scene.GetSceneTransform());
                return null;
            }

            var targetComponent = GetComponent(entity, classId);
            targetComponent.UpdateFromJSON(componentJson);

            return targetComponent;
        }

        public void SceneSharedComponentDispose(string id)
        {
            if (disposableComponents.TryGetValue(id, out ISharedComponent sharedComponent))
            {
                sharedComponent?.Dispose();
                disposableComponents.Remove(id);
            }
        }

        public ISharedComponent SceneSharedComponentUpdate(string id, BaseModel model)
        {
            if (disposableComponents.TryGetValue(id, out ISharedComponent sharedComponent))
            {
                sharedComponent.UpdateFromModel(model);
                return sharedComponent;
            }

            if (scene.GetSceneTransform() == null)
            {
                Debug.LogError($"Unknown disposableComponent {id} -- scene has been destroyed?");
            }
            else
            {
                Debug.LogError($"Unknown disposableComponent {id}", scene.GetSceneTransform());
            }

            return null;
        }

        public ISharedComponent SceneSharedComponentUpdate(string id, string json)
        {
            if (disposableComponents.TryGetValue(id, out ISharedComponent disposableComponent))
            {
                disposableComponent.UpdateFromJSON(json);
                return disposableComponent;
            }

            if (scene.GetSceneTransform() == null)
            {
                Debug.LogError($"Unknown disposableComponent {id} -- scene has been destroyed?");
            }
            else
            {
                Debug.LogError($"Unknown disposableComponent {id}", scene.GetSceneTransform());
            }

            return null;
        }

        public void EntityComponentRemove(long entityId, string componentName)
        {
            IDCLEntity entity = scene.GetEntityById(entityId);

            if (entity == null)
            {
                return;
            }

            switch (componentName)
            {
                case "shape":
                    if (entity.meshesInfo.currentShape is BaseShape baseShape)
                    {
                        baseShape.DetachFrom(entity);
                    }

                    return;

                case ComponentNameLiterals.OnClick:
                    {
                        if (TryGetBaseComponent(entity, CLASS_ID_COMPONENT.UUID_ON_CLICK, out IEntityComponent component))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            RemoveComponent(entity, CLASS_ID_COMPONENT.UUID_ON_CLICK);
                        }

                        return;
                    }
                case ComponentNameLiterals.OnPointerDown:
                    {
                        if (TryGetBaseComponent(entity, CLASS_ID_COMPONENT.UUID_ON_DOWN, out IEntityComponent component))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            RemoveComponent(entity, CLASS_ID_COMPONENT.UUID_ON_DOWN);
                        }
                    }
                    return;
                case ComponentNameLiterals.OnPointerUp:
                    {
                        if (TryGetBaseComponent(entity, CLASS_ID_COMPONENT.UUID_ON_UP, out IEntityComponent component))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            RemoveComponent(entity, CLASS_ID_COMPONENT.UUID_ON_UP);
                        }
                    }
                    return;
                case ComponentNameLiterals.OnPointerHoverEnter:
                    {
                        if (TryGetBaseComponent(entity, CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER, out IEntityComponent component))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            RemoveComponent(entity, CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER);
                        }
                    }
                    return;
                case ComponentNameLiterals.OnPointerHoverExit:
                    {
                        if (TryGetBaseComponent(entity, CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT, out IEntityComponent component))
                        {
                            Utils.SafeDestroy(component.GetTransform().gameObject);
                            RemoveComponent(entity, CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT);
                        }
                    }
                    return;
                case "transform":
                    {
                        if (TryGetBaseComponent(entity, CLASS_ID_COMPONENT.AVATAR_ATTACH, out IEntityComponent component))
                        {
                            component.Cleanup();
                            RemoveComponent(entity, CLASS_ID_COMPONENT.AVATAR_ATTACH);
                        }
                    }
                    return;
            }
        }

        public void DisposeAllSceneComponents()
        {
            List<string> allDisposableComponents = disposableComponents.Select(x => x.Key).ToList();
            foreach (string id in allDisposableComponents)
            {
                parcelScenesCleaner.MarkDisposableComponentForCleanup(scene, id);
            }
        }
    }
}