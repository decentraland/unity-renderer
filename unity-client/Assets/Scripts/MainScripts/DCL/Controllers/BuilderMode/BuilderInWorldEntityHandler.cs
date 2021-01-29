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

public class BuilderInWorldEntityHandler : MonoBehaviour
{
    [Header("Design variables")]
    public float duplicateOffset = 2f;
    public float msBetweenTransformUpdates = 2000;

    [Header("Prefab References")]
    public OutlinerController outlinerController;
    public EntityInformationController entityInformationController;
    public BuilderInWorldController buildModeController;
    public ActionController actionController;
    public BuilderInWorldBridge builderInWorldBridge;

    [Header("Build References")]
    public Material editMaterial;

    public Texture2D duplicateCursorTexture;

    [Header("InputActions")]
    [SerializeField]
    internal InputAction_Trigger hideSelectedEntitiesAction;
    [SerializeField]
    internal InputAction_Trigger showAllEntitiesAction;

    public event Action<DCLBuilderInWorldEntity> onSelectedEntity;
    ParcelScene sceneToEdit;

    Dictionary<string, DCLBuilderInWorldEntity> convertedEntities = new Dictionary<string, DCLBuilderInWorldEntity>();
    List<DCLBuilderInWorldEntity> selectedEntities = new List<DCLBuilderInWorldEntity>();

    BuilderInWorldMode currentActiveMode;
    bool isMultiSelectionActive = false;

    float lastTransformReportTime;

    List<string> entityNameList = new List<string>();

    InputAction_Trigger.Triggered hideSelectedEntitiesDelegate;
    InputAction_Trigger.Triggered showAllEntitiesDelegate;

    private void Start()
    {
        hideSelectedEntitiesDelegate = (action) => ChangeShowStateSelectedEntities();
        showAllEntitiesDelegate = (action) => ShowAllEntities();

        hideSelectedEntitiesAction.OnTriggered += hideSelectedEntitiesDelegate;
        showAllEntitiesAction.OnTriggered += showAllEntitiesDelegate;
    }

    public void Init()
    {
        HUDController.i.builderInWorldMainHud.OnEntityDelete += DeleteEntity;
        HUDController.i.builderInWorldMainHud.OnDuplicateSelectedAction += DuplicateSelectedEntitiesInput;
        HUDController.i.builderInWorldMainHud.OnDeleteSelectedAction += DeleteSelectedEntitiesInput;
        HUDController.i.builderInWorldMainHud.OnEntityClick += ChangeEntitySelectionFromList;
        HUDController.i.builderInWorldMainHud.OnEntityLock += ChangeEntityLockStatus;
        HUDController.i.builderInWorldMainHud.OnEntityChangeVisibility += ChangeEntityVisibilityStatus;
        HUDController.i.builderInWorldMainHud.OnEntityRename += ChangeEntityName;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectPositionChange += UpdateEntityPosition;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectRotationChange += UpdateEntityRotation;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectScaleChange += UpdateEntityScale;

        actionController.OnRedo += ReSelectEntities;
        actionController.OnUndo += ReSelectEntities;
    }

    private void OnDestroy()
    {
        DestroyCollidersForAllEntities();

        actionController.OnRedo -= ReSelectEntities;
        actionController.OnUndo -= ReSelectEntities;

        if (HUDController.i.builderInWorldMainHud == null) return;

        HUDController.i.builderInWorldMainHud.OnEntityDelete -= DeleteEntity;
        HUDController.i.builderInWorldMainHud.OnDuplicateSelectedAction -= DuplicateSelectedEntitiesInput;
        HUDController.i.builderInWorldMainHud.OnDeleteSelectedAction -= DeleteSelectedEntitiesInput;
        HUDController.i.builderInWorldMainHud.OnEntityClick -= ChangeEntitySelectionFromList;
        HUDController.i.builderInWorldMainHud.OnEntityLock -= ChangeEntityLockStatus;
        HUDController.i.builderInWorldMainHud.OnEntityChangeVisibility -= ChangeEntityVisibilityStatus;
        HUDController.i.builderInWorldMainHud.OnEntityChangeVisibility -= ChangeEntityVisibilityStatus;
        HUDController.i.builderInWorldMainHud.OnEntityRename -= ChangeEntityName;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectPositionChange -= UpdateEntityPosition;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectRotationChange -= UpdateEntityRotation;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectScaleChange -= UpdateEntityScale;

        hideSelectedEntitiesAction.OnTriggered -= hideSelectedEntitiesDelegate;
        showAllEntitiesAction.OnTriggered -= showAllEntitiesDelegate;
    }

