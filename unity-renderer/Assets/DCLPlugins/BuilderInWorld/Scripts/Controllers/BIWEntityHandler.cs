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
using System.Diagnostics.CodeAnalysis;
using DCL.Builder;
using UnityEngine;
using Environment = DCL.Environment;

public class BIWEntityHandler : BIWController, IBIWEntityHandler
{
    private const float DUPLICATE_OFFSET = 2f;

    private IBIWOutlinerController outlinerController;

    private IBIWModeController modeController;
    private IBIWActionController actionController;
    private IBIWCreatorController creatorController;
    private IBIWRaycastController raycastController;
    private IBIWSaveController saveController;

    private BuilderInWorldBridge bridge;
    private Material editMaterial;

    private InputAction_Trigger hideSelectedEntitiesAction;

    private InputAction_Trigger showAllEntitiesAction;

    private readonly Dictionary<string, BIWEntity> convertedEntities = new Dictionary<string, BIWEntity>();
    private readonly List<BIWEntity> selectedEntities = new List<BIWEntity>();

    private IBIWMode currentActiveMode;
    internal bool isMultiSelectionActive = false;
    internal bool isSecondayClickPressed = false;

    private float lastTransformReportTime;

    private List<string> entityNameList = new List<string>();

    private InputAction_Trigger.Triggered hideSelectedEntitiesDelegate;
    private InputAction_Trigger.Triggered showAllEntitiesDelegate;

    private IBuilderEditorHUDController hudController;

    public event Action<BIWEntity> OnEntityDeselected;
    public event Action OnEntitySelected;
    public event Action<List<BIWEntity>> OnDeleteSelectedEntities;
    public event Action<BIWEntity> OnEntityDeleted;

    private BIWEntity lastClickedEntity;
    private float lastTimeEntityClicked;

    public override void Initialize(IContext context)
    {
        base.Initialize(context);
        if ( context.editorContext.editorHUD != null)
        {
            hudController = context.editorContext.editorHUD;
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

        if ( context.sceneReferences.biwBridgeGameObject != null )
            bridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();

        outlinerController = context.editorContext.outlinerController;

        modeController = context.editorContext.modeController;
        actionController = context.editorContext.actionController;
        creatorController = context.editorContext.creatorController;
        raycastController = context.editorContext.raycastController;
        saveController = context.editorContext.saveController;

        editMaterial = context.projectReferencesAsset.editMaterial;

        hideSelectedEntitiesAction = context.inputsReferencesAsset.hideSelectedEntitiesAction;
        showAllEntitiesAction = context.inputsReferencesAsset.showAllEntitiesAction;

        hideSelectedEntitiesDelegate = (action) => ChangeShowStateSelectedEntities();
        showAllEntitiesDelegate = (action) => ShowAllEntities();

        hideSelectedEntitiesAction.OnTriggered += hideSelectedEntitiesDelegate;
        showAllEntitiesAction.OnTriggered += showAllEntitiesDelegate;

        actionController.OnRedo += ReSelectEntities;
        actionController.OnUndo += ReSelectEntities;
    }

    internal void OnInputMouseDown(int buttonId, Vector3 mousePosition)
    {
        if (buttonId == 1)
            isSecondayClickPressed = true;
    }

    internal void OnInputMouseUp(int buttonId, Vector3 mousePosition)
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

            if ( bridge != null )
                bridge.EntityTransformReport(entity.rootEntity, sceneToEdit);
            entity.PositionReported();
            entity.ScaleReported();
            entity.RotationReported();
        }

