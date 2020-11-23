using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuildModeAction;

public class ActionController : MonoBehaviour
{
    public ActionListview actionListview;

    public BuilderInWorldEntityHandler builderInWorldEntityHandler;

    public System.Action OnUndo, OnRedo;


    List<BuildModeAction> actionsMade = new List<BuildModeAction>();

    int currentStepIndex = 0;

    private void Awake()
    {
        if(actionListview != null)
            actionListview.OnActionSelected += GoToAction;
    }

    public void ClearActionList()
    {
        actionsMade.Clear();
    }

    public void GoToAction(BuildModeAction action)
    {
        int index = actionsMade.IndexOf(action);
        int stepsAmount = currentStepIndex - index;

        for(int i = 0; i <= Mathf.Abs(stepsAmount); i++)
        {
            if (stepsAmount > 0)
            {
                UndoCurrentAction();
                if (currentStepIndex > 0)
                    currentStepIndex--;
            }
            else
            {
                RedoCurrentAction();
                if (currentStepIndex + 1 < actionsMade.Count)
                    currentStepIndex++;
            }
        }

    }

    public void TryToRedoAction()
    {
        if (currentStepIndex < actionsMade.Count)
        {
            if (currentStepIndex + 1 < actionsMade.Count)
            {
                if(currentStepIndex == 0 && actionsMade[currentStepIndex].isDone)
                    currentStepIndex++;
                else if(currentStepIndex != 0)
                    currentStepIndex++;
                RedoCurrentAction();
            }
        }
    }

    public void TryToUndoAction()
    {
        if (currentStepIndex >= 0 && actionsMade.Count > 0)
        {
            UndoCurrentAction();
            if (currentStepIndex > 0)
                currentStepIndex--;
        }
    }

    public void AddAction(BuildModeAction action)
    {
        bool removedActions = false;
        if (currentStepIndex < actionsMade.Count-1)
        {
            actionsMade.RemoveRange(currentStepIndex, actionsMade.Count - currentStepIndex);
            removedActions = true;
        }
        actionsMade.Add(action);
    
        currentStepIndex = actionsMade.Count-1;
        if (removedActions && actionListview != null)
            actionListview.SetContent(actionsMade);
        action.OnApplyValue += ApplyAction;
    }

    void ApplyAction(DecentralandEntity entityToApply, object value, BuildModeAction.ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.MOVE:
                Vector3 convertedPosition = (Vector3)value;
                entityToApply.gameObject.transform.position = convertedPosition;
                break;
            case ActionType.ROTATE:
                Vector3 convertedAngles = (Vector3)value;
                entityToApply.gameObject.transform.eulerAngles = convertedAngles;
                break;
            case ActionType.SCALE:
                Vector3 convertedScale = (Vector3)value;
                Transform parent = entityToApply.gameObject.transform.parent;

                entityToApply.gameObject.transform.localScale = new Vector3(convertedScale.x / parent.localScale.x, convertedScale.y / parent.localScale.y, convertedScale.z / parent.localScale.z);
                break;
            case ActionType.CREATED:
                string entityString = (string)value;
                if (entityString.Length < 255)
                {
                    builderInWorldEntityHandler.DeleteEntity((string)value);
                }
                else
                {
                    builderInWorldEntityHandler.CreateEntityFromJSON((string)value);
                }
                    break;
        }
    }

    void RedoCurrentAction()
    {
        if (!actionsMade[currentStepIndex].isDone)
        {
            actionsMade[currentStepIndex].ReDo();
            if (actionListview != null)
                actionListview.RefreshInfo();
            OnRedo?.Invoke();
          
        }
  
    }

    void UndoCurrentAction()
    {
        if (actionsMade[currentStepIndex].isDone)
        {
            actionsMade[currentStepIndex].Undo();
            if (actionListview != null)
                actionListview.RefreshInfo();
            OnUndo?.Invoke();         
        }
    }


}
