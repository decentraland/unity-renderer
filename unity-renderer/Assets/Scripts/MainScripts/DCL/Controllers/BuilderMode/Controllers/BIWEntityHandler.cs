using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Environment = DCL.Environment;

public interface IBIWEntityHandler
{
    public event Action<BIWEntity> OnEntityDeselected;
    public event Action OnEntitySelected;
    public event Action<List<BIWEntity>> OnDeleteSelectedEntities;
    public event Action<BIWEntity> OnEntityDeleted;
    public BIWEntity GetConvertedEntity(string entityId);
    public void DeleteEntity(BIWEntity entityToDelete);
    public void DeleteEntity(string entityId);
    public void DeleteFloorEntities();
    public void DeleteSelectedEntities();
    public IDCLEntity CreateEntityFromJSON(string entityJson);
    public BIWEntity CreateEmptyEntity(ParcelScene parcelScene, Vector3 entryPoint, Vector3 editionGOPosition, bool notifyEntityList = true);
    public List<BIWEntity> GetAllEntitiesFromCurrentScene();
    public void DeselectEntities();
    public List<BIWEntity> GetSelectedEntityList();
    public BIWEntity GetEntityOnPointer();
    public bool IsAnyEntitySelected();
    public void SetActiveMode(BuilderInWorldMode buildMode);
    public void SetMultiSelectionActive(bool isActive);
    public void ChangeLockStateSelectedEntities();
    public void DeleteEntitiesOutsideSceneBoundaries();
    public bool AreAllSelectedEntitiesInsideBoundaries();
    public bool AreAllEntitiesInsideBoundaries();
    public void EntityListChanged();
    public void NotifyEntityIsCreated(IDCLEntity entity);
    public void SetEntityName(BIWEntity entityToApply, string newName, bool sendUpdateToKernel = true);
    public void EntityClicked(BIWEntity entityToSelect);
    public void ReportTransform(bool forceReport = false);
    public void CancelSelection();
    public bool IsPointerInSelectedEntity();
    public void DestroyLastCreatedEntities();
    public void Select(IDCLEntity entity);
    public bool SelectEntity(BIWEntity entityEditable, bool selectedFromCatalog = false);
    public void DeselectEntity(BIWEntity entity);
    public int GetCurrentSceneEntityCount();
}

public class BIWEntityHandler : BIWController, IBIWEntityHandler
{
    private const float DUPLICATE_OFFSET = 2f;

    private IBIWOutlinerController outlinerController;

    private IBIWModeController modeController;
    private IBIWActionController actionController;
    private IBIWCreatorController creatorController;

    private BuilderInWorldBridge bridge;
    private Material editMaterial;

    private InputAction_Trigger hideSelectedEntitiesAction;

    private InputAction_Trigger showAllEntitiesAction;

    private readonly Dictionary<string, BIWEntity> convertedEntities = new Dictionary<string, BIWEntity>();
    private readonly List<BIWEntity> selectedEntities = new List<BIWEntity>();

    private BuilderInWorldMode currentActiveMode;
    private bool isMultiSelectionActive = false;
    private bool isSecondayClickPressed = false;

    private float lastTransformReportTime;

    private List<string> entityNameList = new List<string>();

    private InputAction_Trigger.Triggered hideSelectedEntitiesDelegate;
    private InputAction_Trigger.Triggered showAllEntitiesDelegate;

    private BuildModeHUDController hudController;

    public event Action<BIWEntity> OnEntityDeselected;
    public event Action OnEntitySelected;
    public event Action<List<BIWEntity>> OnDeleteSelectedEntities;
    public event Action<BIWEntity> OnEntityDeleted;

    private BIWEntity lastClickedEntity;
    private float lastTimeEntityClicked;

