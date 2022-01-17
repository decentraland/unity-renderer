using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DCL.Builder;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

public class BIWMode : IBIWMode
{
    public event Action OnInputDone;
    public event Action<BIWCompleteAction> OnActionGenerated;

    public bool isSnapActive => isSnapActiveValue;
    public float maxDistanceToSelectEntities => maxDistanceToSelectEntitiesValue;

    protected float maxDistanceToSelectEntitiesValue = 50;

    //Note: Snap variables are set in each mode 
    protected float snapFactor = 1f;
    protected float snapRotationDegresFactor = 15f;
    protected float snapScaleFactor = 0.5f;
    protected float snapDistanceToActivateMovement = 10f;

    protected IBIWEntityHandler entityHandler;
    protected IBIWSaveController saveController;
    protected IBIWActionController actionController;
    internal IBIWRaycastController raycastController;

    protected GameObject editionGO, undoGO, snapGO, freeMovementGO;

    internal bool isSnapActiveValue = false;
    internal bool isModeActive = false;
    internal bool isMultiSelectionActive = false;
    internal List<BIWEntity> selectedEntities = new List<BIWEntity>();

    internal bool isNewObjectPlaced = false;

    internal List<BIWEntityAction> actionList = new List<BIWEntityAction>();

    internal IContext context;

    public virtual void Init(IContext context)
    {
        this.context = context;
        entityHandler = context.editorContext.entityHandler;
        saveController = context.editorContext.saveController;
        actionController = context.editorContext.actionController;
        raycastController = context.editorContext.raycastController;
        entityHandler.OnEntityDeleted += OnDeleteEntity;
    }

    public virtual void SetEditorReferences(GameObject goToEdit, GameObject undoGO, GameObject snapGO, GameObject freeMovementGO, List<BIWEntity> selectedEntities)
    {
        editionGO = goToEdit;
        this.undoGO = undoGO;
        this.snapGO = snapGO;
        this.freeMovementGO = freeMovementGO;

        this.selectedEntities = selectedEntities;
    }

    public virtual void Dispose() { entityHandler.OnEntityDeleted -= OnDeleteEntity; }

    public bool IsActive() { return isModeActive; }
    public virtual void Activate(IParcelScene scene) { isModeActive = true; }

    public virtual void Deactivate()
    {
        isModeActive = false;
        entityHandler.DeselectEntities();
    }

    public virtual void SetSnapActive(bool isActive)
    {
        if (isActive && !isSnapActiveValue)
            AudioScriptableObjects.enable.Play();
        else if (!isActive && isSnapActiveValue)
            AudioScriptableObjects.disable.Play();

        isSnapActiveValue = isActive;
        context.editorContext.editorHUD?.SetSnapModeActive(isSnapActiveValue);
    }

    public virtual void StartMultiSelection() { isMultiSelectionActive = true; }

    public virtual Vector3 GetPointerPosition() { return Input.mousePosition; }

    public virtual void EndMultiSelection() { isMultiSelectionActive = false; }

    public virtual bool ShouldCancelUndoAction() { return false; }

    public virtual void SetDuplicationOffset(float offset) { }

    public virtual void EntityDoubleClick(BIWEntity entity) { }

    public virtual void SelectedEntity(BIWEntity selectedEntity)
    {
        CenterGameObjectToEdit();

        BIWUtils.CopyGameObjectStatus(editionGO, undoGO, false, false);
    }

    public virtual void CenterGameObjectToEdit()
    {
        if (selectedEntities.Count > 0)
        {
            foreach (BIWEntity entity in selectedEntities)
            {
                entity.rootEntity.gameObject.transform.SetParent(null);
            }

            editionGO.transform.position = GetCenterPointOfSelectedObjects();
            editionGO.transform.rotation = Quaternion.Euler(0, 0, 0);
            editionGO.transform.localScale = Vector3.one;
            foreach (BIWEntity entity in selectedEntities)
            {
                entity.rootEntity.gameObject.transform.SetParent(editionGO.transform);
            }
        }
    }

    public virtual void MouseClickDetected()
    {
        BIWEntity entityToSelect = raycastController.GetEntityOnPointer();
        if (entityToSelect != null)
        {
            entityHandler.EntityClicked(entityToSelect);
        }
        else if (!isMultiSelectionActive)
        {
            entityHandler.DeselectEntities();
        }
    }

    public virtual void CreatedEntity(BIWEntity createdEntity) { isNewObjectPlaced = true; }

