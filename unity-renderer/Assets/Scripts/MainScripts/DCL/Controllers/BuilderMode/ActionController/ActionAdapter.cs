using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionAdapter : MonoBehaviour
{
    public Image actionImg,notDoneImg;

    public Sprite moveSprite,
                  rotateSprite,
                  scaleSprite,
                  createdSprite;

    public TextMeshProUGUI actionTitle;
    public System.Action<BuildInWorldCompleteAction, ActionAdapter> OnActionSelected;

    BuildInWorldCompleteAction action;

    public void SetContent(BuildInWorldCompleteAction action)
    {
        this.action = action;

        switch (this.action.actionType)
        {
            case BuildInWorldCompleteAction.ActionType.MOVE:
                actionImg.sprite = moveSprite;
                break;
            case BuildInWorldCompleteAction.ActionType.ROTATE:
                actionImg.sprite = rotateSprite;
                break;
            case BuildInWorldCompleteAction.ActionType.SCALE:
                actionImg.sprite = scaleSprite;
                break;
            case BuildInWorldCompleteAction.ActionType.CREATE:
                actionImg.sprite = createdSprite;
                break;
        
            default:
                actionImg.enabled = false;
                break;
        }

        actionTitle.text = this.action.actionType.ToString().Replace("_", " ");
        RefreshIsDone();
    }

    public void RefreshIsDone()
    {
        notDoneImg.enabled = !action.isDone;
    }

    public void Selected()
    {
        OnActionSelected?.Invoke(action, this);
    }
}
