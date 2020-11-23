using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityListAdapter : MonoBehaviour
{
    public Color entitySelectedColor, entityUnselectedColor;
    public Color iconsSelectedColor, iconsUnselectedColor;
    public TextMeshProUGUI nameTxt;
    public Image selectedImg, lockImg, showImg;
    public System.Action<BuildModeEntityListController.EntityAction, DecentralandEntityToEdit, EntityListAdapter> OnActionInvoked;
    DecentralandEntityToEdit currentEntity;


    private void OnDestroy()
    {
        if (currentEntity != null)
        {
            currentEntity.onStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
        }
    }

    public void SetContent(DecentralandEntityToEdit decentrelandEntity)
    {
        if(currentEntity != null)
        {
            currentEntity.onStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
        }
        currentEntity = decentrelandEntity;
        currentEntity.onStatusUpdate += SetInfo;
        currentEntity.OnDelete += DeleteAdapter;

        SetInfo(decentrelandEntity);
    }

    public void SelectOrDeselect()
    {
        OnActionInvoked?.Invoke(BuildModeEntityListController.EntityAction.SELECT,currentEntity, this);
    }

    public void ShowOrHide()
    {
         OnActionInvoked?.Invoke(BuildModeEntityListController.EntityAction.SHOW, currentEntity, this);
    }

    public void LockOrUnlock()
    {
        OnActionInvoked?.Invoke(BuildModeEntityListController.EntityAction.LOCK, currentEntity, this);
    }

    public void DeleteEntity()
    {
        OnActionInvoked?.Invoke(BuildModeEntityListController.EntityAction.DELETE, currentEntity, this);
    }

    void SetInfo(DecentralandEntityToEdit entityToEdit)
    {
        if (this != null)
        {
            nameTxt.text = entityToEdit.rootEntity.entityId;
            if (entityToEdit.IsVisible)
                showImg.color = iconsSelectedColor;
            else
                showImg.color = iconsUnselectedColor;

            if (entityToEdit.IsLocked)
                lockImg.color = iconsSelectedColor;
            else
                lockImg.color = iconsUnselectedColor;


            if (entityToEdit.IsSelected)
                selectedImg.color = entitySelectedColor;
            else
                selectedImg.color = entityUnselectedColor;
        }
    }

    void DeleteAdapter(DecentralandEntityToEdit entityToEdit)
    {
        if (this != null)
            if (entityToEdit.entityUniqueId == currentEntity.entityUniqueId)
                Destroy(gameObject);
    }
}
