using System;
using Builder.MeshLoadIndicator;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BIWFloorHandler : BIWController
{
    [Header("Design Variables")]
    public float secondsToTimeOut = 10f;

    [Header("Prefab References")]
    public ActionController actionController;

    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public DCLBuilderMeshLoadIndicatorController dclBuilderMeshLoadIndicatorController;
    public DCLBuilderMeshLoadIndicator meshLoadIndicator;
    public BuilderInWorldBridge builderInWorldBridge;
    public BIWCreatorController biwCreatorController;
    public BuilderInWorldController builderInWorldController;

    [Header("Prefabs")]
    public GameObject floorPrefab;

    public event Action OnAllParcelsFloorLoaded;
    private int numberOfParcelsLoaded;

    private CatalogItem lastFloorCalalogItemUsed;
    private readonly Dictionary<string, GameObject> floorPlaceHolderDict = new Dictionary<string, GameObject>();

    private void Start() { meshLoadIndicator.SetCamera(Camera.main); }

    private void OnDestroy() { Clean(); }

    public void Clean()
    {
        RemoveAllPlaceHolders();
        dclBuilderMeshLoadIndicatorController.Dispose();
    }

    public bool ExistsFloorPlaceHolderForEntity(string entityId) { return floorPlaceHolderDict.ContainsKey(entityId); }

    public void ChangeFloor(CatalogItem newFloorObject)
    {
        CatalogItem lastFloor = lastFloorCalalogItemUsed;
        if (lastFloor == null)
            lastFloor = FindCurrentFloorCatalogItem();

        builderInWorldEntityHandler.DeleteFloorEntities();

        CreateFloor(newFloorObject);

        BuildInWorldCompleteAction buildAction = new BuildInWorldCompleteAction();

        buildAction.CreateChangeFloorAction(lastFloor, newFloorObject);
        actionController.AddAction(buildAction);
    }

    public CatalogItem FindCurrentFloorCatalogItem()
    {
        foreach (DCLBuilderInWorldEntity entity in builderInWorldEntityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                return entity.GetCatalogItemAssociated();
            }
        }

        return null;
    }

    public bool IsCatalogItemFloor(CatalogItem floorSceneObject) { return string.Equals(floorSceneObject.category, BuilderInWorldSettings.FLOOR_CATEGORY); }

    public void CreateDefaultFloor()
    {
        CatalogItem floorSceneObject = BuilderInWorldUtils.CreateFloorSceneObject();
        CreateFloor(floorSceneObject);
    }

    public void CreateFloor(CatalogItem floorSceneObject)
    {
        Vector3 initialPosition = new Vector3(ParcelSettings.PARCEL_SIZE / 2, 0, ParcelSettings.PARCEL_SIZE / 2);
        Vector2Int[] parcelsPoints = sceneToEdit.sceneData.parcels;
        numberOfParcelsLoaded = 0;

        foreach (Vector2Int parcel in parcelsPoints)
        {
            DCLBuilderInWorldEntity decentralandEntity = biwCreatorController.CreateCatalogItem(floorSceneObject, WorldStateUtils.ConvertPointInSceneToUnityPosition(initialPosition, parcel), false, true);
            decentralandEntity.rootEntity.OnShapeUpdated += OnFloorLoaded;
            dclBuilderMeshLoadIndicatorController.ShowIndicator(decentralandEntity.rootEntity.gameObject.transform.position, decentralandEntity.rootEntity.entityId);

            GameObject floorPlaceHolder = GameObject.Instantiate(floorPrefab, decentralandEntity.rootEntity.gameObject.transform.position, Quaternion.identity);
            floorPlaceHolderDict.Add(decentralandEntity.rootEntity.entityId, floorPlaceHolder);
            decentralandEntity.OnShapeFinishLoading += OnShapeLoadFinish;
        }

        builderInWorldEntityHandler.DeselectEntities();
        lastFloorCalalogItemUsed = floorSceneObject;
    }

    private void OnShapeLoadFinish(DCLBuilderInWorldEntity entity)
    {
        entity.OnShapeFinishLoading -= OnShapeLoadFinish;
        RemovePlaceHolder(entity.rootEntity.entityId);
    }

    private void OnFloorLoaded(IDCLEntity entity)
    {
        entity.OnShapeUpdated -= OnFloorLoaded;
        RemovePlaceHolder(entity.entityId);

        numberOfParcelsLoaded++;
        if (numberOfParcelsLoaded >= sceneToEdit.sceneData.parcels.Count())
            OnAllParcelsFloorLoaded?.Invoke();
    }

    private void RemovePlaceHolder(string entityId)
    {
        if (!floorPlaceHolderDict.ContainsKey(entityId))
            return;

        GameObject floorPlaceHolder = floorPlaceHolderDict[entityId];
        floorPlaceHolderDict.Remove(entityId);
        GameObject.Destroy(floorPlaceHolder);
        dclBuilderMeshLoadIndicatorController.HideIndicator(entityId);
    }

    private void RemoveAllPlaceHolders()
    {
        foreach (GameObject gameObject in floorPlaceHolderDict.Values)
        {
            GameObject.Destroy(gameObject);
        }
        floorPlaceHolderDict.Clear();
    }

    public override void ExitEditMode()
    {
        base.ExitEditMode();

        RemoveAllPlaceHolders();
        dclBuilderMeshLoadIndicatorController.HideAllIndicators();
    }
}