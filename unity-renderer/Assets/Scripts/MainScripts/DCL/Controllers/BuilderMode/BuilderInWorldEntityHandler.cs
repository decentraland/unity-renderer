using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInWorldEntityHandler : MonoBehaviour
{

    [Header("Design variables")]
    public float duplicateOffset = 2f;

    [Header("Prefab References")]
    public OutlinerController outlinerController;
    public EntityInformationController entityInformationController;
    public BuildModeController buildModeController;
    public ActionController actionController;
    public BuilderInWorldBridge builderInWorldBridge;

    [Header("Build References")]
    public Material editMaterial;
    public Texture2D duplicateCursorTexture;

    public event Action<DecentralandEntityToEdit> onSelectedEntity;
    ParcelScene sceneToEdit;

    Dictionary<string, DecentralandEntityToEdit> convertedEntities = new Dictionary<string, DecentralandEntityToEdit>();
    List<DecentralandEntityToEdit> selectedEntities = new List<DecentralandEntityToEdit>();

    BuildMode currentActiveMode;
    bool isMultiSelectionActive = false;

    private void OnDestroy()
    {
        DestroyCollidersForAllEntities();

        actionController.OnRedo -= ReSelectEntities;
        actionController.OnUndo -= ReSelectEntities;

        if (HUDController.i.buildModeHud == null) return;

        HUDController.i.buildModeHud.OnEntityDelete -= DeleteEntity;
        HUDController.i.buildModeHud.OnDuplicateSelectedAction -= DuplicateSelectedEntitiesInput;
        HUDController.i.buildModeHud.OnDeleteSelectedAction -= DeleteSelectedEntitiesInput;
        HUDController.i.buildModeHud.OnEntityClick -= ChangeEntitySelectionFromList;
        HUDController.i.buildModeHud.OnEntityLock -= ChangeEntityLockStatus;
        HUDController.i.buildModeHud.OnEntityChangeVisibility -= ChangeEntityVisibilityStatus;
     
    }

    public void Init()
    {
        HUDController.i.buildModeHud.OnEntityDelete += DeleteEntity;
        HUDController.i.buildModeHud.OnDuplicateSelectedAction += DuplicateSelectedEntitiesInput;
        HUDController.i.buildModeHud.OnDeleteSelectedAction += DeleteSelectedEntitiesInput;
        HUDController.i.buildModeHud.OnEntityClick += ChangeEntitySelectionFromList;
        HUDController.i.buildModeHud.OnEntityLock += ChangeEntityLockStatus;
        HUDController.i.buildModeHud.OnEntityChangeVisibility += ChangeEntityVisibilityStatus;

        actionController.OnRedo += ReSelectEntities;
        actionController.OnUndo += ReSelectEntities;
    }

    public List<DecentralandEntityToEdit> GetSelectedEntityList()
    {
        return selectedEntities;
    }

    public bool IsAnyEntitySelected()
    {
        return selectedEntities.Count > 0;
    }

    public void SetActiveMode(BuildMode buildMode)
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

    void ChangeEntitySelectionFromList(DecentralandEntityToEdit entityToEdit)
    {
        if (!selectedEntities.Contains(entityToEdit))
            SelectFromList(entityToEdit);
        else
            DeselectEntity(entityToEdit);
    }

    void SelectFromList(DecentralandEntityToEdit entityToEdit)
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

    public void DeselectEntity(DecentralandEntityToEdit entity)
    {
        if (!selectedEntities.Contains(entity))
            return;

        if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
        {
            DestroyLastCreatedEntities();
        }

        SceneController.i.boundariesChecker.EvaluateEntityPosition(entity.rootEntity);
        SceneController.i.boundariesChecker.RemoveEntityToBeChecked(entity.rootEntity);
        entity.Deselect();
        outlinerController.CancelEntityOutline(entity);
        selectedEntities.Remove(entity);
        currentActiveMode.EntityDeselected(entity);
        if (selectedEntities.Count <= 0 && entityInformationController != null)
            entityInformationController.Disable();
    }

    public void DeselectEntities()
    {
        if (selectedEntities.Count <= 0) return;

        if (!AreAllSelectedEntitiesInsideBoundaries()) DestroyLastCreatedEntities();

        int amountToDeselect = selectedEntities.Count;
        for (int i = 0; i < amountToDeselect; i++)
        {
            DeselectEntity(selectedEntities[0]);
        }

        currentActiveMode.DeselectedEntities();

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

    }
    
    public void EntityClicked(DecentralandEntityToEdit entityToSelect)
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
        List<DecentralandEntityToEdit> entitiesToReselect = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            entitiesToReselect.Add(entity);
        }
        DeselectEntities();

        foreach (DecentralandEntityToEdit entity in entitiesToReselect)
        {
            SelectEntity(entity);
        }
    }

    void ChangeEntitySelectStatus(DecentralandEntityToEdit entityCliked)
    {
        if (entityCliked.IsSelected)
            DeselectEntity(entityCliked);
        else
            SelectEntity(entityCliked);
    }

    public void ChangeLockStateSelectedEntities()
    {
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            entity.ToggleLockStatus();
        }
        DeselectEntities();
    }

    public void ChangeShowStateSelectedEntities()
    {
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            entity.ToggleShowStatus();
        }
    }

    public void Select(DecentralandEntity decentralandEntity)
    {
        DecentralandEntityToEdit entityEditable = GetConvertedEntity(decentralandEntity);
        if (entityEditable == null) return;

        SelectEntity(entityEditable);
    }

    public bool SelectEntity(DecentralandEntityToEdit entityEditable)
    {

        if (entityEditable.IsLocked) return false;

        if (entityEditable.IsSelected) return false;

        entityEditable.Select();

        selectedEntities.Add(entityEditable);


        currentActiveMode.SelectedEntity(entityEditable);


        if (entityInformationController != null)
        {
            entityInformationController.Enable();
            entityInformationController.SetEntity(entityEditable.rootEntity, sceneToEdit);
        }


        HUDController.i.buildModeHud.UpdateSceneLimitInfo();
        outlinerController.CancelAllOutlines();
        return true;
    }

    public List<DecentralandEntityToEdit> GetAllVoxelsEntities()
    {
        List<DecentralandEntityToEdit> voxelEntities = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit && entity.isVoxel)
                voxelEntities.Add(entity);
        }

        return voxelEntities;
    }

    public List<DecentralandEntityToEdit> GetAllEntitiesFromCurrentScene()
    {
        List<DecentralandEntityToEdit> entities = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit) entities.Add(entity);
        }

        return entities;
    }

    public DecentralandEntityToEdit GetConvertedEntity(DecentralandEntity decentralandEntity)
    {
        if (convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(decentralandEntity)))
            return convertedEntities[GetConvertedUniqueKeyForEntity(decentralandEntity)];
        else
            return null;
    }


    public void DuplicateSelectedEntities()
    {
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
                return;
        }

        int amount = selectedEntities.Count;
        for (int i = 0; i < amount; i++)
        {
            DuplicateEntity(selectedEntities[i]);
        }
        currentActiveMode.SetDuplicationOffset(duplicateOffset);
        Cursor.SetCursor(duplicateCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    public DecentralandEntity DuplicateEntity(DecentralandEntityToEdit entityToDuplicate)
    {
        DecentralandEntity entity = sceneToEdit.DuplicateEntity(entityToDuplicate.rootEntity);

        BuildModeUtils.CopyGameObjectStatus(entityToDuplicate.gameObject, entity.gameObject, false, false);
        SetupEntityToEdit(entity);
        HUDController.i.buildModeHud.UpdateSceneLimitInfo();

        builderInWorldBridge.AddEntityOnKernel(entity,sceneToEdit);
        EntityListChanged();
        return entity;
    }

    public DecentralandEntity CreateEntityFromJSON(string entityJson)
    {
        DecentralandEntity newEntity = JsonConvert.DeserializeObject<DecentralandEntity>(entityJson);
        sceneToEdit.CreateEntity(newEntity.entityId);

        SetupEntityToEdit(newEntity, true);
        HUDController.i.buildModeHud.UpdateSceneLimitInfo();
        EntityListChanged();
        return newEntity;
    }

    public DecentralandEntityToEdit CreateEntity(ParcelScene parcelScene, Vector3 entryPoint, Vector3 editionGOPosition)
    {
        DecentralandEntity newEntity = parcelScene.CreateEntity(Guid.NewGuid().ToString());

        DCLTransform.model.position = SceneController.i.ConvertUnityToScenePosition(entryPoint, parcelScene);

        Vector3 pointToLookAt = Camera.main.transform.position;
        pointToLookAt.y = editionGOPosition.y;
        Quaternion lookOnLook = Quaternion.LookRotation(editionGOPosition - pointToLookAt);

        DCLTransform.model.rotation = lookOnLook;
        DCLTransform.model.scale = newEntity.gameObject.transform.lossyScale;

        parcelScene.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);

        DecentralandEntityToEdit convertedEntity = SetupEntityToEdit(newEntity, true);
        HUDController.i.buildModeHud.UpdateSceneLimitInfo();
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

    void DestroyLastCreatedEntities()
    {
        List<DecentralandEntityToEdit> entitiesToRemove = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            if (entity.IsSelected && entity.IsNew)
                entitiesToRemove.Add(entity);
        }

        buildModeController.UndoEditionGOLastStep();

        foreach (DecentralandEntityToEdit entity in entitiesToRemove)
        {
            DeleteEntity(entity, false);
        }
    }

    void EntityListChanged()
    {
        HUDController.i.buildModeHud.SetEntityList(GetEntitiesInCurrentScene());
    }

    List<DecentralandEntityToEdit> GetEntitiesInCurrentScene()
    {
        List<DecentralandEntityToEdit> currentEntitiesInScene = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
                currentEntitiesInScene.Add(entity);
        }
        return currentEntitiesInScene;
    }

    DecentralandEntityToEdit SetupEntityToEdit(DecentralandEntity entity, bool hasBeenCreated = false)
    {
        if (!convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(entity)))
        {
            DecentralandEntityToEdit entityToEdit = Utils.GetOrCreateComponent<DecentralandEntityToEdit>(entity.gameObject);
            entityToEdit.Init(entity, editMaterial);
            convertedEntities.Add(entityToEdit.entityUniqueId, entityToEdit);
            entity.OnRemoved += RemoveConvertedEntity;
            entityToEdit.IsNew = hasBeenCreated;
            return entityToEdit;
        }
        else
        {
            return convertedEntities[GetConvertedUniqueKeyForEntity(entity)];
        }
    }

    public void DeleteEntity(string entityId)
    {
        DecentralandEntityToEdit entity = convertedEntities[GetConvertedUniqueKeyForEntity(entityId)];
        DeleteEntity(entity, true);
    }

    public void DeleteEntity(DecentralandEntityToEdit entityToDelete)
    {
        DeleteEntity(entityToDelete, true);
    }

    public void DeleteEntity(DecentralandEntityToEdit entityToDelete, bool checkSelection = true)
    {
        if (entityToDelete.IsSelected && checkSelection)
            DeselectEntity(entityToDelete);
        RemoveConvertedEntity(entityToDelete.rootEntity);
        entityToDelete.rootEntity.OnRemoved -= RemoveConvertedEntity;
        entityToDelete.Delete();
        string idToRemove = entityToDelete.rootEntity.entityId;
        Destroy(entityToDelete);
        sceneToEdit.RemoveEntity(idToRemove, true);
        HUDController.i.buildModeHud.UpdateSceneLimitInfo();
        EntityListChanged();
        builderInWorldBridge.RemoveEntityOnKernel(idToRemove, sceneToEdit);
    }

    public void DeletedSelectedEntities()
    {
        List<DecentralandEntityToEdit> entitiesToRemove = new List<DecentralandEntityToEdit>();

        for (int i = 0; i < selectedEntities.Count; i++)
        {
            entitiesToRemove.Add(selectedEntities[i]);
        }

        DeselectEntities();

        foreach (DecentralandEntityToEdit entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }

    }

    public void DeleteEntitiesOutsideSceneBoundaries()
    {
        List<DecentralandEntityToEdit> entitiesToRemove = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
            {
                if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
                {
                    entitiesToRemove.Add(entity);
                }
            }
        }

        foreach (DecentralandEntityToEdit entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }
    }

    void DestroyCollidersForAllEntities()
    {
        foreach (DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            entity.DestroyColliders();
        }
    }

    void RemoveConvertedEntity(DecentralandEntity entity)
    {
        convertedEntities.Remove(GetConvertedUniqueKeyForEntity(entity));
    }

    void ChangeEntityVisibilityStatus(DecentralandEntityToEdit entityToApply)
    {
        entityToApply.ToggleShowStatus();
        if (!entityToApply.IsVisible && selectedEntities.Contains(entityToApply))
            DeselectEntity(entityToApply);
    }

    void ChangeEntityLockStatus(DecentralandEntityToEdit entityToApply)
    {
        entityToApply.ToggleLockStatus();
        if (entityToApply.IsLocked && selectedEntities.Contains(entityToApply))
            DeselectEntity(entityToApply);
    }

    string GetConvertedUniqueKeyForEntity(string entityID)
    {
        return sceneToEdit + entityID;
    }

    string GetConvertedUniqueKeyForEntity(DecentralandEntity entity)
    {
        return entity.scene.sceneData.id + entity.entityId;
    }

    bool AreAllSelectedEntitiesInsideBoundaries()
    {
        bool areAllIn = true;
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
            {
                areAllIn = false;
                break;
            }
        }
        return areAllIn;
    }
}
