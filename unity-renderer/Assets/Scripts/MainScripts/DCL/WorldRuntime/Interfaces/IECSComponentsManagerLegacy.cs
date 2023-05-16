using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Models;

namespace DCL
{
    public interface IECSComponentsManagerLegacy
    {
        event Action<string, ISharedComponent> OnAddSharedComponent;
        void CleanComponents(IDCLEntity entity);
        void AddSharedComponent(IDCLEntity entity, Type componentType, ISharedComponent component);
        void RemoveSharedComponent(IDCLEntity entity, Type targetType, bool triggerDetaching = true);
        [Obsolete("Please, do not use `TryGetComponent<T>(IDCLEntity entity)`")]
        T TryGetComponent<T>(IDCLEntity entity) where T : class;
        bool TryGetBaseComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId, out IEntityComponent component);
        bool TryGetSharedComponent(IDCLEntity entity, CLASS_ID componentId, out ISharedComponent component);
        ISharedComponent GetSharedComponent(IDCLEntity entity, Type targetType);
        bool HasComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId);
        bool HasSharedComponent(IDCLEntity entity, CLASS_ID componentId);
        void RemoveComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId);
        void AddComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId, IEntityComponent component);
        IEntityComponent GetComponent(IDCLEntity entity, CLASS_ID_COMPONENT componentId);
        IEnumerator<IEntityComponent> GetComponents(IDCLEntity entity);
        IEnumerator<ISharedComponent> GetSharedComponents(IDCLEntity entity);
        IReadOnlyDictionary<CLASS_ID_COMPONENT, IEntityComponent> GetComponentsDictionary(IDCLEntity entity);
        IReadOnlyDictionary<Type, ISharedComponent> GetSharedComponentsDictionary(IDCLEntity entity);
        int GetComponentsCount();
        bool HasSceneSharedComponent(string component);
        ISharedComponent GetSceneSharedComponent(string component);
        bool TryGetSceneSharedComponent(string component, out ISharedComponent sharedComponent);
        IReadOnlyDictionary<string, ISharedComponent> GetSceneSharedComponentsDictionary();
        int GetSceneSharedComponentsCount();
        void AddSceneSharedComponent(string component, ISharedComponent sharedComponent);
        bool RemoveSceneSharedComponent(string component);
        ISharedComponent SceneSharedComponentCreate(string id, int classId);
        T GetSceneSharedComponent<T>() where T : class;
        void SceneSharedComponentAttach(long entityId, string componentId);
        IEntityComponent EntityComponentCreateOrUpdate(long entityId, CLASS_ID_COMPONENT classId, object data);
        IEntityComponent EntityComponentUpdate(IDCLEntity entity, CLASS_ID_COMPONENT classId, object componentData);
        void SceneSharedComponentDispose(string id);
        ISharedComponent SceneSharedComponentUpdate(string id, BaseModel model);
        ISharedComponent SceneSharedComponentUpdate(string id, string json);
        ISharedComponent SceneSharedComponentUpdate(string id, Decentraland.Sdk.Ecs6.ComponentBodyPayload payload);
        void EntityComponentRemove(long entityId, string name);
        void DisposeAllSceneComponents();
    }
}
