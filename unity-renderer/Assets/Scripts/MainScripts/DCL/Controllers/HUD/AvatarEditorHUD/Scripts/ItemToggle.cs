using System;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemToggle : UIButton, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int LOADING_ANIMATOR_TRIGGER_LOADED = Animator.StringToHash("Loaded");

    public event System.Action<ItemToggle> OnClicked;
    public event System.Action<ItemToggle> OnSellClicked;

    public WearableItem wearableItem { get; private set; }

    public Image thumbnail;
    public Image selectionHighlight;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private GameObject hideWarningPanel;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] internal RectTransform amountContainer;
    [SerializeField] internal Animator loadingAnimator;
    [SerializeField] internal TextMeshProUGUI amountText;

    private bool selectedValue;

    private string loadedThumbnailURL;
    private AssetPromise_Texture loadedThumbnailPromise;

    private AvatarEditorHUDView view;

    private Func<WearableItem, bool> getEquippedWearablesReplacedByFunc;
    private Func<WearableItem, bool> getEquippedWearablesHiddenBy;

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

        if (!EnvironmentSettings.RUNNING_TESTS)
            view = GetComponentInParent<AvatarEditorHUDView>();
    }

    protected override void OnClick()
    {
        OnClicked?.Invoke(this);
        warningPanel.SetActive(false);
    }

    public virtual void Initialize(WearableItem w, bool isSelected, int amount)
    {
        wearableItem = w;
        selected = isSelected;
        amountContainer.gameObject.SetActive(amount > 1);
        amountText.text = $"x{amount.ToString()}";

        if (gameObject.activeInHierarchy)
            GetThumbnail();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!ShowReplacementWarningPanel())
            ShowHidingWarningPanel();
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

    public void OnPointerExit(PointerEventData eventData)
    {
        warningPanel.SetActive(false);
        hideWarningPanel.SetActive(false);
    }

    public void SetHideOtherWerablesToastStrategy(Func<WearableItem, bool> function) =>
        getEquippedWearablesHiddenBy = function;
    
    public void SetReplaceOtherWearablesToastStrategy(Func<WearableItem, bool> function) =>
        getEquippedWearablesReplacedByFunc = function;

    private void OnThumbnailReady(Asset_Texture texture)
    {
        SetLoadingAnimationTrigger(LOADING_ANIMATOR_TRIGGER_LOADED);

        if (thumbnail.sprite != null)
            Destroy(thumbnail.sprite);

        thumbnail.sprite = ThumbnailsManager.CreateSpriteFromTexture(texture.texture);

        if (view != null)
        {
            if (view.avatarEditorCanvas.enabled)
                AudioScriptableObjects.listItemAppear.Play(true);
        }
    }

    private void OnEnable() { GetThumbnail(); }

    protected virtual void OnDestroy()
    {
        ForgetThumbnail();
        OnClicked = null;
    }

    protected void CallOnSellClicked() { OnSellClicked?.Invoke(this); }

    private void GetThumbnail()
    {
        string url = wearableItem?.ComposeThumbnailUrl();

        if (url == loadedThumbnailURL)
        {
            SetLoadingAnimationTrigger(LOADING_ANIMATOR_TRIGGER_LOADED);
            return;
        }

        if (wearableItem == null || string.IsNullOrEmpty(url))
        {
            SetLoadingAnimationTrigger(LOADING_ANIMATOR_TRIGGER_LOADED);
            return;
        }

        loadedThumbnailURL = url;
        var newLoadedThumbnailPromise = ThumbnailsManager.GetThumbnail(url, OnThumbnailReady);
        ThumbnailsManager.ForgetThumbnail(loadedThumbnailPromise);
        loadedThumbnailPromise = newLoadedThumbnailPromise;
    }

    private void SetLoadingAnimationTrigger(int id)
    {
        if (!loadingAnimator.isInitialized || loadingAnimator.runtimeAnimatorController == null)
            return;

        loadingAnimator.SetTrigger(id);
    }

    private void ForgetThumbnail()
    {
        ThumbnailsManager.ForgetThumbnail(loadedThumbnailPromise);
        loadedThumbnailURL = null;
        loadedThumbnailPromise = null;
    }
}