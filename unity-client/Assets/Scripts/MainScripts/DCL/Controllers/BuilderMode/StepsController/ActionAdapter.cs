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
    public System.Action<BuildModeAction, ActionAdapter> OnActionSelected;

    BuildModeAction action;

    public void SetContent(BuildModeAction action)
    {
        this.action = action;

        switch (this.action.actionType)
        {
            case BuildModeAction.ActionType.MOVE:
                actionImg.sprite = moveSprite;
                break;
            case BuildModeAction.ActionType.ROTATE:
                actionImg.sprite = rotateSprite;
                break;
            case BuildModeAction.ActionType.SCALE:
                actionImg.sprite = scaleSprite;
                break;
            case BuildModeAction.ActionType.CREATED:
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
