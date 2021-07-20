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

public interface IBIWFloorHandler
{
    public void CreateFloor(CatalogItem floorSceneObject);
    public bool IsCatalogItemFloor(CatalogItem floorSceneObject);
    public void ChangeFloor(CatalogItem newFloorObject);
}

public class BIWFloorHandler : BIWController, IBIWFloorHandler
{
    public event Action OnAllParcelsFloorLoaded;

    private IBIWActionController actionController;

    private IBIWEntityHandler entityHandler;
    private IBIWCreatorController creatorController;
    private IBIWSaveController saveController;

    private DCLBuilderMeshLoadIndicatorController dclBuilderMeshLoadIndicatorController;
    private DCLBuilderMeshLoadIndicator meshLoadIndicator;

    private GameObject floorPrefab;

    private int numberOfParcelsLoaded;

    private CatalogItem lastFloorCalalogItemUsed;
    private readonly Dictionary<string, GameObject> floorPlaceHolderDict = new Dictionary<string, GameObject>();
    private readonly List<string> loadedFloorEntities = new List<string>();

    public override void Init(BIWContext context)
    {
        base.Init(context);
        actionController = context.actionController;

        entityHandler = context.entityHandler;

        dclBuilderMeshLoadIndicatorController = GameObject.Instantiate(context.projectReferencesAsset.floorLoadingPrefab, context.projectReferencesAsset.floorLoadingPrefab.transform.position, context.projectReferencesAsset.floorLoadingPrefab.transform.rotation).GetComponent<DCLBuilderMeshLoadIndicatorController>();
        meshLoadIndicator = dclBuilderMeshLoadIndicatorController.indicator;
        creatorController = context.creatorController;
        saveController = context.saveController;

        floorPrefab = context.projectReferencesAsset.floorPlaceHolderPrefab;

        entityHandler.OnEntityDeleted += OnFloorEntityDeleted;
        meshLoadIndicator.SetCamera(InitialSceneReferences.i.mainCamera);

        dclBuilderMeshLoadIndicatorController.gameObject.SetActive(false);
    }

    public override void Dispose()
    {
        entityHandler.OnEntityDeleted -= OnFloorEntityDeleted;
        Clean();

        GameObject.Destroy(dclBuilderMeshLoadIndicatorController.gameObject);
    }

    private void OnFloorEntityDeleted(DCLBuilderInWorldEntity entity)
    {
        if (entity.isFloor)
            RemovePlaceHolder(entity);
    }

    public void Clean()
    {
        RemoveAllPlaceHolders();
        dclBuilderMeshLoadIndicatorController.Dispose();
    }

    public bool ExistsFloorPlaceHolderForEntity(string entityId) { return floorPlaceHolderDict.ContainsKey(entityId); }

    public void ChangeFloor(CatalogItem newFloorObject)
    {
        saveController.SetSaveActivation(false);
        CatalogItem lastFloor = lastFloorCalalogItemUsed;
        if (lastFloor == null)
            lastFloor = FindCurrentFloorCatalogItem();

        entityHandler.DeleteFloorEntities();

        CreateFloor(newFloorObject);

        BuildInWorldCompleteAction buildAction = new BuildInWorldCompleteAction();

        buildAction.CreateChangeFloorAction(lastFloor, newFloorObject);
        actionController.AddAction(buildAction);

        saveController.SetSaveActivation(true, true);
    }

    public CatalogItem FindCurrentFloorCatalogItem()
    {
        foreach (DCLBuilderInWorldEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if (entity.isFloor)
            {
                return entity.GetCatalogItemAssociated();
            }
        }

        return null;
    }

    public bool IsCatalogItemFloor(CatalogItem floorSceneObject) { return string.Equals(floorSceneObject.category, BIWSettings.FLOOR_CATEGORY); }

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
        loadedFloorEntities.Clear();

        foreach (Vector2Int parcel in parcelsPoints)
        {
            DCLBuilderInWorldEntity decentralandEntity = creatorController.CreateCatalogItem(
                floorSceneObject,
                WorldStateUtils.ConvertPointInSceneToUnityPosition(initialPosition, parcel),
                false,
                true,
                OnFloorLoaded);

            // It may happen that when you get here, the floor entity is already loaded and it wouldn't be necessary to show its loading indicator.
            if (!loadedFloorEntities.Contains(decentralandEntity.rootEntity.entityId))
            {
                dclBuilderMeshLoadIndicatorController.ShowIndicator(decentralandEntity.rootEntity.gameObject.transform.position, decentralandEntity.rootEntity.entityId);
                GameObject floorPlaceHolder = GameObject.Instantiate(floorPrefab, decentralandEntity.rootEntity.gameObject.transform.position, Quaternion.identity);
                floorPlaceHolderDict.Add(decentralandEntity.rootEntity.entityId, floorPlaceHolder);
                decentralandEntity.OnShapeFinishLoading += RemovePlaceHolder;
            }
        }

        entityHandler.DeselectEntities();
        lastFloorCalalogItemUsed = floorSceneObject;
    }

    private void RemovePlaceHolder(DCLBuilderInWorldEntity entity)
    {
        entity.OnShapeFinishLoading -= RemovePlaceHolder;
        RemovePlaceHolder(entity.rootEntity.entityId);
    }

    private void OnFloorLoaded(IDCLEntity entity)
    {
        entity.OnShapeUpdated -= OnFloorLoaded;
        loadedFloorEntities.Add(entity.entityId);
        RemovePlaceHolder(entity.entityId);

        numberOfParcelsLoaded++;
        if (sceneToEdit != null && numberOfParcelsLoaded >= sceneToEdit.sceneData.parcels.Count())
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