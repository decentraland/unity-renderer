using System;
using Builder.MeshLoadIndicator;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWFloorHandler : BIWController
{
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

    private CatalogItem lastFloorCalalogItemUsed;
    private readonly Dictionary<string, GameObject> floorPlaceHolderDict = new Dictionary<string, GameObject>();

    private void Start()
    {
        meshLoadIndicator.SetCamera(Camera.main);
    }

    private void OnDestroy()
    {
        Clean();
    }

    public void Clean()
    {
        foreach (GameObject gameObject in floorPlaceHolderDict.Values)
        {
            GameObject.Destroy(gameObject);
        }
        floorPlaceHolderDict.Clear();
    }

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

    public bool IsCatalogItemFloor(CatalogItem floorSceneObject)
    {
        return string.Equals(floorSceneObject.category, BuilderInWorldSettings.FLOOR_CATEGORY);
    }

    public void CreateDefaultFloor()
    {
        CatalogItem floorSceneObject = BuilderInWorldUtils.CreateFloorSceneObject();
        CreateFloor(floorSceneObject);
    }

    public void CreateFloor(CatalogItem floorSceneObject)
    {
        Vector3 initialPosition = new Vector3(ParcelSettings.PARCEL_SIZE / 2, 0, ParcelSettings.PARCEL_SIZE / 2);
        Vector2Int[] parcelsPoints = sceneToEdit.sceneData.parcels;

        foreach (Vector2Int parcel in parcelsPoints)
        {
            DCLBuilderInWorldEntity decentralandEntity = biwCreatorController.CreateSceneObject(floorSceneObject, false, true);
            decentralandEntity.rootEntity.OnShapeUpdated += OnFloorLoaded;
            decentralandEntity.transform.position = WorldStateUtils.ConvertPointInSceneToUnityPosition(initialPosition, parcel);
            dclBuilderMeshLoadIndicatorController.ShowIndicator(decentralandEntity.rootEntity.gameObject.transform.position, decentralandEntity.rootEntity.entityId);

            GameObject floorPlaceHolder = GameObject.Instantiate(floorPrefab, decentralandEntity.rootEntity.gameObject.transform.position, Quaternion.identity);
            floorPlaceHolderDict.Add(decentralandEntity.rootEntity.entityId, floorPlaceHolder);
            builderInWorldBridge?.EntityTransformReport(decentralandEntity.rootEntity, sceneToEdit);
        }

        builderInWorldEntityHandler.DeselectEntities();
        lastFloorCalalogItemUsed = floorSceneObject;
    }

    private void OnFloorLoaded(DecentralandEntity entity)
    {
        entity.OnShapeUpdated -= OnFloorLoaded;
        dclBuilderMeshLoadIndicatorController.HideIndicator(entity.entityId);

        GameObject floorPlaceHolder = floorPlaceHolderDict[entity.entityId];
        floorPlaceHolderDict.Remove(entity.entityId);
        GameObject.Destroy(floorPlaceHolder);
    }
}
