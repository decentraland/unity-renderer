using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.Models;
using UnityEngine;

/*
 * we are creating this class for test instead of just using Substitute.For<IParcelScene>
 * so we can use it as measurement of ECS7 dependencies on `IParcelScene`
 */
public class ECS7TestScene : IParcelScene
{
    public LoadParcelScenesMessage.UnityParcelScene sceneData { get; internal set; }
    public ICRDTExecutor crdtExecutor { get; set; }
    public Dictionary<long, IDCLEntity> entities { get; internal set; }
    public ECS7TestEntity CreateEntity(long id) => _entityCreator(id);
    public void RemoveEntity(long id, bool removeImmediatelyFromEntitiesList = true) => _entityRemover(id);
    public Transform GetSceneTransform() => _go.transform;
    public ContentProvider contentProvider { get; } = new ContentProvider();

// INTERNAL CONFIG FOR MOCKING    
    internal GameObject _go;
    internal Func<long, ECS7TestEntity> _entityCreator;
    internal Action<long> _entityRemover;
    internal ECS7TestScene() { }

// FOLLOWING NOT SUPPORTED
    event Action<float> IParcelScene.OnLoadingStateUpdated { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
    event Action<IDCLEntity> IParcelScene.OnEntityAdded { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
    event Action<IDCLEntity> IParcelScene.OnEntityRemoved { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
    IDCLEntity IParcelScene.GetEntityById(long entityId)
    {
        throw new NotImplementedException();
    }
    IDCLEntity IParcelScene.CreateEntity(long id)
    {
        return CreateEntity(id);
    }
    IECSComponentsManagerLegacy IParcelScene.componentsManagerLegacy => throw new NotImplementedException();
    bool IParcelScene.isPersistent => throw new NotImplementedException();
    bool IParcelScene.isTestScene => throw new NotImplementedException();
    float IParcelScene.loadingProgress => throw new NotImplementedException();
    string IParcelScene.GetSceneName()
    {
        throw new NotImplementedException();
    }
    ISceneMetricsCounter IParcelScene.metricsCounter => throw new NotImplementedException();
    public HashSet<Vector2Int> GetParcels() { throw new NotImplementedException(); }
    bool IParcelScene.IsInsideSceneBoundaries(Bounds objectBounds)
    {
        throw new NotImplementedException();
    }
    bool IParcelScene.IsInsideSceneBoundaries(Vector2Int gridPosition, float height)
    {
        throw new NotImplementedException();
    }
    bool IParcelScene.IsInsideSceneBoundaries(Vector3 worldPosition, float height)
    {
        throw new NotImplementedException();
    }
    bool IParcelScene.IsInsideSceneOuterBoundaries(Bounds objectBounds)
    {
        throw new NotImplementedException();
    }
    bool IParcelScene.IsInsideSceneOuterBoundaries(Vector3 objectUnityPosition)
    {
        throw new NotImplementedException();
    }
    void IParcelScene.CalculateSceneLoadingState()
    {
        throw new NotImplementedException();
    }
    void IParcelScene.GetWaitingComponentsDebugInfo()
    {
        throw new NotImplementedException();
    }
    void IParcelScene.SetEntityParent(long entityId, long parentId)
    {
        throw new NotImplementedException();
    }
}