    public virtual void EntityDeselected(BIWEntity entityDeselected)
    {
        CenterGameObjectToEdit();

        if (isNewObjectPlaced)
        {
            actionController.CreateActionEntityCreated(entityDeselected.rootEntity);
        }

        isNewObjectPlaced = false;
    }

    public virtual void OnDeleteEntity(BIWEntity entity) { }

    public virtual void OnDeselectedEntities() { entityHandler.ReportTransform(true); }

    public virtual void CheckInput() { }

    public virtual void CheckInputSelectedEntities() { }

    public virtual void InputDone() { OnInputDone?.Invoke(); }

    public virtual void ResetScaleAndRotation()
    {
        editionGO.transform.localScale = Vector3.one;
        snapGO.transform.localScale = Vector3.one;
        freeMovementGO.transform.localScale = Vector3.one;

        Quaternion zeroAnglesQuaternion = Quaternion.Euler(Vector3.zero);

        snapGO.transform.rotation = zeroAnglesQuaternion;
        freeMovementGO.transform.rotation = zeroAnglesQuaternion;
        editionGO.transform.rotation = zeroAnglesQuaternion;

        foreach (BIWEntity decentralandEntityToEdit in selectedEntities)
        {
            decentralandEntityToEdit.ResetTransfrom();
        }

        CenterGameObjectToEdit();
    }

    public virtual Vector3 GetCreatedEntityPoint() { return Vector3.zero; }

    protected Vector3 GetCenterPointOfSelectedObjects()
    {
        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;
        foreach (BIWEntity entity in selectedEntities)
        {
            totalX += entity.rootEntity.gameObject.transform.position.x;
            totalY += entity.rootEntity.gameObject.transform.position.y;
            totalZ += entity.rootEntity.gameObject.transform.position.z;
        }

        float centerX = totalX / selectedEntities.Count;
        float centerY = totalY / selectedEntities.Count;
        float centerZ = totalZ / selectedEntities.Count;
        return new Vector3(centerX, centerY, centerZ);
    }

    protected void TransformActionStarted(IDCLEntity entity, string type)
    {
        BIWEntityAction buildModeEntityAction = new BIWEntityAction(entity);
        switch (type)
        {
            case BIWSettings.TRANSLATE_GIZMO_NAME:
                buildModeEntityAction.oldValue = entity.gameObject.transform.position;
                break;
            case BIWSettings.ROTATE_GIZMO_NAME:
                buildModeEntityAction.oldValue = entity.gameObject.transform.rotation.eulerAngles;
                break;
            case BIWSettings.SCALE_GIZMO_NAME:
                buildModeEntityAction.oldValue = entity.gameObject.transform.lossyScale;
                break;
        }

        actionList.Add(buildModeEntityAction);
    }

    protected void TransformActionEnd(IDCLEntity entity, string type)
    {
        List<BIWEntityAction> removeList = new List<BIWEntityAction>();
        foreach (BIWEntityAction entityAction in actionList)
        {
            if (entityAction.entityId != entity.entityId)
                continue;

            switch (type)
            {
                case "MOVE":

                    entityAction.newValue = entity.gameObject.transform.position;
                    if (Vector3.Distance((Vector3) entityAction.oldValue, (Vector3) entityAction.newValue) <= 0.09f)
                        removeList.Add(entityAction);
                    break;
                case "ROTATE":

                    entityAction.newValue = entity.gameObject.transform.rotation.eulerAngles;
                    if (Vector3.Distance((Vector3) entityAction.oldValue, (Vector3) entityAction.newValue) <= 0.09f)
                        removeList.Add(entityAction);
                    break;
                case "SCALE":
                    entityAction.newValue = entity.gameObject.transform.lossyScale;
                    if (Vector3.Distance((Vector3) entityAction.oldValue, (Vector3) entityAction.newValue) <= 0.09f)
                        removeList.Add(entityAction);
                    break;
            }
        }

        foreach (BIWEntityAction entityAction in removeList)
        {
            actionList.Remove(entityAction);
        }
    }

    protected void ActionFinish(IBIWCompleteAction.ActionType type)
    {
        if (actionList.Count > 0 && selectedEntities.Count > 0)
        {
            BIWCompleteAction buildModeAction = new BIWCompleteAction();

            buildModeAction.actionType = type;
            buildModeAction.CreateActionType(actionList, type);
            OnActionGenerated?.Invoke(buildModeAction);

            actionList = new List<BIWEntityAction>();
        }
    }
    public virtual void Update() { }
    public virtual void OnGUI() { }
    public virtual void LateUpdate() { }
}