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
    public TMP_InputField nameInputField;
    public Image selectedImg, lockImg, showImg;
    public System.Action<BuilderInWorldEntityListController.EntityAction, DCLBuilderInWorldEntity, EntityListAdapter> OnActionInvoked;
    public System.Action<DCLBuilderInWorldEntity, string> OnEntityRename;
    DCLBuilderInWorldEntity currentEntity;


    private void OnDestroy()
    {
        if (currentEntity != null)
        {
            currentEntity.onStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
        }
    }

    public void SetContent(DCLBuilderInWorldEntity decentrelandEntity)
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
        OnActionInvoked?.Invoke(BuilderInWorldEntityListController.EntityAction.SELECT,currentEntity, this);
    }

    public void ShowOrHide()
    {
         OnActionInvoked?.Invoke(BuilderInWorldEntityListController.EntityAction.SHOW, currentEntity, this);
    }

    public void LockOrUnlock()
    {
        OnActionInvoked?.Invoke(BuilderInWorldEntityListController.EntityAction.LOCK, currentEntity, this);
    }

    public void DeleteEntity()
    {
        OnActionInvoked?.Invoke(BuilderInWorldEntityListController.EntityAction.DELETE, currentEntity, this);
    }

    void SetInfo(DCLBuilderInWorldEntity entityToEdit)
    {
        if (this != null)
        {
            if (string.IsNullOrEmpty(entityToEdit.GetDescriptiveName()))
            {
                nameInputField.text = entityToEdit.rootEntity.entityId;
                nameTxt.text = entityToEdit.rootEntity.entityId;
            }
            else
            {
                nameInputField.text = entityToEdit.GetDescriptiveName();
                nameTxt.text = entityToEdit.GetDescriptiveName();
            }

            //NOTE (Adrian): this is done to force the text component to update, otherwise it won't show the text, seems like a bug on textmeshpro to me
            nameInputField.textComponent.enabled = true;

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

    public void Rename(string newName)
    {
        OnEntityRename?.Invoke(currentEntity, newName);
    }

    void DeleteAdapter(DCLBuilderInWorldEntity entityToEdit)
    {
        if (this != null)
            if (entityToEdit.entityUniqueId == currentEntity.entityUniqueId)
                Destroy(gameObject);
    }
}
