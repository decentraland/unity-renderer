using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExcludeFromCodeCoverage]
public class ActionAdapter : MonoBehaviour
{
    public Image actionImg, notDoneImg;

    public Sprite moveSprite,
                  rotateSprite,
                  scaleSprite,
                  createdSprite;

    public TextMeshProUGUI actionTitle;
    public System.Action<BIWCompleteAction, ActionAdapter> OnActionSelected;

    BIWCompleteAction action;

    public void SetContent(BIWCompleteAction action)
    {
        this.action = action;

        switch (this.action.actionType)
        {
            case IBIWCompleteAction.ActionType.MOVE:
                actionImg.sprite = moveSprite;
                break;
            case IBIWCompleteAction.ActionType.ROTATE:
                actionImg.sprite = rotateSprite;
                break;
            case IBIWCompleteAction.ActionType.SCALE:
                actionImg.sprite = scaleSprite;
                break;
            case IBIWCompleteAction.ActionType.CREATE:
                actionImg.sprite = createdSprite;
                break;

            default:
                actionImg.enabled = false;
                break;
        }

        actionTitle.text = this.action.actionType.ToString().Replace("_", " ");
        RefreshIsDone();
    }

    public void RefreshIsDone() { notDoneImg.enabled = !action.isDone; }

    public void Selected() { OnActionSelected?.Invoke(action, this); }
}