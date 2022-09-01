using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

/*
 * we are creating this class for test instead of just using Substitute.For<IDCLEntity>
 * so we can use it as measurement of ECS7 dependencies on `IDCLEntity`
 */
public class ECS7TestEntity : IDCLEntity
{
    public GameObject gameObject { get; internal set; }
    public long entityId { get; set; }
    public long parentId { get; set; }
    public IList<long> childrenId { get; internal set; }
    public Action<IDCLEntity> OnRemoved { get; set; }

    internal ECS7TestEntity() { }

    internal void _triggerRemove()
    {
        OnRemoved?.Invoke(this);
    }

// FOLLOWING NOT SUPPORTED
    void ICleanable.Cleanup()
    {
        throw new NotImplementedException();
    }
    Action<ICleanableEventDispatcher> ICleanableEventDispatcher.OnCleanupEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    MeshesInfo IDCLEntity.meshesInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    GameObject IDCLEntity.meshRootGameObject => throw new NotImplementedException();
    Renderer[] IDCLEntity.renderers => throw new NotImplementedException();
    void IDCLEntity.SetParent(IDCLEntity entity)
    {
        throw new NotImplementedException();
    }
    void IDCLEntity.AddChild(IDCLEntity entity)
    {
        throw new NotImplementedException();
    }
    void IDCLEntity.RemoveChild(IDCLEntity entity)
    {
        throw new NotImplementedException();
    }
    void IDCLEntity.EnsureMeshGameObject(string gameObjectName)
    {
        throw new NotImplementedException();
    }
    void IDCLEntity.ResetRelease()
    {
        throw new NotImplementedException();
    }
    IParcelScene IDCLEntity.scene { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    bool IDCLEntity.markedForCleanup { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    bool IDCLEntity.isInsideSceneOuterBoundaries { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    bool IDCLEntity.isInsideSceneBoundaries { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    Dictionary<long, IDCLEntity> IDCLEntity.children => throw new NotImplementedException();
    IDCLEntity IDCLEntity.parent => throw new NotImplementedException();
    Action<IDCLEntity> IDCLEntity.OnShapeUpdated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    Action<IDCLEntity> IDCLEntity.OnShapeLoaded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    Action<IDCLEntity> IDCLEntity.OnMeshesInfoUpdated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    Action<IDCLEntity> IDCLEntity.OnMeshesInfoCleaned { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    Action<CLASS_ID_COMPONENT, IDCLEntity> IDCLEntity.OnBaseComponentAdded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    Action<object> IDCLEntity.OnNameChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    Action<object> IDCLEntity.OnTransformChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}