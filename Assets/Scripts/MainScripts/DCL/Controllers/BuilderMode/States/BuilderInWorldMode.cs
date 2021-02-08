using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInWorldMode : MonoBehaviour
{

    [Header("Snap variables")]
    public float snapFactor = 1f;
    public float snapRotationDegresFactor = 15f;
    public float snapScaleFactor = 0.5f;

    public float snapDistanceToActivateMovement = 10f;

    [Header("Prefab references")]
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public ActionController actionController;
    public BuilderInWorldBridge builderInWorldBridge;

    public event System.Action OnInputDone;
    public event System.Action<BuildInWorldCompleteAction> OnActionGenerated;

    protected GameObject editionGO, undoGO, snapGO, freeMovementGO;

    protected bool isSnapActive = false, isMultiSelectionActive = false, isModeActive = false;
    protected List<DCLBuilderInWorldEntity> selectedEntities;

    bool isNewObjectPlaced = false;

    protected List<BuilderInWorldEntityAction> actionList = new List<BuilderInWorldEntityAction>();

    public virtual void Init(GameObject goToEdit, GameObject undoGO, GameObject snapGO, GameObject freeMovementGO, List<DCLBuilderInWorldEntity> selectedEntities)
    {
        editionGO = goToEdit;
        this.undoGO = undoGO;
        this.snapGO = snapGO;
        this.freeMovementGO = freeMovementGO;

        this.selectedEntities = selectedEntities;
        gameObject.SetActive(false);

    }

    public virtual void Activate(ParcelScene scene)
    {
        gameObject.SetActive(true);
        isModeActive = true;
    }

    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
        isModeActive = false;
    }

    public virtual void SetSnapActive(bool isActive)
    {
        isSnapActive = isActive;
    }

    public virtual void StartMultiSelection()
    {
        isMultiSelectionActive = true;

    }

    public virtual void EndMultiSelection()
    {
        isMultiSelectionActive = false;

    }

    public virtual bool ShouldCancelUndoAction()
    {
        return false;
    }

    public virtual void SetDuplicationOffset(float offset)
    {

    }

    public virtual void SelectedEntity(DCLBuilderInWorldEntity selectedEntity)
    {
        CenterGameObjectToEdit();

        BuilderInWorldUtils.CopyGameObjectStatus(editionGO, undoGO, false, false);
    }

    public virtual void CenterGameObjectToEdit()
    {
        if (selectedEntities.Count > 0)
        {
            foreach (DCLBuilderInWorldEntity entity in selectedEntities)
            {
                entity.rootEntity.gameObject.transform.SetParent(null);
            }
            editionGO.transform.position = GetCenterPointOfSelectedObjects();
            editionGO.transform.rotation = Quaternion.Euler(0, 0, 0);
            editionGO.transform.localScale = Vector3.one;
            foreach (DCLBuilderInWorldEntity entity in selectedEntities)
            {
                entity.rootEntity.gameObject.transform.SetParent(editionGO.transform);
            }
        }
    }

    public virtual void CreatedEntity(DCLBuilderInWorldEntity createdEntity)
    {
        isNewObjectPlaced = true;
    }

    public virtual void EntityDeselected(DCLBuilderInWorldEntity entityDeselected)
    {
        CenterGameObjectToEdit();

        if (isNewObjectPlaced)
        {
            actionController.CreateActionEntityCreated(entityDeselected.rootEntity);
        }

        isNewObjectPlaced = false;
    }

    public virtual void DeselectedEntities()
    {

    }

    public virtual void CheckInput()
    {

    }

    public virtual void CheckInputSelectedEntities()
    {

    }

    public virtual void InputDone()
    {
        OnInputDone?.Invoke();
    }

    public virtual void ResetScaleAndRotation()
    {
        editionGO.transform.localScale = Vector3.one;
        snapGO.transform.localScale = Vector3.one;
        freeMovementGO.transform.localScale = Vector3.one;

        Quaternion zeroAnglesQuaternion = Quaternion.Euler(Vector3.zero);

        snapGO.transform.rotation = zeroAnglesQuaternion;
        freeMovementGO.transform.rotation = zeroAnglesQuaternion;
        editionGO.transform.rotation = zeroAnglesQuaternion;

        foreach(DCLBuilderInWorldEntity decentralandEntityToEdit in selectedEntities)
        {
            decentralandEntityToEdit.rootEntity.gameObject.transform.eulerAngles = Vector3.zero;
        }

    }

    public virtual Vector3 GetCreatedEntityPoint()
    {
        return Vector3.zero;
    }

    protected Vector3 GetCenterPointOfSelectedObjects()
    {
        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
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

    protected void TransformActionStarted(DecentralandEntity entity, string type)
    {
        BuilderInWorldEntityAction buildModeEntityAction = new BuilderInWorldEntityAction(entity);
        switch (type)
        {
            case "MOVE":
                buildModeEntityAction.oldValue = entity.gameObject.transform.position;
                break;
            case "ROTATE":
                buildModeEntityAction.oldValue = entity.gameObject.transform.rotation.eulerAngles;
                break;
            case "SCALE":
                buildModeEntityAction.oldValue = entity.gameObject.transform.lossyScale;
                break;
        }
        actionList.Add(buildModeEntityAction);


    }
    protected void TransformActionEnd(DecentralandEntity entity, string type)
    {

        List<BuilderInWorldEntityAction> removeList = new List<BuilderInWorldEntityAction>();
        foreach (BuilderInWorldEntityAction entityAction in actionList)
        {
            if (entityAction.entityId != entity.entityId) continue;

            switch (type)
            {
                case "MOVE":

                    entityAction.newValue = entity.gameObject.transform.position;
                    if (Vector3.Distance((Vector3)entityAction.oldValue, (Vector3)entityAction.newValue) <= 0.09f) removeList.Add(entityAction);
                    break;
                case "ROTATE":

                    entityAction.newValue = entity.gameObject.transform.rotation.eulerAngles;
                    if (Vector3.Distance((Vector3)entityAction.oldValue, (Vector3)entityAction.newValue) <= 0.09f) removeList.Add(entityAction);
                    break;
                case "SCALE":
                    entityAction.newValue = entity.gameObject.transform.lossyScale;
                    if (Vector3.Distance((Vector3)entityAction.oldValue, (Vector3)entityAction.newValue) <= 0.09f) removeList.Add(entityAction);
                    break;
            }

        }
        foreach (BuilderInWorldEntityAction entityAction in removeList)
        {
            actionList.Remove(entityAction);
        }
    }

    protected void ActionFinish(BuildInWorldCompleteAction.ActionType type)
    {
        if (actionList.Count > 0 && selectedEntities.Count > 0)
        {
            BuildInWorldCompleteAction buildModeAction = new BuildInWorldCompleteAction();

            buildModeAction.actionType = type;
            buildModeAction.CreateActionType(actionList, type);
            OnActionGenerated?.Invoke(buildModeAction);

            actionList = new List<BuilderInWorldEntityAction>();          
        }
    }
}
