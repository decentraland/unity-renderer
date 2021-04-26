using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BIWCreatorController : BIWController
{
    [Header("Prefab references")]
    public BIWModeController biwModeController;

    public BIWFloorHandler biwFloorHandler;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;

    [FormerlySerializedAs("loadingGO")]
    [Header("Project references")]
    public GameObject loadingObjectPrefab;

    [SerializeField]
    internal InputAction_Trigger toggleCreateLastSceneObjectInputAction;

    public Action OnInputDone;

    //Note(Adrian): This is for tutorial purposes
    public Action OnSceneObjectPlaced;

    private CatalogItem lastCatalogItemCreated;

    private InputAction_Trigger.Triggered createLastSceneObjectDelegate;

    private readonly Dictionary<string, BIWLoadingPlaceHolder> loadingGameObjects = new Dictionary<string, BIWLoadingPlaceHolder>();

    private void Start()
    {
        createLastSceneObjectDelegate = (action) => CreateLastCatalogItem();
        toggleCreateLastSceneObjectInputAction.OnTriggered += createLastSceneObjectDelegate;
    }

    private void OnDestroy()
    {
        toggleCreateLastSceneObjectInputAction.OnTriggered -= createLastSceneObjectDelegate;
        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.OnCatalogItemSelected -= OnCatalogItemSelected;
        Clean();
    }

    public void Clean()
    {
        foreach (BIWLoadingPlaceHolder placeHolder in loadingGameObjects.Values)
        {
            placeHolder.Disspose();
        }

        loadingGameObjects.Clear();
    }

    public override void Init()
    {
        base.Init();
        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.OnCatalogItemSelected += OnCatalogItemSelected;
    }

    private bool IsInsideTheLimits(CatalogItem sceneObject)
    {
        if (HUDController.i.builderInWorldMainHud == null)
            return false;

        SceneMetricsController.Model limits = sceneToEdit.metricsController.GetLimits();
        SceneMetricsController.Model usage = sceneToEdit.metricsController.GetModel();

        if (limits.bodies < usage.bodies + sceneObject.metrics.bodies)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.entities < usage.entities + sceneObject.metrics.entities)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.materials < usage.materials + sceneObject.metrics.materials)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.meshes < usage.meshes + sceneObject.metrics.meshes)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.textures < usage.textures + sceneObject.metrics.textures)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.triangles < usage.triangles + sceneObject.metrics.triangles)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        return true;
    }

    public DCLBuilderInWorldEntity CreateCatalogItem(CatalogItem catalogItem, bool autoSelect = true, bool isFloor = false)
    {
        if (catalogItem.IsNFT() && BuilderInWorldNFTController.i.IsNFTInUse(catalogItem.id))
            return null;

        IsInsideTheLimits(catalogItem);

        //Note (Adrian): This is a workaround until the mapping is handle by kernel
        AddSceneMappings(catalogItem);

        Vector3 startPosition = biwModeController.GetModeCreationEntryPoint();
        Vector3 editionPosition = biwModeController.GetCurrentEditionPosition();

        DCLBuilderInWorldEntity entity = builderInWorldEntityHandler.CreateEmptyEntity(sceneToEdit, startPosition, editionPosition, false);
        entity.isFloor = isFloor;
        entity.SetRotation(Vector3.zero);

        if (!isFloor)
            CreateLoadingObject(entity);

        AddShape(catalogItem, entity);

        AddEntityNameComponent(catalogItem, entity);

        AddLockedComponent(entity);

        if (catalogItem.IsSmartItem())
        {
            AddSmartItemComponent(entity);
        }

        if (catalogItem.IsVoxel())
            entity.isVoxel = true;

        if (autoSelect)
        {
            builderInWorldEntityHandler.DeselectEntities();
            builderInWorldEntityHandler.Select(entity.rootEntity);
        }

        entity.gameObject.transform.eulerAngles = Vector3.zero;

        biwModeController.CreatedEntity(entity);

        lastCatalogItemCreated = catalogItem;

        entity.OnShapeFinishLoading += OnShapeLoadFinish;
        builderInWorldEntityHandler.EntityListChanged();
        builderInWorldEntityHandler.NotifyEntityIsCreated(entity.rootEntity);
        OnInputDone?.Invoke();
        OnSceneObjectPlaced?.Invoke();
        return entity;
    }

    #region LoadingObjects

    public bool ExistsLoadingGameObjectForEntity(string entityId) { return loadingGameObjects.ContainsKey(entityId); }

    private void CreateLoadingObject(DCLBuilderInWorldEntity entity)
    {
        BIWLoadingPlaceHolder loadingPlaceHolder = GameObject.Instantiate(loadingObjectPrefab, entity.gameObject.transform).GetComponent<BIWLoadingPlaceHolder>();
        loadingGameObjects.Add(entity.rootEntity.entityId, loadingPlaceHolder);
    }

    private void OnShapeLoadFinish(DCLBuilderInWorldEntity entity)
    {
        entity.OnShapeFinishLoading -= OnShapeLoadFinish;
        RemoveLoadingObject(entity.rootEntity.entityId);
    }

    public void RemoveLoadingObject(string entityId)
    {
        if (!loadingGameObjects.ContainsKey(entityId))
            return;
        BIWLoadingPlaceHolder loadingPlaceHolder = loadingGameObjects[entityId];
        loadingGameObjects.Remove(entityId);
        loadingPlaceHolder.DestroyAfterAnimation();
    }

    #endregion

    #region Add Components

    private void AddSmartItemComponent(DCLBuilderInWorldEntity entity)
    {
        //Note (Adrian): This will disable the smart item component until it is implemented in kernel
        //TODO: After the implementation in kernel of smart items, we should eliminate this return
        return;
        SmartItemComponent.Model model = new SmartItemComponent.Model();
        model.values = new Dictionary<object, object>();

        sceneToEdit.EntityComponentCreateOrUpdateWithModel(entity.rootEntity.entityId, CLASS_ID_COMPONENT.SMART_ITEM, model);

        //Note (Adrian): We can't wait to set the component 1 frame, so we set it
        if (entity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent component))
            ((SmartItemComponent) component).UpdateFromModel(model);
    }

    private void AddEntityNameComponent(CatalogItem catalogItem, DCLBuilderInWorldEntity entity)
    {
        DCLName name = (DCLName) sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));
        sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, name.id);
        builderInWorldEntityHandler.SetEntityName(entity, catalogItem.name);
    }

    private void AddLockedComponent(DCLBuilderInWorldEntity entity)
    {
        DCLLockedOnEdit entityLocked = (DCLLockedOnEdit) sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));
        if (entity.isFloor)
            entityLocked.SetIsLocked(true);
        else
            entityLocked.SetIsLocked(false);

        sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, entityLocked.id);
    }

    private void AddShape(CatalogItem catalogItem, DCLBuilderInWorldEntity entity)
    {
        if (catalogItem.IsNFT())
        {
            NFTShape nftShape = (NFTShape) sceneToEdit.SharedComponentCreate(catalogItem.id, Convert.ToInt32(CLASS_ID.NFT_SHAPE));
            nftShape.model = new NFTShape.Model();
            nftShape.model.color = new Color(0.6404918f, 0.611472f, 0.8584906f);
            nftShape.model.src = catalogItem.model;
            nftShape.model.assetId = catalogItem.id;
            sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, nftShape.id);

            nftShape.CallWhenReady(entity.ShapeLoadFinish);
        }
        else
        {
            GLTFShape gltfComponent = (GLTFShape) sceneToEdit.SharedComponentCreate(catalogItem.id, Convert.ToInt32(CLASS_ID.GLTF_SHAPE));
            gltfComponent.model = new LoadableShape.Model();
            gltfComponent.model.src = catalogItem.model;
            gltfComponent.model.assetId = catalogItem.id;
            sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, gltfComponent.id);

            gltfComponent.CallWhenReady(entity.ShapeLoadFinish);
        }
    }

    #endregion

    private void AddSceneMappings(CatalogItem catalogItem)
    {
        LoadParcelScenesMessage.UnityParcelScene data = sceneToEdit.sceneData;
        data.baseUrl = BuilderInWorldSettings.BASE_URL_CATALOG;
        if (data.contents == null)
            data.contents = new List<ContentServerUtils.MappingPair>();
        foreach (KeyValuePair<string, string> content in catalogItem.contents)
        {
            ContentServerUtils.MappingPair mappingPair = new ContentServerUtils.MappingPair();
            mappingPair.file = content.Key;
            mappingPair.hash = content.Value;
            bool found = false;
            foreach (ContentServerUtils.MappingPair mappingPairToCheck in data.contents)
            {
                if (mappingPairToCheck.file == mappingPair.file)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                data.contents.Add(mappingPair);
        }

        DCL.Environment.i.world.sceneController.UpdateParcelScenesExecute(data);
    }

    public void CreateLastCatalogItem()
    {
        if (lastCatalogItemCreated != null)
        {
            if (builderInWorldEntityHandler.IsAnyEntitySelected())
                builderInWorldEntityHandler.DeselectEntities();
            OnCatalogItemSelected(lastCatalogItemCreated);
            OnInputDone?.Invoke();
        }
    }

    private void OnCatalogItemSelected(CatalogItem catalogItem)
    {
        if (biwFloorHandler.IsCatalogItemFloor(catalogItem))
        {
            biwFloorHandler.ChangeFloor(catalogItem);
        }
        else
        {
            CreateCatalogItem(catalogItem);
        }
    }
}