    public override void Init(BIWContext context)
    {
        base.Init(context);
        if (HUDController.i.builderInWorldMainHud != null)
        {
            hudController = HUDController.i.builderInWorldMainHud;
            hudController.OnEntityDelete += DeleteSingleEntity;
            hudController.OnDuplicateSelectedAction += DuplicateSelectedEntitiesInput;
            hudController.OnDeleteSelectedAction += DeleteSelectedEntitiesInput;
            hudController.OnEntityClick += ChangeEntitySelectionFromList;
            hudController.OnEntityLock += ChangeEntityLockStatus;
            hudController.OnEntityChangeVisibility += ChangeEntityVisibilityStatus;
            hudController.OnEntityRename += SetEntityName;
            hudController.OnEntitySmartItemComponentUpdate += UpdateSmartItemComponentInKernel;
        }


        BIWInputWrapper.OnMouseDown += OnInputMouseDown;
        BIWInputWrapper.OnMouseUp += OnInputMouseUp;

        DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged += ChangeEntityBoundsCheckerStatus;

        bridge = InitialSceneReferences.i.builderInWorldBridge;

        outlinerController = context.outlinerController;

        modeController = context.modeController;
        actionController = context.actionController;
        creatorController = context.creatorController;

        editMaterial = context.projectReferences.editMaterial;

        hideSelectedEntitiesAction = context.inputsReferences.hideSelectedEntitiesAction;
        showAllEntitiesAction = context.inputsReferences.showAllEntitiesAction;

        hideSelectedEntitiesDelegate = (action) => ChangeShowStateSelectedEntities();
        showAllEntitiesDelegate = (action) => ShowAllEntities();

        hideSelectedEntitiesAction.OnTriggered += hideSelectedEntitiesDelegate;
        showAllEntitiesAction.OnTriggered += showAllEntitiesDelegate;

        actionController.OnRedo += ReSelectEntities;
        actionController.OnUndo += ReSelectEntities;
    }

    private void OnInputMouseDown(int buttonId, Vector3 mousePosition)
    {
        if (buttonId == 1)
            isSecondayClickPressed = true;
    }

    private void OnInputMouseUp(int buttonId, Vector3 mousePosition)
    {
        if (buttonId == 1)
            isSecondayClickPressed = false;
    }

    public override void Dispose()
    {
        DestroyCollidersForAllEntities();

        actionController.OnRedo -= ReSelectEntities;
        actionController.OnUndo -= ReSelectEntities;

        hideSelectedEntitiesAction.OnTriggered -= hideSelectedEntitiesDelegate;
        showAllEntitiesAction.OnTriggered -= showAllEntitiesDelegate;

        DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged -= ChangeEntityBoundsCheckerStatus;

        BIWInputWrapper.OnMouseDown -= OnInputMouseDown;
        BIWInputWrapper.OnMouseUp -= OnInputMouseUp;

        if (hudController != null)
        {
            hudController.OnEntityDelete -= DeleteSingleEntity;
            hudController.OnDuplicateSelectedAction -= DuplicateSelectedEntitiesInput;
            hudController.OnDeleteSelectedAction -= DeleteSelectedEntitiesInput;
            hudController.OnEntityClick -= ChangeEntitySelectionFromList;
            hudController.OnEntityLock -= ChangeEntityLockStatus;
            hudController.OnEntityChangeVisibility -= ChangeEntityVisibilityStatus;
            hudController.OnEntityChangeVisibility -= ChangeEntityVisibilityStatus;
            hudController.OnEntityRename -= SetEntityName;
            hudController.OnEntitySmartItemComponentUpdate -= UpdateSmartItemComponentInKernel;
        }
    }

    public override void Update()
    {
        base.Update();

        if (selectedEntities.Count == 0)
            return;
        if ((DCLTime.realtimeSinceStartup - lastTransformReportTime) <= BIWSettings.ENTITY_POSITION_REPORTING_DELAY)
            return;

        ReportTransform();
    }

    public void ReportTransform(bool forceReport = false)
    {
        foreach (BIWEntity entity in selectedEntities)
        {
            if (!entity.HasMovedSinceLastReport() &&
                !entity.HasScaledSinceLastReport() &&
                !entity.HasRotatedSinceLastReport() &&
                !forceReport)
                return;
            bridge.EntityTransformReport(entity.rootEntity, sceneToEdit);
            entity.PositionReported();
            entity.ScaleReported();
            entity.RotationReported();
        }

        lastTransformReportTime = DCLTime.realtimeSinceStartup;
    }

    public List<BIWEntity> GetSelectedEntityList() { return selectedEntities; }

    public bool IsAnyEntitySelected() { return selectedEntities.Count > 0; }

    public void SetActiveMode(BuilderInWorldMode buildMode)
    {
        currentActiveMode = buildMode;
        DeselectEntities();
    }

