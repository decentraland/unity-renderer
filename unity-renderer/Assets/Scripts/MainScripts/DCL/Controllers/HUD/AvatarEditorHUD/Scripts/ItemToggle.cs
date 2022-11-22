using System;
using DCL;
using DCL.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemToggle : UIButton, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly string ANIMATION_LOADED = "Loaded";
    private static readonly string ANIMATION_LOADING_IDLE = "LoadingIdle";

    public event Action<ItemToggle> OnClicked;
    public event Action<ItemToggle> OnSellClicked;

    public WearableItem wearableItem { get; private set; }

    public Image thumbnail;
    public Image selectionHighlight;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private GameObject incompatibleWarningPanel;
    [SerializeField] private GameObject hideWarningPanel;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] internal RectTransform amountContainer;
    [SerializeField] internal Animation loadingAnimation;
    [SerializeField] internal TextMeshProUGUI amountText;
    [SerializeField] internal GameObject root;
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] internal Material grayScaleMaterial;
    [SerializeField] internal Button selectButton;

    private bool selectedValue;

    private AvatarEditorHUDView view;

    private Func<WearableItem, bool> getEquippedWearablesReplacedByFunc;
    private Func<WearableItem, bool> getEquippedWearablesHiddenBy;
    private Func<WearableItem, bool> getBodyShapeCompatibility;

    public string collectionId { get; set; }

    public bool selected
    {
        get { return selectedValue; }
        set
        {
            selectedValue = value;
            SetSelection(selectedValue);
        }
    }

    protected virtual void SetSelection(bool isSelected)
    {
        if (selectionHighlight != null)
            selectionHighlight.enabled = isSelected;
    }

    public void SetLoadingSpinner(bool isActive) { loadingSpinner?.SetActive(isActive); }

    protected new virtual void Awake()
    {
        base.Awake();
        thumbnail.sprite = null;
        warningPanel.SetActive(false);
        hideWarningPanel.SetActive(false);
        incompatibleWarningPanel.SetActive(false);
        if (!EnvironmentSettings.RUNNING_TESTS)
            view = GetComponentInParent<AvatarEditorHUDView>();
    }

    protected override void OnClick()
    {
        OnClicked?.Invoke(this);
        warningPanel.SetActive(false);
    }

    public virtual void Initialize(WearableItem w, bool isSelected, int amount, NFTItemToggleSkin skin)
    {
        root.gameObject.SetActive(true);

        wearableItem = w;
        selected = isSelected;
        amountContainer.gameObject.SetActive(amount > 1);
        amountText.text = $"x{amount.ToString()}";
        
        UpdateThumbnail();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        incompatibleWarningPanel.SetActive(false);
        warningPanel.SetActive(false);
        hideWarningPanel.SetActive(false);
        if(IsIncompatible())
        {
            incompatibleWarningPanel.SetActive(true);
        }
        else if(IsReplacingWearables())
        {
            warningPanel.SetActive(true);
        }
        else if(IsHidingWearables())
        {
            hideWarningPanel.SetActive(true);
        }
    }

    private bool IsIncompatible()
    {
        if(getBodyShapeCompatibility == null) return false;
        return getBodyShapeCompatibility(wearableItem);
    }

    private bool IsReplacingWearables()
    {
        if (getEquippedWearablesReplacedByFunc == null) return false;
        return getEquippedWearablesReplacedByFunc(wearableItem);
    }

    private bool IsHidingWearables()
    {
        if (getEquippedWearablesHiddenBy == null) return false;
        return getEquippedWearablesHiddenBy(wearableItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        warningPanel.SetActive(false);
        hideWarningPanel.SetActive(false);
        incompatibleWarningPanel.SetActive(false);
    }

    public void SetHideOtherWerablesToastStrategy(Func<WearableItem, bool> function) =>
        getEquippedWearablesHiddenBy = function;

    public void SetBodyShapeCompatibilityStrategy(Func<WearableItem, bool> function) =>
        getBodyShapeCompatibility = function;
    
    public void SetReplaceOtherWearablesToastStrategy(Func<WearableItem, bool> function) =>
        getEquippedWearablesReplacedByFunc = function;

    private void OnEnable() 
    { 
        UpdateThumbnail(); 
    }

    protected virtual void OnDestroy()
    {
        OnClicked = null;
    }

    protected void CallOnSellClicked() { OnSellClicked?.Invoke(this); }

    private void SetColorScale()
    {
        if(getBodyShapeCompatibility != null && getBodyShapeCompatibility(wearableItem))
        {
            thumbnail.material = grayScaleMaterial;
            thumbnail.SetMaterialDirty();
            disabledOverlay.SetActive(true);
            selectButton.interactable = false;
        }
        else
        {
            thumbnail.material = null;
            disabledOverlay.SetActive(false);
            selectButton.interactable = true;
        }
    }

    private void UpdateThumbnail()
    {
        string url = wearableItem?.ComposeThumbnailUrl();
        
        if (wearableItem == null || string.IsNullOrEmpty(url))
        {
            SetLoadingAnimation(ANIMATION_LOADED);
            return;
        }

        SetLoadingAnimation(ANIMATION_LOADING_IDLE);

        ThumbnailsManager.GetThumbnail(url, OnThumbnailReady);
    }
    
    private void OnThumbnailReady(Asset_Texture texture)
    {
        SetLoadingAnimation(ANIMATION_LOADED);

        thumbnail.sprite = ThumbnailsManager.GetOrCreateSpriteFromTexture(texture.texture, out var wasCreated);

        if (view != null && wasCreated)
        {
            if (view.avatarEditorCanvas.enabled)
                AudioScriptableObjects.listItemAppear.Play(true);
        }
        SetColorScale();
    }
    
    private void SetLoadingAnimation(string id)
    {
        if (!loadingAnimation.isActiveAndEnabled)
            return;

        loadingAnimation.Play(id);
    }
    
    public void Hide()
    {
        root.gameObject.SetActive(false);
    }
    public void SetCallbacks(Action<ItemToggle> toggleClicked, Action<ItemToggle> sellClicked)
    {
        OnClicked = toggleClicked;
        OnSellClicked = sellClicked;
    }
}