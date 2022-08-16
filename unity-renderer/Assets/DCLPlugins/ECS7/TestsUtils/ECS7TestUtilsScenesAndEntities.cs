using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Object = UnityEngine.Object;

public class ECS7TestUtilsScenesAndEntities : IDisposable
{
    private Dictionary<string, ECS7TestScene> scenes = new Dictionary<string, ECS7TestScene>();
    private Dictionary<string, GameObject> scenesGO = new Dictionary<string, GameObject>();
    private ECSComponentsManager componentsManager;

    public ECS7TestUtilsScenesAndEntities() : this(null) { }

    public ECS7TestUtilsScenesAndEntities(ECSComponentsManager componentsManager)
    {
        this.componentsManager = componentsManager ?? new ECSComponentsManager(new Dictionary<int, ECSComponentsFactory.ECSComponentBuilder>());
    }

    public void Dispose()
    {
        ECS7TestScene[] scenes = this.scenes.Values.ToArray();
        foreach (var scene in scenes)
        {
            Internal_DeleteScene(scene);
        }
    }

    public ECS7TestScene GetScene(string sceneId)
    {
        scenes.TryGetValue(sceneId, out ECS7TestScene scene);
        return scene;
    }

    public ECS7TestScene CreateScene(string sceneId)
    {
        return CreateScene(sceneId, Vector2Int.zero, new[] { Vector2Int.zero });
    }

    public ECS7TestScene CreateScene(string sceneId, Vector2Int baseParcel, IList<Vector2Int> parcels)
    {
        ECS7TestScene scene = Internal_CreateSene(sceneId, baseParcel, parcels);
        scenes.Add(sceneId, scene);
        scenesGO.Add(sceneId, scene.GetSceneTransform().gameObject);
        return scene;
    }

    private ECS7TestScene Internal_CreateSene(string sceneId, Vector2Int baseParcel, IList<Vector2Int> parcels)
    {
        ECS7TestScene scene = new ECS7TestScene();

        GameObject go = new GameObject($"SCENE_{sceneId}");
        go.transform.position = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(baseParcel.x, baseParcel.y));

        // properties
        scene.entities = new Dictionary<long, IDCLEntity>();
        scene.sceneData = new LoadParcelScenesMessage.UnityParcelScene()
        {
            id = sceneId,
            basePosition = baseParcel,
            parcels = parcels.ToArray()
        };
        scene.crdtExecutor = new CRDTExecutor(scene, componentsManager);


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
        if (scenesGO.TryGetValue(scene.sceneData.id, out GameObject go))
        {
            Object.DestroyImmediate(go);
        }
        foreach (var entity in scene.entities.Values)
        {
            Internal_DeleteEntity(entity);
        }
        scenesGO.Remove(scene.sceneData.id);
        scenes.Remove(scene.sceneData.id);
    }

    private void Internal_DeleteEntity(IDCLEntity entity)
    {
        Object.DestroyImmediate(entity.gameObject);
    }
}
