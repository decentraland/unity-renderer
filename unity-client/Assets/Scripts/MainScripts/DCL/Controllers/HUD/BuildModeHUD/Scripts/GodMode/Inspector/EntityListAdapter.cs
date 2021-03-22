using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityListAdapter : MonoBehaviour
{
    public Color entitySelectedColor;
    public Color entityUnselectedColor;
    public Color entityInsideOfBoundsColor;
    public Color entityOutOfBoundsColor;
    public Color iconsSelectedColor;
    public Color iconsUnselectedColor;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI nameInputField_Text;
    public Image selectedImg;
    public Button unlockButton;
    public Button lockButton;
    public Image showImg;
    public System.Action<EntityAction, DCLBuilderInWorldEntity, EntityListAdapter> OnActionInvoked;
    public System.Action<DCLBuilderInWorldEntity, string> OnEntityRename;
    DCLBuilderInWorldEntity currentEntity;

    private void OnDestroy()
    {
        if (currentEntity != null)
        {
            currentEntity.onStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
            DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged -= ChangeEntityBoundsCheckerStatus;
        }
    }

    public void SetContent(DCLBuilderInWorldEntity decentrelandEntity)
    {
        if (currentEntity != null)
        {
            currentEntity.onStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
            DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged -= ChangeEntityBoundsCheckerStatus;
        }
        currentEntity = decentrelandEntity;
        currentEntity.onStatusUpdate += SetInfo;
        currentEntity.OnDelete += DeleteAdapter;
        DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged += ChangeEntityBoundsCheckerStatus;

        AllowNameEdition(false);
        SetInfo(decentrelandEntity);
    }

    public void SelectOrDeselect() { OnActionInvoked?.Invoke(EntityAction.SELECT, currentEntity, this); }

    public void ShowOrHide() { OnActionInvoked?.Invoke(EntityAction.SHOW, currentEntity, this); }

    public void LockOrUnlock() { OnActionInvoked?.Invoke(EntityAction.LOCK, currentEntity, this); }

    public void DeleteEntity() { OnActionInvoked?.Invoke(EntityAction.DELETE, currentEntity, this); }

    void SetInfo(DCLBuilderInWorldEntity entityToEdit)
    {
        if (this != null)
        {
            if (string.IsNullOrEmpty(entityToEdit.GetDescriptiveName()))
                nameInputField.text = entityToEdit.rootEntity.entityId;
            else
                nameInputField.text = entityToEdit.GetDescriptiveName();

            //NOTE (Adrian): this is done to force the text component to update, otherwise it won't show the text, seems like a bug on textmeshpro to me
            nameInputField.textComponent.enabled = true;

            if (entityToEdit.IsVisible)
                showImg.color = iconsSelectedColor;
            else
                showImg.color = iconsUnselectedColor;

            unlockButton.gameObject.SetActive(!entityToEdit.IsLocked);
            lockButton.gameObject.SetActive(entityToEdit.IsLocked);

            if (entityToEdit.IsSelected)
            {
                AllowNameEdition(true);
                selectedImg.color = entitySelectedColor;
            }
            else
            {
                AllowNameEdition(false);
                selectedImg.color = entityUnselectedColor;
            }
        }
    }

    public void Rename(string newName) { OnEntityRename?.Invoke(currentEntity, newName); }

    public void AllowNameEdition(bool isAllowed) { nameInputField.enabled = isAllowed; }

    void DeleteAdapter(DCLBuilderInWorldEntity entityToEdit)
    {
        if (this != null)
            if (entityToEdit.entityUniqueId == currentEntity.entityUniqueId)
                Destroy(gameObject);
    }

    private void ChangeEntityBoundsCheckerStatus(DCL.Models.DecentralandEntity entity, bool isInsideBoundaries)
    {
        if (currentEntity.rootEntity.entityId != entity.entityId)
            return;

        nameInputField_Text.color = isInsideBoundaries ? entityInsideOfBoundsColor : entityOutOfBoundsColor;
    }
}