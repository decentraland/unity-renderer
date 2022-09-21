using System;
using DCL;
using DCL.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemToggle : UIButton, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int LOADING_ANIMATOR_TRIGGER_LOADED = Animator.StringToHash("Loaded");

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
    [SerializeField] internal Animator loadingAnimator;
    [SerializeField] internal TextMeshProUGUI amountText;
    [SerializeField] internal GameObject root;
    [SerializeField] internal Material grayScaleMaterial;

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
        thumbnail.material = grayScaleMaterial;
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

        if (gameObject.activeInHierarchy)
            GetThumbnail();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!ShowIncompatibleWarningPanel())
        {
            if (!ShowReplacementWarningPanel())
                ShowHidingWarningPanel();
        }
    }

    private bool ShowReplacementWarningPanel()
    {
        if (getEquippedWearablesReplacedByFunc == null) return false;
        var shouldShow = getEquippedWearablesReplacedByFunc(wearableItem);
        warningPanel.SetActive(shouldShow);
        return shouldShow;
    }
    
    private bool ShowHidingWarningPanel()
    {
        if (getEquippedWearablesHiddenBy == null) return false;
        var shouldShow = getEquippedWearablesHiddenBy(wearableItem);
        hideWarningPanel.SetActive(shouldShow);
        return shouldShow;
    }

    private bool ShowIncompatibleWarningPanel()
    {
        if(getBodyShapeCompatibility == null) return false;
        var shouldShow = getBodyShapeCompatibility(wearableItem);
        incompatibleWarningPanel.SetActive(shouldShow);
        return shouldShow;
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
        GetThumbnail(); 
    }

    protected virtual void OnDestroy()
    {
        OnClicked = null;
    }

    protected void CallOnSellClicked() { OnSellClicked?.Invoke(this); }

    private void SetColorScale()
    {
        if(getBodyShapeCompatibility != null && wearableItem != null && getBodyShapeCompatibility(wearableItem))
        {
            thumbnail.material = grayScaleMaterial;
        }
        else
        {
            thumbnail.material = null;
        }
    }

    private void GetThumbnail()
    {
        string url = wearableItem?.ComposeThumbnailUrl();
        
        if (wearableItem == null || string.IsNullOrEmpty(url))
        {
            SetLoadingAnimationTrigger(LOADING_ANIMATOR_TRIGGER_LOADED);
            return;
        }

        ThumbnailsManager.GetThumbnail(url, OnThumbnailReady);
    }
    
    private void OnThumbnailReady(Asset_Texture texture)
    {
        SetLoadingAnimationTrigger(LOADING_ANIMATOR_TRIGGER_LOADED);

        thumbnail.sprite = ThumbnailsManager.GetOrCreateSpriteFromTexture(texture.texture, out var wasCreated);

        if (view != null && wasCreated)
        {
            if (view.avatarEditorCanvas.enabled)
                AudioScriptableObjects.listItemAppear.Play(true);
        }
        thumbnail.material = grayScaleMaterial;
        SetColorScale();
    }
    
    private void SetLoadingAnimationTrigger(int id)
    {
        if (!loadingAnimator.isInitialized || loadingAnimator.runtimeAnimatorController == null)
            return;

        loadingAnimator.SetTrigger(id);
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