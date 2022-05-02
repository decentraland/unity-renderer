using DCL;
using DCL.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntityListAdapter : MonoBehaviour
{
    public Color entitySelectedColor;
    public Color entityUnselectedColor;
    public Color entityWithoutErrorsColor;
    public Color entityErrorColor;
    public Color iconsSelectedColor;
    public Color iconsUnselectedColor;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI nameInputField_Text;
    public Image selectedImg;
    public RawImage entityThumbnailImg;
    public Button unlockButton;
    public Button lockButton;
    public Image showImg;
    public Image textBoxImage;
    public System.Action<EntityAction, BIWEntity, EntityListAdapter> OnActionInvoked;
    public System.Action<BIWEntity, string> OnEntityRename;
    BIWEntity currentEntity;
    internal AssetPromise_Texture loadedThumbnailPromise;

    private void Start()
    {
        if (nameInputField != null)
        {
            nameInputField.onSelect.AddListener((currentText) => SetTextboxActive(true));

            nameInputField.onEndEdit.AddListener((newText) =>
            {
                Rename(newText);
                SetTextboxActive(false);

                if (EventSystem.current != null && !EventSystem.current.alreadySelecting)
                    EventSystem.current.SetSelectedGameObject(null);
            });

            nameInputField.onSubmit.AddListener((newText) => EventSystem.current?.SetSelectedGameObject(null));
        }

        SetTextboxActive(false);
    }

    private void OnDestroy()
    {
        if (nameInputField != null)
        {
            nameInputField.onSelect.RemoveAllListeners();
            nameInputField.onEndEdit.RemoveAllListeners();
            nameInputField.onSubmit.RemoveAllListeners();
        }

        if (currentEntity != null)
        {
            currentEntity.OnStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
            currentEntity.OnErrorStatusChange -= SetEntityError;
        }
    }

    public void SetContent(BIWEntity decentrelandEntity)
    {
        if (currentEntity != null)
        {
            currentEntity.OnStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
            currentEntity.OnErrorStatusChange -= SetEntityError;
        }

        currentEntity = decentrelandEntity;
        currentEntity.OnStatusUpdate += SetInfo;
        currentEntity.OnDelete += DeleteAdapter;
        currentEntity.OnErrorStatusChange += SetEntityError;

        AllowNameEdition(false);
        SetInfo(decentrelandEntity);

        entityThumbnailImg.enabled = false;
        CatalogItem entitySceneObject = decentrelandEntity.GetCatalogItemAssociated();
        GetThumbnail(entitySceneObject);
    }

    public void SelectOrDeselect()
    {
        if (currentEntity.isVisible)
            OnActionInvoked?.Invoke(EntityAction.SELECT, currentEntity, this);
    }

    public void ShowOrHide() { OnActionInvoked?.Invoke(EntityAction.SHOW, currentEntity, this); }

    public void LockOrUnlock() { OnActionInvoked?.Invoke(EntityAction.LOCK, currentEntity, this); }

    public void DeleteEntity() { OnActionInvoked?.Invoke(EntityAction.DELETE, currentEntity, this); }

    void SetInfo(BIWEntity entityToEdit)
    {
        if (this == null)
            return;

        if (string.IsNullOrEmpty(entityToEdit.GetDescriptiveName()))
            nameInputField.text = entityToEdit.rootEntity.entityId.ToString();
        else
            nameInputField.text = entityToEdit.GetDescriptiveName();

        //NOTE (Adrian): this is done to force the text component to update, otherwise it won't show the text, seems like a bug on textmeshpro to me
        nameInputField.textComponent.enabled = true;


        if (entityToEdit.isVisible)
            showImg.color = iconsSelectedColor;
        else
            showImg.color = iconsUnselectedColor;

        CheckEntityNameColor(entityToEdit);

        unlockButton.gameObject.SetActive(!entityToEdit.isLocked);
        lockButton.gameObject.SetActive(entityToEdit.isLocked);

        if (entityToEdit.isSelected)
        {
            AllowNameEdition(true);
            selectedImg.color = entitySelectedColor;
        }
        else
        {
            AllowNameEdition(false);
            selectedImg.color = entityUnselectedColor;
        }

        SetEntityError(entityToEdit);
    }

    internal void GetThumbnail(CatalogItem catalogItem)
    {
        if (catalogItem == null)
            return;

        var url = catalogItem.thumbnailURL;

        if (string.IsNullOrEmpty(url))
            return;

        var newLoadedThumbnailPromise = new AssetPromise_Texture(url);
        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += (x, error) => { Debug.Log($"Error downloading: {url}, Exception: {error}"); };
        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);
        AssetPromiseKeeper_Texture.i.Forget(loadedThumbnailPromise);
        loadedThumbnailPromise = newLoadedThumbnailPromise;
    }

    internal void SetThumbnail(Asset_Texture texture)
    {
        if (entityThumbnailImg == null)
            return;
        entityThumbnailImg.enabled = true;
        entityThumbnailImg.texture = texture.texture;
    }

    public void Rename(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
            OnEntityRename?.Invoke(currentEntity, newName);
        else
            nameInputField.text = currentEntity.GetDescriptiveName();
    }

    public void AllowNameEdition(bool isAllowed) { nameInputField.enabled = isAllowed; }

    void DeleteAdapter(BIWEntity entityToEdit)
    {
        if (this != null && entityToEdit.entityUniqueId == currentEntity.entityUniqueId)
            Destroy(gameObject);
    }

    private void SetEntityError(BIWEntity entity)
    {
        if (entity != currentEntity)
            return;
        
        CheckEntityNameColor(entity);
    }

    private void SetTextboxActive(bool isActive)
    {
        if (textBoxImage == null)
            return;

        textBoxImage.enabled = isActive;
    }

    private void CheckEntityNameColor(BIWEntity entity)
    {
        if (entity.hasError)
            nameInputField_Text.color = entityErrorColor;
        else if (!entity.isVisible || entity.isLocked)
            nameInputField_Text.color = iconsUnselectedColor;
        else
            nameInputField_Text.color = entityWithoutErrorsColor;
    }
}