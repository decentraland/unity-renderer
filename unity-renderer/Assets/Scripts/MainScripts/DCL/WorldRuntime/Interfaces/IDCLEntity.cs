using System;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Models
{
    public interface IDCLEntity : ICleanable, ICleanableEventDispatcher
    {
        GameObject gameObject { get; }
        long entityId { get; set; }
        MeshesInfo meshesInfo { get; set; }
        GameObject meshRootGameObject { get; }
        Renderer[] renderers { get; }
        void SetParent(IDCLEntity entity);
        void AddChild(IDCLEntity entity);
        void RemoveChild(IDCLEntity entity);
        void EnsureMeshGameObject(string gameObjectName = null);
        void ResetRelease();
        IParcelScene scene { get; set; }
        bool markedForCleanup { get; set; }
        bool isInsideSceneOuterBoundaries { get; }
        bool isInsideSceneBoundaries { get; }
        Dictionary<long, IDCLEntity> children { get; }
        IDCLEntity parent { get; }
        Action<IDCLEntity> OnShapeUpdated { get; set; }
        Action<IDCLEntity> OnShapeLoaded { get; set; }
        Action<IDCLEntity> OnRemoved { get; set; }
        Action<IDCLEntity> OnMeshesInfoUpdated { get; set; }
        Action<IDCLEntity> OnMeshesInfoCleaned { get; set; }
        Action<CLASS_ID_COMPONENT, IDCLEntity> OnBaseComponentAdded { get; set; }

        Action<object> OnNameChange { get; set; }
        Action<object> OnTransformChange { get; set; }
        Action<IDCLEntity, bool> OnInsideBoundariesChanged { get; set; }
        Action<IDCLEntity, bool> OnOuterBoundariesChanged { get; set; }
        long parentId { get; set; }
        IList<long> childrenId { get; }

        void UpdateInsideBoundariesStatus(bool isInsideBoundaries);

        void UpdateOuterBoundariesStatus(bool isInsideOuterBoundaries);
    }
}