    private void Update()
    {
        if (selectedEntities.Count <= 0) return;
        if ((DCLTime.realtimeSinceStartup - lastTransformReportTime) <= BuilderInWorldSettings.ENTITY_POSITION_REPORTING_DELAY) return;

        ReportTransform();
    }

    void UpdateEntityPosition(Vector3 position)
    {
        if (selectedEntities.Count <= 0)
            return;

        selectedEntities[0].transform.position = DCL.Environment.i.world.state.ConvertSceneToUnityPosition(position, sceneToEdit);
    }

    void UpdateEntityRotation(Vector3 rotation)
    {
        if (selectedEntities.Count <= 0)
            return;

        selectedEntities[0].transform.rotation = Quaternion.Euler(rotation);
    }

    void UpdateEntityScale(Vector3 scale)
    {
        if (selectedEntities.Count <= 0)
            return;

        selectedEntities[0].transform.localScale = scale;
    }

    void ReportTransform()
    {
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            builderInWorldBridge.EntityTransformReport(entity.rootEntity, sceneToEdit);
        }

        lastTransformReportTime = DCLTime.realtimeSinceStartup;
    }

    public ParcelScene GetParcelSceneToEdit()
    {
        return sceneToEdit;
    }

    public List<DCLBuilderInWorldEntity> GetSelectedEntityList()
    {
        return selectedEntities;
    }

    public bool IsAnyEntitySelected()
    {
        return selectedEntities.Count > 0;
    }

    public void SetActiveMode(BuilderInWorldMode buildMode)
    {
        currentActiveMode = buildMode;
        DeselectEntities();
    }

    public void SetMultiSelectionActive(bool isActive)
    {
        isMultiSelectionActive = isActive;
    }

    public void EnterEditMode(ParcelScene sceneToEdit)
    {
        this.sceneToEdit = sceneToEdit;
        SetupAllEntities();
        EntityListChanged();
    }

    public bool IsPointerInSelectedEntity()
    {
        DCLBuilderInWorldEntity entityInPointer = buildModeController.GetEntityOnPointer();
        if (entityInPointer == null)
            return false;

        foreach(DCLBuilderInWorldEntity entity in selectedEntities)
        {
            if (entityInPointer == entity)
                return true;
        }

        return false;
    }

    void DeleteSelectedEntitiesInput()
    {
        if (selectedEntities.Count > 0)
            DeletedSelectedEntities();
    }

    void DuplicateSelectedEntitiesInput()
    {
        if (selectedEntities.Count > 0)
            DuplicateSelectedEntities();
    }

    public void ExitFromEditMode()
    {
        DeselectEntities();

        foreach(DCLBuilderInWorldEntity entity in convertedEntities.Values)
        {
            entity.Delete();
        }
        convertedEntities.Clear();
    }

    void ChangeEntitySelectionFromList(DCLBuilderInWorldEntity entityToEdit)
    {
        if (!selectedEntities.Contains(entityToEdit))
            SelectFromList(entityToEdit);
        else
            DeselectEntity(entityToEdit);
    }

    void SelectFromList(DCLBuilderInWorldEntity entityToEdit)
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

    public void DeselectEntity(DCLBuilderInWorldEntity entity)
    {
        if (!selectedEntities.Contains(entity))
            return;

        if (!DCL.Environment.i.world.sceneBoundsChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
        {
            DestroyLastCreatedEntities();
        }

        DCL.Environment.i.world.sceneBoundsChecker.EvaluateEntityPosition(entity.rootEntity);
        DCL.Environment.i.world.sceneBoundsChecker.RemoveEntityToBeChecked(entity.rootEntity);

        entity.Deselect();

        outlinerController.CancelEntityOutline(entity);
        selectedEntities.Remove(entity);
        currentActiveMode.EntityDeselected(entity);
        if (selectedEntities.Count <= 0 && HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.HideEntityInformation();
    }

    public void DeselectEntities()
    {
        if (selectedEntities.Count <= 0) return;

        int amountToDeselect = selectedEntities.Count;
        for (int i = 0; i < amountToDeselect; i++)
        {
            DeselectEntity(selectedEntities[0]);
        }

        currentActiveMode.DeselectedEntities();

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void EntityClicked(DCLBuilderInWorldEntity entityToSelect)
    {
        if (entityToSelect != null)
        {
            if (selectedEntities.Count <= 0)
            {
                ChangeEntitySelectStatus(entityToSelect);
            }
            else
            {
                if (!isMultiSelectionActive)
                {
                    DeselectEntities();
                }
                else
                {
                    ChangeEntitySelectStatus(entityToSelect);
                }
            }
        }
        else if (!isMultiSelectionActive)
        {
            DeselectEntities();
        }
    }

    void ReSelectEntities()
    {
        List<DCLBuilderInWorldEntity> entitiesToReselect = new List<DCLBuilderInWorldEntity>();
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            entitiesToReselect.Add(entity);
        }

        DeselectEntities();

        foreach (DCLBuilderInWorldEntity entity in entitiesToReselect)
        {
            SelectEntity(entity);
        }
    }

    void ChangeEntitySelectStatus(DCLBuilderInWorldEntity entityCliked)
    {
        if (entityCliked.IsSelected)
            DeselectEntity(entityCliked);
        else
            SelectEntity(entityCliked);
    }

    public void ChangeLockStateSelectedEntities()
    {
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            entity.ToggleLockStatus();
        }

        DeselectEntities();
    }

    public void ShowAllEntities()
    {
        foreach (DCLBuilderInWorldEntity entity in convertedEntities.Values)
        {
           if (!entity.IsVisible)
                entity.ToggleShowStatus();
        }
    }

    public void ChangeShowStateSelectedEntities()
    {
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            entity.ToggleShowStatus();
        }
    }

    public void Select(DecentralandEntity decentralandEntity)
    {
        DCLBuilderInWorldEntity entityEditable = GetConvertedEntity(decentralandEntity);
        if (entityEditable == null) return;

        SelectEntity(entityEditable);
    }

    public bool SelectEntity(DCLBuilderInWorldEntity entityEditable)
    {
        if (entityEditable.IsLocked) return false;

        if (entityEditable.IsSelected) return false;

        entityEditable.Select();

        selectedEntities.Add(entityEditable);

        currentActiveMode.SelectedEntity(entityEditable);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.ShowEntityInformation();
            HUDController.i.builderInWorldMainHud.EntityInformationSetEntity(entityEditable, sceneToEdit);
            HUDController.i.builderInWorldMainHud.UpdateSceneLimitInfo();
        }

        outlinerController.CancelAllOutlines();
        return true;
    }

    public List<DCLBuilderInWorldEntity> GetAllVoxelsEntities()
    {
        List<DCLBuilderInWorldEntity> voxelEntities = new List<DCLBuilderInWorldEntity>();
        foreach (DCLBuilderInWorldEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit && entity.isVoxel)
                voxelEntities.Add(entity);
        }

        return voxelEntities;
    }

    public List<DCLBuilderInWorldEntity> GetAllEntitiesFromCurrentScene()
    {
        List<DCLBuilderInWorldEntity> entities = new List<DCLBuilderInWorldEntity>();
        foreach (DCLBuilderInWorldEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit) entities.Add(entity);
        }

        return entities;
    }

    public DCLBuilderInWorldEntity GetEntity(string entityId)
    {
        if (convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(entityId)))
            return convertedEntities[GetConvertedUniqueKeyForEntity(entityId)];
        else
            return null;
    }

    public DCLBuilderInWorldEntity GetConvertedEntity(DecentralandEntity decentralandEntity)
    {
        if (convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(decentralandEntity)))
            return convertedEntities[GetConvertedUniqueKeyForEntity(decentralandEntity)];
        else
            return null;
    }


    public void DuplicateSelectedEntities()
    {
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            if (!DCL.Environment.i.world.sceneBoundsChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
                return;
        }

        BuildInWorldCompleteAction buildAction = new BuildInWorldCompleteAction();
        buildAction.actionType = BuildInWorldCompleteAction.ActionType.CREATE;

        List<BuilderInWorldEntityAction> entityActionList = new List<BuilderInWorldEntityAction>();

        int amount = selectedEntities.Count;
        for (int i = 0; i < amount; i++)
        {
            DecentralandEntity entityDuplicated = DuplicateEntity(selectedEntities[i]);
            BuilderInWorldEntityAction builderInWorldEntityAction = new BuilderInWorldEntityAction(entityDuplicated, entityDuplicated.entityId, BuilderInWorldUtils.ConvertEntityToJSON(entityDuplicated));
            entityActionList.Add(builderInWorldEntityAction);
        }

        currentActiveMode.SetDuplicationOffset(duplicateOffset);
        Cursor.SetCursor(duplicateCursorTexture, Vector2.zero, CursorMode.Auto);


        buildAction.CreateActionType(entityActionList, BuildInWorldCompleteAction.ActionType.CREATE);
        actionController.AddAction(buildAction);
    }

    public DecentralandEntity DuplicateEntity(DCLBuilderInWorldEntity entityToDuplicate)
    {
        DecentralandEntity entity = sceneToEdit.DuplicateEntity(entityToDuplicate.rootEntity);

        BuilderInWorldUtils.CopyGameObjectStatus(entityToDuplicate.gameObject, entity.gameObject, false, false);
        SetupEntityToEdit(entity);
        HUDController.i.builderInWorldMainHud.UpdateSceneLimitInfo();

        NotifyEntityIsCreated(entity);
        EntityListChanged();
        return entity;
    }

    public DecentralandEntity CreateEntityFromJSON(string entityJson)
    {
        EntityData data = BuilderInWorldUtils.ConvertJSONToEntityData(entityJson);

        DecentralandEntity newEntity = sceneToEdit.CreateEntity(data.entityId);


        if (data.transformComponent != null)
        {
            DCLTransform.model.position = data.transformComponent.position;
            DCLTransform.model.rotation = Quaternion.Euler(data.transformComponent.rotation);
            DCLTransform.model.scale = data.transformComponent.scale;
            sceneToEdit.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);
        }

        foreach (ProtocolV2.GenericComponent component in data.components)
        {
            sceneToEdit.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, (CLASS_ID_COMPONENT) component.componentId, component.data);
        }


        foreach (ProtocolV2.GenericComponent component in data.sharedComponents)
        {
            sceneToEdit.SharedComponentAttach(newEntity.entityId, component.classId);
        }


        if (data.nftComponent != null)
        {
            NFTShape nftShape = (NFTShape)sceneToEdit.SharedComponentCreate(data.nftComponent.id, Convert.ToInt32(CLASS_ID.NFT_SHAPE));
            nftShape.model = new NFTShape.Model();
            nftShape.model.color = data.nftComponent.color.ToColor();
            nftShape.model.src = data.nftComponent.src;
            nftShape.model.assetId = data.nftComponent.assetId;

            sceneToEdit.SharedComponentAttach(newEntity.entityId, nftShape.id);
        }

        SetupEntityToEdit(newEntity, true);
        HUDController.i.builderInWorldMainHud.UpdateSceneLimitInfo();
        EntityListChanged();
        return newEntity;
    }

    public DCLBuilderInWorldEntity CreateEmptyEntity(ParcelScene parcelScene, Vector3 entryPoint, Vector3 editionGOPosition)
    {
        DecentralandEntity newEntity = parcelScene.CreateEntity(Guid.NewGuid().ToString());

        DCLTransform.model.position = Environment.i.world.state.ConvertUnityToScenePosition(entryPoint, parcelScene);

        Vector3 pointToLookAt = Camera.main.transform.position;
        pointToLookAt.y = editionGOPosition.y;
        Quaternion lookOnLook = Quaternion.LookRotation(editionGOPosition - pointToLookAt);

        DCLTransform.model.rotation = lookOnLook;
        DCLTransform.model.scale = newEntity.gameObject.transform.lossyScale;

        parcelScene.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);

        DCLBuilderInWorldEntity convertedEntity = SetupEntityToEdit(newEntity, true);
        HUDController.i.builderInWorldMainHud.UpdateSceneLimitInfo();

        EntityListChanged();
        return convertedEntity;
    }

    public void SetupAllEntities()
    {
        foreach (DecentralandEntity entity in sceneToEdit.entities.Values)
        {
            SetupEntityToEdit(entity);
        }
    }

    public void DestroyLastCreatedEntities()
    {
        List<DCLBuilderInWorldEntity> entitiesToRemove = new List<DCLBuilderInWorldEntity>();
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            if (entity.IsSelected && entity.IsNew)
                entitiesToRemove.Add(entity);
        }

        buildModeController.UndoEditionGOLastStep();

        foreach (DCLBuilderInWorldEntity entity in entitiesToRemove)
        {
            DeleteEntity(entity, false);
        }
    }

    void EntityListChanged()
    {
        HUDController.i.builderInWorldMainHud.SetEntityList(GetEntitiesInCurrentScene());
    }

    List<DCLBuilderInWorldEntity> GetEntitiesInCurrentScene()
    {
        List<DCLBuilderInWorldEntity> currentEntitiesInScene = new List<DCLBuilderInWorldEntity>();
        foreach (DCLBuilderInWorldEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
                currentEntitiesInScene.Add(entity);
        }

        return currentEntitiesInScene;
    }

    DCLBuilderInWorldEntity SetupEntityToEdit(DecentralandEntity entity, bool hasBeenCreated = false)
    {
        if (!convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(entity)))
        {
            DCLBuilderInWorldEntity entityToEdit = Utils.GetOrCreateComponent<DCLBuilderInWorldEntity>(entity.gameObject);
            entityToEdit.Init(entity, editMaterial);
            convertedEntities.Add(entityToEdit.entityUniqueId, entityToEdit);
            entity.OnRemoved += RemoveConvertedEntity;
            entityToEdit.IsNew = hasBeenCreated;

            string entityName = entityToEdit.GetDescriptiveName();
            if (!string.IsNullOrEmpty(entityName))
                entityNameList.Add(entityName);

            return entityToEdit;
        }
        else
        {
            return convertedEntities[GetConvertedUniqueKeyForEntity(entity)];
        }
    }

    public string GetNewNameForEntity(SceneObject sceneObject)
    {
        int i = 1;
        string name = sceneObject.name;
        if (!entityNameList.Contains(name))
        {
            entityNameList.Add(name);
            return name;
        }

        string newName = name + " " + i;
        while (entityNameList.Contains(newName))
        {
            i++;
            newName = name + " " + i;
        }

        entityNameList.Add(newName);
        return newName;
    }

    public void DeleteFloorEntities()
    {
        List<DCLBuilderInWorldEntity> entitiesToDelete = new List<DCLBuilderInWorldEntity>();

        foreach(DCLBuilderInWorldEntity entity in convertedEntities.Values)
        {
            if(entity.isFloor)
            {
                entitiesToDelete.Add(entity);
            }
        }
        foreach (DCLBuilderInWorldEntity entity in entitiesToDelete)
            DeleteEntity(entity);
    }

    public void DeleteEntity(string entityId)
    {
        DCLBuilderInWorldEntity entity = convertedEntities[GetConvertedUniqueKeyForEntity(entityId)];
        DeleteEntity(entity, true);
    }

    public void DeleteEntity(DCLBuilderInWorldEntity entityToDelete)
    {
        DeleteEntity(entityToDelete, true);
    }

    public void DeleteEntity(DCLBuilderInWorldEntity entityToDelete, bool checkSelection = true)
    {
        if (entityToDelete.IsSelected && checkSelection)
            DeselectEntity(entityToDelete);

        if (selectedEntities.Contains(entityToDelete))
            selectedEntities.Remove(entityToDelete);
        RemoveConvertedEntity(entityToDelete.rootEntity);
        entityToDelete.rootEntity.OnRemoved -= RemoveConvertedEntity;
        entityToDelete.Delete();
        string idToRemove = entityToDelete.rootEntity.entityId;
        Destroy(entityToDelete);
        sceneToEdit.RemoveEntity(idToRemove, true);
        HUDController.i.builderInWorldMainHud.UpdateSceneLimitInfo();
        HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        EntityListChanged();
        builderInWorldBridge.RemoveEntityOnKernel(idToRemove, sceneToEdit);
    }

    public void DeletedSelectedEntities()
    {
        List<DCLBuilderInWorldEntity> entitiesToRemove = new List<DCLBuilderInWorldEntity>();

        for (int i = 0; i < selectedEntities.Count; i++)
        {
            entitiesToRemove.Add(selectedEntities[i]);
        }

        actionController.CreateActionEntityDeleted(entitiesToRemove);

        DeselectEntities();

        foreach (DCLBuilderInWorldEntity entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }
    }

    public void DeleteEntitiesOutsideSceneBoundaries()
    {
        List<DCLBuilderInWorldEntity> entitiesToRemove = new List<DCLBuilderInWorldEntity>();
        foreach (DCLBuilderInWorldEntity entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
            {
                if (!DCL.Environment.i.world.sceneBoundsChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
                {
                    entitiesToRemove.Add(entity);
                }
            }
        }

        foreach (DCLBuilderInWorldEntity entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }
    }

    void DestroyCollidersForAllEntities()
    {
        foreach (DCLBuilderInWorldEntity entity in convertedEntities.Values)
        {
            entity.DestroyColliders();
        }
    }

    void RemoveConvertedEntity(DecentralandEntity entity)
    {
        convertedEntities.Remove(GetConvertedUniqueKeyForEntity(entity));
    }

    public void NotifyEntityIsCreated(DecentralandEntity entity)
    {
        builderInWorldBridge.AddEntityOnKernel(entity, sceneToEdit);
    }

    void ChangeEntityName(DCLBuilderInWorldEntity entityToApply)
    {
        builderInWorldBridge.ChangedEntityName(entityToApply, sceneToEdit);
    }

    void ChangeEntityVisibilityStatus(DCLBuilderInWorldEntity entityToApply)
    {
        entityToApply.ToggleShowStatus();
        if (!entityToApply.IsVisible && selectedEntities.Contains(entityToApply))
            DeselectEntity(entityToApply);
    }

    void ChangeEntityLockStatus(DCLBuilderInWorldEntity entityToApply)
    {
        entityToApply.ToggleLockStatus();
        if (entityToApply.IsLocked && selectedEntities.Contains(entityToApply))
            DeselectEntity(entityToApply);

        builderInWorldBridge.ChangeEntityLockStatus(entityToApply, sceneToEdit);
    }

    string GetConvertedUniqueKeyForEntity(string entityID)
    {
        return sceneToEdit.sceneData.id + entityID;
    }

    string GetConvertedUniqueKeyForEntity(DecentralandEntity entity)
    {
        return entity.scene.sceneData.id + entity.entityId;
    }

    bool AreAllSelectedEntitiesInsideBoundaries()
    {
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
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
        foreach (DCLBuilderInWorldEntity entity in convertedEntities.Values)
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