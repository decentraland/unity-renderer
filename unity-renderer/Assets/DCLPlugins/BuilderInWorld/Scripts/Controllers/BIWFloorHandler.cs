using System;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Builder;
using DCL.Components;
using UnityEngine;

public class BIWFloorHandler : BIWController, IBIWFloorHandler
{
    public event Action OnAllParcelsFloorLoaded;

    private IBIWActionController actionController;

    private IBIWEntityHandler entityHandler;
    private IBIWCreatorController creatorController;
    private IBIWSaveController saveController;

    private GameObject floorPrefab;

    private int numberOfParcelsLoaded;

    private CatalogItem lastFloorCalalogItemUsed;
    public Dictionary<long, GameObject> floorPlaceHolderDict { get; set; } = new Dictionary<long, GameObject>();
    private readonly List<long> loadedFloorEntities = new List<long>();
    private Camera mainCamera;

    public override void Initialize(IContext context)
    {
        base.Initialize(context);

        actionController = context.editorContext.actionController;
        entityHandler = context.editorContext.entityHandler;
        creatorController = context.editorContext.creatorController;
        saveController = context.editorContext.saveController;
        mainCamera = context.sceneReferences.mainCamera;
        floorPrefab = context.projectReferencesAsset.floorPlaceHolderPrefab;

        entityHandler.OnEntityDeleted += OnFloorEntityDeleted;
    }

    public override void EnterEditMode(IBuilderScene scene)
    {
        base.EnterEditMode(scene);
        foreach (BIWEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
        {
            if(!entity.isFloor)
                continue;

            if (!entity.isLoaded)
                entity.rootEntity.OnShapeLoaded += OnFloorLoaded;
            else
                OnFloorLoaded(entity.rootEntity);
        }
    }

    public override void Dispose()
    {
        entityHandler.OnEntityDeleted -= OnFloorEntityDeleted;
        CleanUp();
    }

    private void OnFloorEntityDeleted(BIWEntity entity)
    {
        if (entity.isFloor)
            RemovePlaceHolder(entity);
    }

    public void CleanUp() { RemoveAllPlaceHolders(); }

    public bool ExistsFloorPlaceHolderForEntity(long entityId) { return floorPlaceHolderDict.ContainsKey(entityId); }

    public void ChangeFloor(CatalogItem newFloorObject)
    {
        saveController.SetSaveActivation(false);
        CatalogItem lastFloor = lastFloorCalalogItemUsed;
        if (lastFloor == null)
            lastFloor = FindCurrentFloorCatalogItem();

        entityHandler.DeleteFloorEntities();

        CreateFloor(newFloorObject);

        BIWCompleteAction buildAction = new BIWCompleteAction();

        buildAction.CreateChangeFloorAction(lastFloor, newFloorObject);
        actionController.AddAction(buildAction);

        saveController.SetSaveActivation(true, true);
    }

    public CatalogItem FindCurrentFloorCatalogItem()
    {
        foreach (BIWEntity entity in entityHandler.GetAllEntitiesFromCurrentScene())
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
        CatalogItem floorSceneObject = BIWUtils.CreateFloorCatalogItem();
        CreateFloor(floorSceneObject);
    }

    public void CreateFloor(CatalogItem floorSceneObject)
    {
        if (sceneToEdit == null)
            return;

        Vector3 initialPosition = new Vector3(ParcelSettings.PARCEL_SIZE / 2, 0, ParcelSettings.PARCEL_SIZE / 2);
        Vector2Int[] parcelsPoints = sceneToEdit.sceneData.parcels;
        numberOfParcelsLoaded = 0;
        loadedFloorEntities.Clear();

        foreach (Vector2Int parcel in parcelsPoints)
        {
            BIWEntity entity = creatorController.CreateCatalogItem(
                floorSceneObject,
                WorldStateUtils.ConvertPointInSceneToUnityPosition(initialPosition, parcel),
                false,
                true,
                OnFloorLoaded);

            long rootEntityId = entity.rootEntity.entityId;

            // It may happen that when you get here, the floor entity is already loaded and it wouldn't be necessary to show its loading indicator.
            if (!loadedFloorEntities.Contains(rootEntityId))
            {
                GameObject floorPlaceHolder = UnityEngine.Object.Instantiate(floorPrefab, entity.rootEntity.gameObject.transform.position, Quaternion.identity);
                floorPlaceHolder.GetComponentInChildren<BIWFloorLoading>().Initialize(mainCamera);
                floorPlaceHolderDict.Add(entity.rootEntity.entityId, floorPlaceHolder);
                entity.OnShapeFinishLoading += RemovePlaceHolder;
            }
        }

        entityHandler.DeselectEntities();
        lastFloorCalalogItemUsed = floorSceneObject;
    }

    private void RemovePlaceHolder(BIWEntity entity)
    {
        entity.OnShapeFinishLoading -= RemovePlaceHolder;
        RemovePlaceHolder(entity.rootEntity.entityId);
    }

    private void OnFloorLoaded(IDCLEntity entity)
    {
        entity.OnShapeLoaded -= OnFloorLoaded;
        loadedFloorEntities.Add(entity.entityId);
        RemovePlaceHolder(entity.entityId);

        numberOfParcelsLoaded++;
        if (sceneToEdit != null && numberOfParcelsLoaded >= sceneToEdit.sceneData.parcels.Count())
            OnAllParcelsFloorLoaded?.Invoke();
    }

    private void RemovePlaceHolder(long entityId)
    {
        if (!floorPlaceHolderDict.ContainsKey(entityId))
            return;

        GameObject floorPlaceHolder = floorPlaceHolderDict[entityId];
        floorPlaceHolderDict.Remove(entityId);
        UnityEngine.Object.Destroy(floorPlaceHolder);
    }

    private void RemoveAllPlaceHolders()
    {
        foreach (GameObject gameObject in floorPlaceHolderDict.Values)
        {
            UnityEngine.Object.Destroy(gameObject);
        }

        floorPlaceHolderDict.Clear();
    }

    public override void ExitEditMode()
    {
        base.ExitEditMode();

        numberOfParcelsLoaded = 0;
        RemoveAllPlaceHolders();
    }
}