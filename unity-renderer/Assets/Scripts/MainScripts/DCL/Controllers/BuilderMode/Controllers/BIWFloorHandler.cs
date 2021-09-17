using System;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IBIWFloorHandler : IDisposable
{
    void EnterEditMode( IParcelScene scene );
    void CreateDefaultFloor();
    void CreateFloor(CatalogItem floorSceneObject);
    bool IsCatalogItemFloor(CatalogItem floorSceneObject);
    void ChangeFloor(CatalogItem newFloorObject);
}

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
    internal readonly Dictionary<string, GameObject> floorPlaceHolderDict = new Dictionary<string, GameObject>();
    private readonly List<string> loadedFloorEntities = new List<string>();
    private Camera mainCamera;

    public override void Initialize(BIWContext context)
    {
        base.Initialize(context);
        actionController = context.actionController;

        entityHandler = context.entityHandler;

        creatorController = context.creatorController;
        saveController = context.saveController;
        mainCamera = context.sceneReferences.mainCamera;

        floorPrefab = context.projectReferencesAsset.floorPlaceHolderPrefab;

        entityHandler.OnEntityDeleted += OnFloorEntityDeleted;
    }

    public override void Dispose()
    {
        entityHandler.OnEntityDeleted -= OnFloorEntityDeleted;
        Clean();
    }

    private void OnFloorEntityDeleted(BIWEntity entity)
    {
        if (entity.isFloor)
            RemovePlaceHolder(entity);
    }

    public void Clean() { RemoveAllPlaceHolders(); }

    public bool ExistsFloorPlaceHolderForEntity(string entityId) { return floorPlaceHolderDict.ContainsKey(entityId); }

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
        CatalogItem floorSceneObject = BIWUtils.CreateFloorSceneObject();
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
            BIWEntity decentralandEntity = creatorController.CreateCatalogItem(
                floorSceneObject,
                WorldStateUtils.ConvertPointInSceneToUnityPosition(initialPosition, parcel),
                false,
                true,
                OnFloorLoaded);

            // It may happen that when you get here, the floor entity is already loaded and it wouldn't be necessary to show its loading indicator.
            if (!loadedFloorEntities.Contains(decentralandEntity.rootEntity.entityId))
            {
                GameObject floorPlaceHolder = GameObject.Instantiate(floorPrefab, decentralandEntity.rootEntity.gameObject.transform.position, Quaternion.identity);
                floorPlaceHolder.GetComponentInChildren<BIWFloorLoading>().Initialize(mainCamera);
                floorPlaceHolderDict.Add(decentralandEntity.rootEntity.entityId, floorPlaceHolder);
                decentralandEntity.OnShapeFinishLoading += RemovePlaceHolder;
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
    }
}