    public void SetMultiSelectionActive(bool isActive) { isMultiSelectionActive = isActive; }

    public override void EnterEditMode(ParcelScene scene)
    {
        base.EnterEditMode(scene);

        SetupAllEntities();
        EntityListChanged();
        CheckErrorOnEntities();
    }

    private void CheckErrorOnEntities()
    {
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            entity.CheckErrors();
            if (entity.hasMissingCatalogItemError)
                creatorController.CreateErrorOnEntity(entity);
        }
    }

    public bool IsPointerInSelectedEntity()
    {
        BIWEntity entityInPointer = GetEntityOnPointer();
        if (entityInPointer == null)
            return false;

        foreach (BIWEntity entity in selectedEntities)
        {
            if (entityInPointer == entity)
                return true;
        }

        return false;
    }

    private void DeleteSelectedEntitiesInput()
    {
        if (selectedEntities.Count > 0)
            DeleteSelectedEntities();
    }

    private void DuplicateSelectedEntitiesInput()
    {
        if (selectedEntities.Count <= 0 || isSecondayClickPressed)
            return;

        DuplicateSelectedEntities();
    }

    public override void ExitEditMode()
    {
        base.ExitEditMode();
        DeselectEntities();

        foreach (BIWEntity entity in convertedEntities.Values)
        {
            entity.Dispose();
        }

        convertedEntities.Clear();
    }

    private void ChangeEntitySelectionFromList(BIWEntity entityToEdit)
    {
        if (!selectedEntities.Contains(entityToEdit))
            SelectFromList(entityToEdit);
        else
            DeselectEntity(entityToEdit);
    }

    private void SelectFromList(BIWEntity entityToEdit)
    {
        if (!isMultiSelectionActive)
            DeselectEntities();
        if (SelectEntity(entityToEdit))
        {
            if (!isMultiSelectionActive)
                outlinerController.OutlineEntity(entityToEdit);
            else
                outlinerController.OutlineEntities(selectedEntities);
        }
    }

    public void DeselectEntity(BIWEntity entity)
    {
        if (!selectedEntities.Contains(entity))
            return;

        entity.Deselect();

        outlinerController.CancelEntityOutline(entity);
        selectedEntities.Remove(entity);
        hudController?.UpdateEntitiesSelection(selectedEntities.Count);
        currentActiveMode?.EntityDeselected(entity);
        if (selectedEntities.Count <= 0 &&
            hudController != null)
            hudController.HideEntityInformation();

        OnEntityDeselected?.Invoke(entity);
    }

    public void DeselectEntities()
    {
        if (selectedEntities.Count <= 0)
            return;

        int amountToDeselect = selectedEntities.Count;
        for (int i = 0; i < amountToDeselect; i++)
        {
            DeselectEntity(selectedEntities[0]);
        }

        currentActiveMode?.OnDeselectedEntities();

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public BIWEntity GetEntityOnPointer()
    {
        Camera camera = Camera.main;

        if (camera == null)
            return null;

        RaycastHit hit;
        UnityEngine.Ray ray = camera.ScreenPointToRay(modeController.GetMousePosition());
        float distanceToSelect = modeController.GetMaxDistanceToSelectEntities();

        if (Physics.Raycast(ray, out hit, distanceToSelect, BIWSettings.COLLIDER_SELECTION_LAYER))
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                return GetConvertedEntity(sceneToEdit.entities[entityID]);
            }
        }

        return null;
    }

    public void EntityClicked(BIWEntity entityToSelect)
    {
        if (entityToSelect != null)
        {
            if (!isMultiSelectionActive)
                DeselectEntities();

            if (!entityToSelect.IsLocked)
                ChangeEntitySelectStatus(entityToSelect);

            if (entityToSelect == lastClickedEntity && (lastTimeEntityClicked + BIWSettings.MOUSE_MS_DOUBLE_CLICK_THRESHOLD / 1000f) >= Time.realtimeSinceStartup )
                modeController.EntityDoubleClick(entityToSelect);

            lastClickedEntity = entityToSelect;
            lastTimeEntityClicked = Time.realtimeSinceStartup;
        }
        else if (!isMultiSelectionActive)
        {
            DeselectEntities();
        }
    }

    void ReSelectEntities()
    {
        List<BIWEntity> entitiesToReselect = new List<BIWEntity>();
        foreach (BIWEntity entity in selectedEntities)
        {
            entitiesToReselect.Add(entity);
        }

        DeselectEntities();

        foreach (BIWEntity entity in entitiesToReselect)
        {
            SelectEntity(entity);
        }
    }

    void ChangeEntitySelectStatus(BIWEntity entityCliked)
    {
        if (entityCliked.IsSelected)
            DeselectEntity(entityCliked);
        else
            SelectEntity(entityCliked);
    }

    public void CancelSelection()
    {
        if (selectedEntities.Count == 0)
            return;

        DestroyLastCreatedEntities();
        DeselectEntities();
    }

    public void ChangeLockStateSelectedEntities()
    {
        foreach (BIWEntity entity in selectedEntities)
        {
            entity.ToggleLockStatus();
        }

        DeselectEntities();
    }

    public void ShowAllEntities()
    {
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (!entity.IsVisible)
                entity.ToggleShowStatus();
        }
    }

    public void ChangeShowStateSelectedEntities()
    {
        List<BIWEntity> entitiesToHide = new List<BIWEntity>(selectedEntities);

        foreach (BIWEntity entity in entitiesToHide)
        {
            if (entity.IsVisible && entity.IsSelected)
                DeselectEntity(entity);
            entity.ToggleShowStatus();
        }
    }

    public void Select(IDCLEntity entity)
    {
        BIWEntity entityEditable = GetConvertedEntity(entity);
        if (entityEditable == null)
            return;

        SelectEntity(entityEditable, true);
    }

    public bool SelectEntity(BIWEntity entityEditable, bool selectedFromCatalog = false)
    {
        if (entityEditable.IsLocked)
            return false;

        if (entityEditable.IsSelected)
            return false;

        entityEditable.Select();

        selectedEntities.Add(entityEditable);

        currentActiveMode?.SelectedEntity(entityEditable);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            hudController.UpdateEntitiesSelection(selectedEntities.Count);
            hudController.ShowEntityInformation(selectedFromCatalog);
            hudController.EntityInformationSetEntity(entityEditable, sceneToEdit);
        }

        outlinerController.CancelAllOutlines();

        OnEntitySelected?.Invoke();

        return true;
    }

    public List<BIWEntity> GetAllVoxelsEntities()
    {
        List<BIWEntity> voxelEntities = new List<BIWEntity>();
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit && entity.isVoxel)
                voxelEntities.Add(entity);
        }

        return voxelEntities;
    }

    public List<BIWEntity> GetAllEntitiesFromCurrentScene()
    {
        List<BIWEntity> entities = new List<BIWEntity>();
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
                entities.Add(entity);
        }

        return entities;
    }

    public BIWEntity GetConvertedEntity(string entityId)
    {
        if (convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(entityId)))
            return convertedEntities[GetConvertedUniqueKeyForEntity(entityId)];

        return null;
    }

    public BIWEntity GetConvertedEntity(IDCLEntity entity)
    {
        if (convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(entity)))
            return convertedEntities[GetConvertedUniqueKeyForEntity(entity)];

        return null;
    }

    public void DuplicateSelectedEntities()
    {
        BuildInWorldCompleteAction buildAction = new BuildInWorldCompleteAction();
        buildAction.actionType = BuildInWorldCompleteAction.ActionType.CREATE;

        List<BuilderInWorldEntityAction> entityActionList = new List<BuilderInWorldEntityAction>();
        List<BIWEntity> entitiesToDuplicate = new List<BIWEntity>(selectedEntities);
        DeselectEntities();

        foreach (BIWEntity entityToDuplicate in entitiesToDuplicate)
        {
            if (entityToDuplicate.isNFT)
                continue;

            var entityDuplicated = DuplicateEntity(entityToDuplicate);
            BuilderInWorldEntityAction builderInWorldEntityAction = new BuilderInWorldEntityAction(entityDuplicated.rootEntity, entityDuplicated.rootEntity.entityId, BuilderInWorldUtils.ConvertEntityToJSON(entityDuplicated.rootEntity));
            entityActionList.Add(builderInWorldEntityAction);
            SelectEntity(entityDuplicated);
        }

        currentActiveMode?.SetDuplicationOffset(DUPLICATE_OFFSET);

        buildAction.CreateActionType(entityActionList, BuildInWorldCompleteAction.ActionType.CREATE);
        actionController.AddAction(buildAction);
    }

    public BIWEntity DuplicateEntity(BIWEntity entityToDuplicate)
    {
        IDCLEntity entity = SceneUtils.DuplicateEntity(sceneToEdit, entityToDuplicate.rootEntity);
        //Note: If the entity contains the name component or DCLLockedOnEdit, we don't want to copy them 
        entity.RemoveSharedComponent(typeof(DCLName), false);
        entity.RemoveSharedComponent(typeof(DCLLockedOnEdit), false);

        BuilderInWorldUtils.CopyGameObjectStatus(entityToDuplicate.rootEntity.gameObject, entity.gameObject, false, false);
        BIWEntity convertedEntity = SetupEntityToEdit(entity);

        NotifyEntityIsCreated(entity);
        EntityListChanged();
        return convertedEntity;
    }

    public IDCLEntity CreateEntityFromJSON(string entityJson)
    {
        EntityData data = BuilderInWorldUtils.ConvertJSONToEntityData(entityJson);

        IDCLEntity newEntity = sceneToEdit.CreateEntity(data.entityId);


        if (data.transformComponent != null)
        {
            DCLTransform.model.position = data.transformComponent.position;
            DCLTransform.model.rotation = Quaternion.Euler(data.transformComponent.rotation);
            DCLTransform.model.scale = data.transformComponent.scale;
            sceneToEdit.EntityComponentCreateOrUpdateWithModel(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);
        }

        foreach (ProtocolV2.GenericComponent component in data.components)
        {
            sceneToEdit.EntityComponentCreateOrUpdateWithModel(newEntity.entityId, (CLASS_ID_COMPONENT) component.componentId, component.data);
        }

        foreach (ProtocolV2.GenericComponent component in data.sharedComponents)
        {
            sceneToEdit.SharedComponentAttach(newEntity.entityId, component.classId);
        }

        if (data.nftComponent != null)
        {
            NFTShape nftShape = (NFTShape) sceneToEdit.SharedComponentCreate(data.nftComponent.id, Convert.ToInt32(CLASS_ID.NFT_SHAPE));
            nftShape.model = new NFTShape.Model();
            nftShape.model.color = data.nftComponent.color.ToColor();
            nftShape.model.src = data.nftComponent.src;
            nftShape.model.assetId = data.nftComponent.assetId;

            sceneToEdit.SharedComponentAttach(newEntity.entityId, nftShape.id);
        }

        var convertedEntity = SetupEntityToEdit(newEntity, true);

        if (convertedEntity.rootEntity.TryGetSharedComponent(CLASS_ID.GLTF_SHAPE, out var gltfComponent))
            gltfComponent.CallWhenReady(convertedEntity.ShapeLoadFinish);

        if (convertedEntity.rootEntity.TryGetSharedComponent(CLASS_ID.NFT_SHAPE, out var nftComponent))
            nftComponent.CallWhenReady(convertedEntity.ShapeLoadFinish);


        creatorController.CreateLoadingObject(convertedEntity);
        EntityListChanged();

        return newEntity;
    }

    public BIWEntity CreateEmptyEntity(ParcelScene parcelScene, Vector3 entryPoint, Vector3 editionGOPosition, bool notifyEntityList = true)
    {
        IDCLEntity newEntity = parcelScene.CreateEntity(Guid.NewGuid().ToString());

        DCLTransform.model.position = WorldStateUtils.ConvertUnityToScenePosition(entryPoint, parcelScene);

        Vector3 pointToLookAt = Camera.main.transform.position;
        pointToLookAt.y = editionGOPosition.y;
        Quaternion lookOnLook = Quaternion.LookRotation(editionGOPosition - pointToLookAt);

        DCLTransform.model.rotation = lookOnLook;
        DCLTransform.model.scale = newEntity.gameObject.transform.lossyScale;

        parcelScene.EntityComponentCreateOrUpdateWithModel(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);

        BIWEntity convertedEntity = SetupEntityToEdit(newEntity, true);
        hudController?.UpdateSceneLimitInfo();

        if (notifyEntityList)
            EntityListChanged();
        return convertedEntity;
    }

    private void SetupAllEntities()
    {
        foreach (IDCLEntity entity in sceneToEdit.entities.Values)
        {
            SetupEntityToEdit(entity);
        }
    }

    public void DestroyLastCreatedEntities()
    {
        List<BIWEntity> entitiesToRemove = new List<BIWEntity>();
        foreach (BIWEntity entity in selectedEntities)
        {
            if (entity.IsSelected && entity.IsNew)
                entitiesToRemove.Add(entity);
        }

        if (entitiesToRemove.Count == 0)
            return;

        modeController.UndoEditionGOLastStep();

        foreach (BIWEntity entity in entitiesToRemove)
        {
            DeleteEntity(entity, false);
        }

        hudController.HideEntityInformation();
    }

    public void EntityListChanged()
    {
        if (HUDController.i.builderInWorldMainHud == null)
            return;
        hudController.SetEntityList(GetEntitiesInCurrentScene());
    }

    public int GetCurrentSceneEntityCount() { return GetEntitiesInCurrentScene().Count; }

    List<BIWEntity> GetEntitiesInCurrentScene()
    {
        List<BIWEntity> currentEntitiesInScene = new List<BIWEntity>();
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
                currentEntitiesInScene.Add(entity);
        }

        return currentEntitiesInScene;
    }

    BIWEntity SetupEntityToEdit(IDCLEntity entity, bool hasBeenCreated = false)
    {
        if (!convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(entity)))
        {
            BIWEntity entityToEdit = new BIWEntity();
            entityToEdit.Init(entity, editMaterial);
            convertedEntities.Add(entityToEdit.entityUniqueId, entityToEdit);
            entity.OnRemoved += RemoveConvertedEntity;
            entityToEdit.IsNew = hasBeenCreated;

            string entityName = entityToEdit.GetDescriptiveName();
            var catalogItem = entityToEdit.GetCatalogItemAssociated();

            if ((string.IsNullOrEmpty(entityName) || entityNameList.Contains(entityName)) && catalogItem != null)
            {
                entityName = GetNewNameForEntity(catalogItem);
                SetEntityName(entityToEdit, entityName);
            }
            else if (!string.IsNullOrEmpty(entityName) && !entityNameList.Contains(entityName))
            {
                entityNameList.Add(entityName);
            }

            return entityToEdit;
        }
        else
        {
            return convertedEntities[GetConvertedUniqueKeyForEntity(entity)];
        }
    }

    private void ChangeEntityBoundsCheckerStatus(IDCLEntity entity, bool isInsideBoundaries)
    {
        var convertedEntity = GetConvertedEntity(entity);
        if (convertedEntity == null)
            return;

        convertedEntity.SetEntityBoundariesError(isInsideBoundaries);
    }

    public string GetNewNameForEntity(CatalogItem sceneObject) { return GetNewNameForEntity(sceneObject.name); }

    public string GetNewNameForEntity(string name)
    {
        int i = 1;
        if (!entityNameList.Contains(name))
            return name;

        string newName = name + " " + i;
        while (entityNameList.Contains(newName))
        {
            i++;
            newName = name + " " + i;
        }

        return newName;
    }

    public void DeleteFloorEntities()
    {
        List<BIWEntity> entitiesToDelete = new List<BIWEntity>();

        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (entity.isFloor)
            {
                entitiesToDelete.Add(entity);
            }
        }

        foreach (BIWEntity entity in entitiesToDelete)
            DeleteEntity(entity, false);
    }

    public void DeleteEntity(string entityId)
    {
        BIWEntity entity = convertedEntities[GetConvertedUniqueKeyForEntity(entityId)];
        DeleteEntity(entity, true);
    }

    public void DeleteEntity(BIWEntity entityToDelete) { DeleteEntity(entityToDelete, true); }

    public void DeleteEntity(BIWEntity entityToDelete, bool checkSelection)
    {
        if (entityToDelete.IsSelected && checkSelection)
            DeselectEntity(entityToDelete);

        if (selectedEntities.Contains(entityToDelete))
        {
            selectedEntities.Remove(entityToDelete);
            hudController?.UpdateEntitiesSelection(selectedEntities.Count);
        }

        string entityName = entityToDelete.GetDescriptiveName();

        if (entityNameList.Contains(entityName))
            entityNameList.Remove(entityName);

        RemoveConvertedEntity(entityToDelete.rootEntity);
        entityToDelete.rootEntity.OnRemoved -= RemoveConvertedEntity;
        entityToDelete.Delete();
        string idToRemove = entityToDelete.rootEntity.entityId;
        OnEntityDeleted?.Invoke(entityToDelete);
        creatorController.RemoveLoadingObjectInmediate(entityToDelete.rootEntity.entityId);
        if (sceneToEdit.entities.ContainsKey(idToRemove))
            sceneToEdit.RemoveEntity(idToRemove, true);

        hudController?.RefreshCatalogAssetPack();
        EntityListChanged();
        bridge?.RemoveEntityOnKernel(idToRemove, sceneToEdit);
    }

    public void DeleteSingleEntity(BIWEntity entityToDelete)
    {
        actionController.CreateActionEntityDeleted(entityToDelete);
        DeleteEntity(entityToDelete, true);
    }

    public void DeleteSelectedEntities()
    {
        List<BIWEntity> entitiesToRemove = new List<BIWEntity>();

        for (int i = 0; i < selectedEntities.Count; i++)
        {
            entitiesToRemove.Add(selectedEntities[i]);
        }

        actionController.CreateActionEntityDeleted(entitiesToRemove);

        DeselectEntities();

        foreach (BIWEntity entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }

        OnDeleteSelectedEntities?.Invoke(entitiesToRemove);
    }

    public void DeleteEntitiesOutsideSceneBoundaries()
    {
        List<BIWEntity> entitiesToRemove = new List<BIWEntity>();
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
            {
                if (!DCL.Environment.i.world.sceneBoundsChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
                {
                    entitiesToRemove.Add(entity);
                }
            }
        }

        foreach (BIWEntity entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }
    }

    private void DestroyCollidersForAllEntities()
    {
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            entity.DestroyColliders();
        }
    }

    private void RemoveConvertedEntity(IDCLEntity entity) { convertedEntities.Remove(GetConvertedUniqueKeyForEntity(entity)); }

    public void NotifyEntityIsCreated(IDCLEntity entity) { bridge?.AddEntityOnKernel(entity, sceneToEdit); }

    public void UpdateSmartItemComponentInKernel(BIWEntity entityToUpdate) { bridge?.UpdateSmartItemComponent(entityToUpdate, sceneToEdit); }

    public void SetEntityName(BIWEntity entityToApply, string newName) { SetEntityName(entityToApply, newName, true); }

    public void SetEntityName(BIWEntity entityToApply, string newName, bool sendUpdateToKernel = true)
    {
        string currentName = entityToApply.GetDescriptiveName();

        if (currentName == newName)
            return;

        if (entityNameList.Contains(newName))
            newName = GetNewNameForEntity(newName);

        if (entityNameList.Contains(currentName))
            entityNameList.Remove(currentName);

        entityToApply.SetDescriptiveName(newName);
        entityNameList.Add(newName);

        if (sendUpdateToKernel)
            bridge?.ChangedEntityName(entityToApply, sceneToEdit);
    }

    private void ChangeEntityVisibilityStatus(BIWEntity entityToApply)
    {
        entityToApply.ToggleShowStatus();
        if (!entityToApply.IsVisible && selectedEntities.Contains(entityToApply))
            DeselectEntity(entityToApply);
    }

    private void ChangeEntityLockStatus(BIWEntity entityToApply)
    {
        entityToApply.ToggleLockStatus();
        if (entityToApply.IsLocked && selectedEntities.Contains(entityToApply))
            DeselectEntity(entityToApply);

        bridge.ChangeEntityLockStatus(entityToApply, sceneToEdit);
    }

    private string GetConvertedUniqueKeyForEntity(string entityID) { return sceneToEdit.sceneData.id + entityID; }

    private string GetConvertedUniqueKeyForEntity(IDCLEntity entity) { return entity.scene.sceneData.id + entity.entityId; }

    public bool AreAllSelectedEntitiesInsideBoundaries()
    {
        foreach (BIWEntity entity in selectedEntities)
        {
            if (!DCL.Environment.i.world.sceneBoundsChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
            {
                return false;
            }
        }

        return true;
    }

    public bool AreAllEntitiesInsideBoundaries()
    {
        bool areAllIn = true;
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (!DCL.Environment.i.world.sceneBoundsChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
            {
                areAllIn = false;
                break;
            }
        }

        return areAllIn;
    }
}