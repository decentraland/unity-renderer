using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Models
{
    public interface IDCLEntity : ICleanable, ICleanableEventDispatcher
    {
        GameObject gameObject { get; }
        string entityId { get; set; }
        MeshesInfo meshesInfo { get; set; }
        GameObject meshRootGameObject { get; }
        Renderer[] renderers { get; }
        void SetParent(IDCLEntity entity);
        void AddChild(IDCLEntity entity);
        void RemoveChild(IDCLEntity entity);
        void EnsureMeshGameObject(string gameObjectName = null);
        void ResetRelease();
        void AddSharedComponent(System.Type componentType, ISharedComponent component);
        void RemoveSharedComponent(System.Type targetType, bool triggerDetaching = true);

        /// <summary>
        /// This function is designed to get interfaces implemented by diverse components.
        ///
        /// If you want to get the component itself please use TryGetBaseComponent or TryGetSharedComponent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T TryGetComponent<T>() where T : class;

        bool TryGetBaseComponent(CLASS_ID_COMPONENT componentId, out IEntityComponent component);
        bool TryGetSharedComponent(CLASS_ID componentId, out ISharedComponent component);
        ISharedComponent GetSharedComponent(System.Type targetType);
        IParcelScene scene { get; set; }
        bool markedForCleanup { get; set; }
        bool isInsideBoundaries { get; set; }
        Dictionary<string, IDCLEntity> children { get; }
        IDCLEntity parent { get; }
        Dictionary<CLASS_ID_COMPONENT, IEntityComponent> components { get; }
        Dictionary<System.Type, ISharedComponent> sharedComponents { get; }
        Action<IDCLEntity> OnShapeUpdated { get; set; }
        Action<IDCLEntity> OnShapeLoaded { get; set; }
        Action<IDCLEntity> OnRemoved { get; set; }
        Action<IDCLEntity> OnMeshesInfoUpdated { get; set; }
        Action<IDCLEntity> OnMeshesInfoCleaned { get; set; }

        Action<object> OnNameChange { get; set; }
        Action<object> OnTransformChange { get; set; }
    }
}