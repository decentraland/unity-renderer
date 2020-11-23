using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildMode : MonoBehaviour
{

    [Header("Snap variables")]
    public float snapFactor = 1f;
    public float snapRotationDegresFactor = 15f;
    public float snapScaleFactor = 0.5f;

    public float snapDistanceToActivateMovement = 10f;

    public System.Action OnInputDone;
    public System.Action<BuildModeAction> OnActionGenerated;

    protected GameObject editionGO, undoGO, snapGO, freeMovementGO;

    protected bool isSnapActive = false, isMultiSelectionActive = false, isModeActive = false;
    protected List<DecentralandEntityToEdit> selectedEntities;
    public virtual void Init(GameObject goToEdit, GameObject undoGO, GameObject snapGO, GameObject freeMovementGO, List<DecentralandEntityToEdit> selectedEntities)
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

    public virtual void Desactivate()
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

    public virtual void SetDuplicationOffset(float offset)
    {

    }

    public virtual void SelectedEntity(DecentralandEntityToEdit selectedEntity)
    {
        CenterGameObjectToEdit();

        BuildModeUtils.CopyGameObjectStatus(editionGO, undoGO, false, false);
    }

    public virtual void CenterGameObjectToEdit()
    {
        if (selectedEntities.Count > 0)
        {
            foreach (DecentralandEntityToEdit entity in selectedEntities)
            {
                entity.rootEntity.gameObject.transform.SetParent(null);
            }
            editionGO.transform.position = GetCenterPointOfSelectedObjects();
            editionGO.transform.rotation = Quaternion.Euler(0, 0, 0);
            editionGO.transform.localScale = Vector3.one;
            foreach (DecentralandEntityToEdit entity in selectedEntities)
            {
                entity.rootEntity.gameObject.transform.SetParent(editionGO.transform);
            }
        }
    }

    public virtual void CreatedEntity(DecentralandEntityToEdit createdEntity)
    {

    }

    public virtual void EntityDeselected(DecentralandEntityToEdit entityDeselected)
    {
        CenterGameObjectToEdit();
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

        foreach(DecentralandEntityToEdit decentralandEntityToEdit in selectedEntities)
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
        foreach (DecentralandEntityToEdit entity in selectedEntities)
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

    protected List<BuildModeEntityAction> actionList = new List<BuildModeEntityAction>();
    protected void TransformActionStarted(DecentralandEntity entity, string type)
    {
        
        BuildModeEntityAction buildModeEntityAction = new BuildModeEntityAction(entity);
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

        List<BuildModeEntityAction> removeList = new List<BuildModeEntityAction>();
        foreach (BuildModeEntityAction entityAction in actionList)
        {
            if (entityAction.entity == entity)
            {
         
                switch (type)
                {
                    case "MOVE":
                      
                        entityAction.newValue = entity.gameObject.transform.position;
                        if (Vector3.Distance((Vector3)entityAction.oldValue ,(Vector3)entityAction.newValue) <= 0.09f) removeList.Add(entityAction);
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
        }
        foreach (BuildModeEntityAction entityAction in removeList)
        {
            actionList.Remove(entityAction);
        }
    }

    protected void ActionFinish(BuildModeAction.ActionType type)
    {
        if (actionList.Count > 0 && selectedEntities.Count > 0)
        {
            BuildModeAction buildModeAction = new BuildModeAction();

            buildModeAction.actionType = type;
            buildModeAction.CreateActionType(actionList, type);
            OnActionGenerated?.Invoke(buildModeAction);

            actionList = new List<BuildModeEntityAction>();
        }
    }
}
