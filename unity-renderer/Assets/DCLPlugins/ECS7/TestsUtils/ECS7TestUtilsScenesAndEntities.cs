using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class ECS7TestUtilsScenesAndEntities : IDisposable
{
    private Dictionary<int, ECS7TestScene> scenes = new Dictionary<int, ECS7TestScene>();
    private Dictionary<int, GameObject> scenesGO = new Dictionary<int, GameObject>();
    private ECSComponentsManager componentsManager;
    private Dictionary<int, ICRDTExecutor> executorsDictionary;

    public ECS7TestUtilsScenesAndEntities() : this(null, null) { }

    public ECS7TestUtilsScenesAndEntities(ECSComponentsManager componentsManager) : this(componentsManager, null) { }

    public ECS7TestUtilsScenesAndEntities(ECSComponentsManager componentsManager, Dictionary<int, ICRDTExecutor> executorsDictionary)
    {
        this.componentsManager = componentsManager ?? new ECSComponentsManager(new Dictionary<int, ECSComponentsFactory.ECSComponentBuilder>());
        this.executorsDictionary = executorsDictionary ?? new Dictionary<int, ICRDTExecutor>();
    }

    public void Dispose()
    {
        ECS7TestScene[] scenes = this.scenes.Values.ToArray();

        foreach (var scene in scenes)
        {
            Internal_DeleteScene(scene);
        }

        foreach (var crdtExecutor in executorsDictionary.Values)
        {
            crdtExecutor.Dispose();
        }

        executorsDictionary.Clear();
    }

    public ECS7TestScene GetScene(int sceneNumber)
    {
        scenes.TryGetValue(sceneNumber, out ECS7TestScene scene);
        return scene;
    }

    public ECS7TestScene CreateScene(int sceneNumber)
    {
        return CreateScene(sceneNumber, Vector2Int.zero, new[] { Vector2Int.zero });
    }

    public ECS7TestScene CreateScene(int sceneNumber, Vector2Int baseParcel, IList<Vector2Int> parcels)
    {
        ECS7TestScene scene = Internal_CreateSene(sceneNumber, baseParcel, parcels);
        scenes.Add(sceneNumber, scene);
        scenesGO.Add(sceneNumber, scene.GetSceneTransform().gameObject);
        return scene;
    }

    private ECS7TestScene Internal_CreateSene(int sceneNumber, Vector2Int baseParcel, IList<Vector2Int> parcels)
    {
        ECS7TestScene scene = new ECS7TestScene();

        GameObject go = new GameObject($"SCENE_NUMBER_{sceneNumber}");
        go.transform.position = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(baseParcel.x, baseParcel.y));

        // properties
        scene.entities = new Dictionary<long, IDCLEntity>();

        scene.sceneData = new LoadParcelScenesMessage.UnityParcelScene()
        {
            sceneNumber = sceneNumber,
            basePosition = baseParcel,
            parcels = parcels.ToArray(),
            sdk7 = true
        };

        // methods: `CreateEntity` `RemoveEntity` `GetSceneTransform`
        scene._entityCreator = (id) =>
        {
            ECS7TestEntity entity = Internal_CreateEntity(id);
            entity.gameObject.transform.SetParent(scene.GetSceneTransform(), false);
            scene.entities.Add(entity.entityId, entity);
            return entity;
        };

        scene._entityRemover = (id) =>
        {
            if (scene.entities.TryGetValue(id, out IDCLEntity entity))
            {
                scene.entities.Remove(id);
                ((ECS7TestEntity)entity)._triggerRemove();
                Internal_DeleteEntity(entity);
            }
        };

        scene._go = go;

        executorsDictionary[sceneNumber] = new CRDTExecutor(scene, componentsManager);

        return scene;
    }

    private ECS7TestEntity Internal_CreateEntity(long entityId)
    {
        ECS7TestEntity entity = new ECS7TestEntity();
        GameObject go = new GameObject($"ENTITY_{entityId}");
        go.transform.ResetLocalTRS();

        // properties
        entity.entityId = entityId;
        entity.childrenId = new List<long>();
        entity.gameObject = go;
        entity.parentId = 0;

        return entity;
    }

    private void Internal_DeleteScene(IParcelScene scene)
    {
        if (scenesGO.TryGetValue(scene.sceneData.sceneNumber, out GameObject go))
        {
            Object.DestroyImmediate(go);
        }

        foreach (var entity in scene.entities.Values)
        {
            Internal_DeleteEntity(entity);
        }

        scenesGO.Remove(scene.sceneData.sceneNumber);
        scenes.Remove(scene.sceneData.sceneNumber);
    }

    private void Internal_DeleteEntity(IDCLEntity entity)
    {
        Object.DestroyImmediate(entity.gameObject);
    }
}