        lastTransformReportTime = DCLTime.realtimeSinceStartup;
    }

    public float GetLastTimeReport() { return lastTransformReportTime; }

    public List<BIWEntity> GetSelectedEntityList() { return selectedEntities; }

    public bool IsAnyEntitySelected() { return selectedEntities.Count > 0; }

    public void SetActiveMode(IBIWMode buildMode)
    {
        currentActiveMode = buildMode;
        DeselectEntities();
    }

    public void SetMultiSelectionActive(bool isActive) { isMultiSelectionActive = isActive; }

    public override void EnterEditMode(IBuilderScene scene)
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
        BIWEntity entityInPointer = raycastController.GetEntityOnPointer();
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

    internal void ChangeEntitySelectionFromList(BIWEntity entityToEdit)
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

    public void EntityClicked(BIWEntity entityToSelect)
    {
        if (entityToSelect != null)
        {
            if (!isMultiSelectionActive)
                DeselectEntities();

            if (!entityToSelect.isLocked)
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

    internal void ReSelectEntities()
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

    internal void ChangeEntitySelectStatus(BIWEntity entityCliked)
    {
        if (entityCliked.isSelected)
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
            if (!entity.isVisible)
                entity.ToggleShowStatus();
        }
    }

    public void ChangeShowStateSelectedEntities()
    {
        List<BIWEntity> entitiesToHide = new List<BIWEntity>(selectedEntities);

        foreach (BIWEntity entity in entitiesToHide)
        {
            if (entity.isVisible && entity.isSelected)
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
        if (entityEditable.isLocked)
            return false;

        if (entityEditable.isSelected)
            return false;

        entityEditable.Select();

        selectedEntities.Add(entityEditable);

        currentActiveMode?.SelectedEntity(entityEditable);

        if ( context.editorContext.editorHUD != null)
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

    public BIWEntity GetConvertedEntity(long entityId)
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
        BIWCompleteAction buildAction = new BIWCompleteAction();
        buildAction.actionType = IBIWCompleteAction.ActionType.CREATE;

        List<BIWEntityAction> entityActionList = new List<BIWEntityAction>();
        List<BIWEntity> entitiesToDuplicate = new List<BIWEntity>(selectedEntities);
        DeselectEntities();

        foreach (BIWEntity entityToDuplicate in entitiesToDuplicate)
        {
            if (entityToDuplicate.isNFT)
                continue;

            var entityDuplicated = DuplicateEntity(entityToDuplicate);
            
            // We move the entity before completing the action to save its position too
            entityDuplicated.rootEntity.gameObject.transform.position += Vector3.right * DUPLICATE_OFFSET;
            
            BIWEntityAction biwEntityAction = new BIWEntityAction(entityDuplicated.rootEntity, entityDuplicated.rootEntity.entityId, BIWUtils.ConvertEntityToJSON(entityDuplicated.rootEntity));
            entityActionList.Add(biwEntityAction);
            SelectEntity(entityDuplicated);
        }

        buildAction.CreateActionType(entityActionList, IBIWCompleteAction.ActionType.CREATE);
        actionController.AddAction(buildAction);
    }

    public BIWEntity DuplicateEntity(BIWEntity entityToDuplicate)
    {
        IDCLEntity entity = BIWUtils.DuplicateEntity(sceneToEdit, entityToDuplicate.rootEntity);
        //Note: If the entity contains the name component or DCLLockedOnEdit, we don't want to copy them 
        sceneToEdit.componentsManagerLegacy.RemoveSharedComponent(entity, typeof(DCLName), false);
        sceneToEdit.componentsManagerLegacy.RemoveSharedComponent(entity, typeof(DCLLockedOnEdit), false);

        BIWUtils.CopyGameObjectStatus(entityToDuplicate.rootEntity.gameObject, entity.gameObject, false, false);
        BIWEntity convertedEntity = SetupEntityToEdit(entity);

        NotifyEntityIsCreated(entity);
        EntityListChanged();
        return convertedEntity;
    }

    public IDCLEntity CreateEntityFromJSON(string entityJson)
    {
        EntityData data = BIWUtils.ConvertJSONToEntityData(entityJson);

        IDCLEntity newEntity = sceneToEdit.CreateEntity(data.entityId);

        if (data.transformComponent != null)
        {
            DCLTransform.Model model = new DCLTransform.Model();
            model.position = data.transformComponent.position;
            model.rotation = Quaternion.Euler(data.transformComponent.rotation);
            model.scale = data.transformComponent.scale;

            EntityComponentsUtils.AddTransformComponent(sceneToEdit, newEntity, model);
        }

        foreach (ProtocolV2.GenericComponent component in data.components)
        {
            sceneToEdit.componentsManagerLegacy.EntityComponentCreateOrUpdate(newEntity.entityId, (CLASS_ID_COMPONENT) component.componentId, component.data);
        }

        foreach (ProtocolV2.GenericComponent component in data.sharedComponents)
        {
            sceneToEdit.componentsManagerLegacy.SceneSharedComponentAttach(newEntity.entityId, component.classId);
        }

        if (data.nftComponent != null)
        {
            NFTShape.Model model = new NFTShape.Model();
            model.color = data.nftComponent.color.ToColor();
            model.src = data.nftComponent.src;
            model.assetId = data.nftComponent.assetId;

            EntityComponentsUtils.AddNFTShapeComponent(sceneToEdit, newEntity, model, data.nftComponent.id);
        }

        var convertedEntity = SetupEntityToEdit(newEntity, true);
        
        if(!convertedEntity.isLoaded)
            creatorController.CreateLoadingObject(convertedEntity);

        var rootEntity = convertedEntity.rootEntity;
        if (sceneToEdit.componentsManagerLegacy.TryGetSharedComponent(rootEntity, CLASS_ID.GLTF_SHAPE, out var gltfComponent))
            gltfComponent.CallWhenReady(convertedEntity.ShapeLoadFinish);

        if (sceneToEdit.componentsManagerLegacy.TryGetSharedComponent(rootEntity, CLASS_ID.NFT_SHAPE, out var nftComponent))
            nftComponent.CallWhenReady(convertedEntity.ShapeLoadFinish);
        
        EntityListChanged();
        return newEntity;
    }

    public BIWEntity CreateEmptyEntity(IParcelScene parcelScene, Vector3 entryPoint, Vector3 editionGOPosition, bool notifyEntityList = true)
    {
        var sceneController = Environment.i.world.sceneController;
        IDCLEntity newEntity =
            parcelScene.CreateEntity(
                sceneController.entityIdHelper.EntityFromLegacyEntityString(Guid.NewGuid().ToString()));
        DCLTransform.Model transformModel = new DCLTransform.Model();
        transformModel.position = WorldStateUtils.ConvertUnityToScenePosition(entryPoint, parcelScene);

        Camera camera = context.sceneReferences.mainCamera;
        Vector3 pointToLookAt = camera != null ? camera.transform.position : Vector3.zero;
        pointToLookAt.y = editionGOPosition.y;
        Quaternion lookOnLook = Quaternion.LookRotation(editionGOPosition - pointToLookAt);

        transformModel.rotation = lookOnLook;
        transformModel.scale = newEntity.gameObject.transform.lossyScale;

        EntityComponentsUtils.AddTransformComponent(parcelScene, newEntity, transformModel);

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
            if (entity.isSelected && entity.isNew)
                entitiesToRemove.Add(entity);
        }

        if (entitiesToRemove.Count == 0)
            return;

        modeController.UndoEditionGOLastStep();

        foreach (BIWEntity entity in entitiesToRemove)
        {
            DeleteEntity(entity, false);
        }

        saveController.TryToSave();
        hudController?.HideEntityInformation();
    }

    public void EntityListChanged()
    {
        if ( context.editorContext.editorHUD == null)
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
        string biwEntityId = GetConvertedUniqueKeyForEntity(entity);

        if (convertedEntities.ContainsKey(biwEntityId))
            return convertedEntities[biwEntityId];

        BIWEntity entityToEdit = new BIWEntity();
        entityToEdit.Initialize(entity, editMaterial);
        convertedEntities.Add(entityToEdit.entityUniqueId, entityToEdit);
        entity.OnRemoved += RemoveConvertedEntity;
        entityToEdit.isNew = hasBeenCreated;

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

    internal void ChangeEntityBoundsCheckerStatus(IDCLEntity entity, bool isInsideBoundaries)
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
        
        saveController.TryToSave();
    }

    public void DeleteEntity(long entityId)
    {
        BIWEntity entity = convertedEntities[GetConvertedUniqueKeyForEntity(entityId)];
        DeleteEntity(entity, true);
    }

    public void DeleteEntity(BIWEntity entityToDelete) { DeleteEntity(entityToDelete, true); }

    public void DeleteEntity(BIWEntity entityToDelete, bool checkSelection)
    {
        if (entityToDelete.isSelected && checkSelection)
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
        long idToRemove = entityToDelete.rootEntity.entityId;
        OnEntityDeleted?.Invoke(entityToDelete);
        creatorController.RemoveLoadingObjectInmediate(entityToDelete.rootEntity.entityId);
        if (sceneToEdit.entities.ContainsKey(idToRemove))
            sceneToEdit.RemoveEntity(idToRemove, true);

        hudController?.RefreshCatalogAssetPack();
        EntityListChanged();

        if ( bridge != null )
            bridge.RemoveEntityOnKernel(idToRemove, sceneToEdit);
    }

    public void DeleteSingleEntity(BIWEntity entityToDelete)
    {
        actionController.CreateActionEntityDeleted(entityToDelete);
        DeleteEntity(entityToDelete, true);
        saveController.TryToSave();
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
        saveController.TryToSave();
    }

    public void DeleteEntitiesOutsideSceneBoundaries()
    {
        List<BIWEntity> entitiesToRemove = new List<BIWEntity>();
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
            {
                if (!DCL.Environment.i.world.sceneBoundsChecker.IsEntityMeshInsideSceneBoundaries(entity.rootEntity))
                {
                    entitiesToRemove.Add(entity);
                }
            }
        }

        foreach (BIWEntity entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }
        
        saveController.TryToSave();
    }

    private void DestroyCollidersForAllEntities()
    {
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            entity.DestroyColliders();
        }
    }

    private void RemoveConvertedEntity(IDCLEntity entity) { convertedEntities.Remove(GetConvertedUniqueKeyForEntity(entity)); }

    public void NotifyEntityIsCreated(IDCLEntity entity)
    {
        if (bridge != null)
            bridge.AddEntityOnKernel(entity, sceneToEdit);
    }

    [ExcludeFromCodeCoverage]
    public void UpdateSmartItemComponentInKernel(BIWEntity entityToUpdate)
    {
        if ( bridge != null )
            bridge.UpdateSmartItemComponent(entityToUpdate, sceneToEdit);
    }

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

        if (sendUpdateToKernel && bridge != null)
            bridge.ChangedEntityName(entityToApply, sceneToEdit);
    }

    internal void ChangeEntityVisibilityStatus(BIWEntity entityToApply)
    {
        entityToApply.ToggleShowStatus();
        if (!entityToApply.isVisible && selectedEntities.Contains(entityToApply))
            DeselectEntity(entityToApply);
    }

    internal void ChangeEntityLockStatus(BIWEntity entityToApply)
    {
        entityToApply.ToggleLockStatus();
        if (entityToApply.isLocked && selectedEntities.Contains(entityToApply))
            DeselectEntity(entityToApply);

        if (bridge != null)
            bridge.ChangeEntityLockStatus(entityToApply, sceneToEdit);
    }

    private string GetConvertedUniqueKeyForEntity(long entityID) { return sceneToEdit.sceneData.id + entityID; }

    private string GetConvertedUniqueKeyForEntity(IDCLEntity entity) { return entity.scene.sceneData.id + entity.entityId; }

    public bool AreAllEntitiesInsideBoundaries()
    {
        bool areAllIn = true;
        foreach (BIWEntity entity in convertedEntities.Values)
        {
            if (!DCL.Environment.i.world.sceneBoundsChecker.IsEntityMeshInsideSceneBoundaries(entity.rootEntity))
            {
                areAllIn = false;
                break;
            }
        }

        return areAllIn;
    }
}