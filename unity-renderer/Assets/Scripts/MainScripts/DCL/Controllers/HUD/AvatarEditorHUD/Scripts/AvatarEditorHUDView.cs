using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[assembly: InternalsVisibleTo("AvatarEditorHUDTests")]

public class AvatarEditorHUDView : MonoBehaviour, IPointerDownHandler
{
    private static readonly int RANDOMIZE_ANIMATOR_LOADING_BOOL = Animator.StringToHash("Loading");
    private const string VIEW_PATH = "AvatarEditorHUD";
    private const string VIEW_OBJECT_NAME = "_AvatarEditorHUD";
    internal const int AVATAR_SECTION_INDEX = 0;
    internal const string AVATAR_SECTION_TITLE = "<b>Avatar</b>";
    internal const int EMOTES_SECTION_INDEX = 1;
    internal const string EMOTES_SECTION_TITLE = "<b>Emotes</b>";
    private const string RESET_PREVIEW_ANIMATION = "Idle";
    private const float TIME_TO_RESET_PREVIEW_ANIMATION = 0.2f;

    public bool isOpen { get; private set; }
    internal DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;

    internal bool arePanelsInitialized = false;

    [Serializable]
    public class AvatarEditorNavigationInfo
    {
        public Toggle toggle;
        public Canvas canvas;
        public bool enabledByDefault;
        public CharacterPreviewController.CameraFocus focus = CharacterPreviewController.CameraFocus.DefaultEditing;

        //To remove when we refactor this to avoid ToggleGroup issues when quitting application
        public void Initialize() { Application.quitting += () => toggle.onValueChanged.RemoveAllListeners(); }
    }

    [Serializable]
    public class AvatarEditorWearableFilter
    {
        public string categoryFilter;
        public ItemSelector selector;
    }

    [SerializeField]
    internal Canvas avatarEditorCanvas;

    [SerializeField]
    internal CanvasGroup avatarEditorCanvasGroup;

    [SerializeField]
    internal AvatarEditorNavigationInfo[] navigationInfos;

    [SerializeField]
    internal AvatarEditorWearableFilter[] wearableGridPairs;

    [SerializeField]
    internal AvatarEditorNavigationInfo collectiblesNavigationInfo;

    [SerializeField]
    internal ItemSelector collectiblesItemSelector;

    [SerializeField]
    internal ColorSelector skinColorSelector;

    [SerializeField]
    internal ColorPickerPanel skinColorPicker;

    [SerializeField]
    internal ColorPickerComponentView eyeColorPickerComponent;

    [SerializeField]
    internal ColorPickerComponentView hairColorPickerComponent;

    [SerializeField]
    internal GameObject characterPreviewPrefab;

    [SerializeField]
    internal PreviewCameraRotation characterPreviewRotation;

    [SerializeField]
    internal Button randomizeButton;

    [SerializeField]
    internal Animator randomizeAnimator;

    [SerializeField]
    internal Button doneButton;
    
    [SerializeField] internal Button[] goToMarketplaceButtons;

    [SerializeField] internal GameObject loadingSpinnerGameObject;

    [Header("Collectibles")]
    [SerializeField]
    internal GameObject web3Container;

    [SerializeField]
    internal GameObject noWeb3Container;

    [Header("Skins")]
    
    [SerializeField] internal GameObject skinsFeatureContainer;
    
    [SerializeField] internal GameObject skinsWeb3Container;

    [SerializeField] internal GameObject skinsMissingWeb3Container;
    
    [SerializeField] internal GameObject skinsConnectWalletButtonContainer;

    [SerializeField] private GameObject skinsPopulatedListContainer;
    [SerializeField] private GameObject skinsEmptyListContainer;

    [Header("Section Selector")]
    [SerializeField] internal SectionSelectorComponentView sectionSelector;
    [SerializeField] internal TMP_Text sectionTitle;
    [SerializeField] internal GameObject avatarSection;
    [SerializeField] internal GameObject emotesSection;

    internal static CharacterPreviewController characterPreviewController;
    private AvatarEditorHUDController controller;
    internal readonly Dictionary<string, ItemSelector> selectorsByCategory = new Dictionary<string, ItemSelector>();
    private readonly HashSet<WearableItem> wearablesWithLoadingSpinner = new HashSet<WearableItem>();

    public event Action<AvatarModel> OnAvatarAppear;
    public event Action<bool> OnSetVisibility;
    public event Action OnRandomize;

    private void Awake()
    {
        loadingSpinnerGameObject.SetActive(false);
        doneButton.interactable = false; //the default state of the button should be disable until a profile has been loaded.
        if (characterPreviewController == null)
        {
            characterPreviewController = Instantiate(characterPreviewPrefab).GetComponent<CharacterPreviewController>();
            characterPreviewController.name = "_CharacterPreviewController";
        }

        isOpen = false;
        arePanelsInitialized = false;
    }

    private void Initialize(AvatarEditorHUDController controller)
    {
        this.controller = controller;
        gameObject.name = VIEW_OBJECT_NAME;

        randomizeButton.onClick.AddListener(OnRandomizeButton);
        doneButton.onClick.AddListener(OnDoneButton);
        InitializeWearableChangeEvents();

        foreach (var button in goToMarketplaceButtons)
            button.onClick.RemoveAllListeners();
        foreach (var button in goToMarketplaceButtons)
            button.onClick.AddListener(controller.GoToMarketplaceOrConnectWallet);

        characterPreviewController.camera.enabled = false;
        
        ConfigureSectionSelector();
    }

    public void SetIsWeb3(bool isWeb3User)
    {
        web3Container.SetActive(isWeb3User);
        noWeb3Container.SetActive(!isWeb3User);
        skinsWeb3Container.SetActive(isWeb3User);
        skinsMissingWeb3Container.SetActive(!isWeb3User);
    }

    internal void InitializeNavigationEvents(bool isGuest)
    {
        if (arePanelsInitialized)
            return;

        for (int i = 0; i < navigationInfos.Length; i++)
        {
            InitializeNavigationInfo(navigationInfos[i]);
        }
        InitializeNavigationInfo(collectiblesNavigationInfo, !isGuest);
        
        characterPreviewRotation.OnHorizontalRotation += characterPreviewController.Rotate;
        arePanelsInitialized = true;
    }

    private void InitializeNavigationInfo(AvatarEditorNavigationInfo current, bool isActive) 
    {
        current.Initialize();

        current.toggle.isOn = isActive ? current.enabledByDefault : false;
        current.canvas.gameObject.SetActive(isActive ? current.enabledByDefault : false);

        current.toggle.onValueChanged.AddListener((on) =>
        {
            current.canvas.gameObject.SetActive(@on);
            characterPreviewController.SetFocus(current.focus);
        });
    }

    private void InitializeNavigationInfo(AvatarEditorNavigationInfo current)
    {
        InitializeNavigationInfo(current, true);
    }

    private void InitializeWearableChangeEvents()
    {
        int nPairs = wearableGridPairs.Length;
        for (int i = 0; i < nPairs; i++)
        {
            wearableGridPairs[i].selector.OnItemClicked += controller.WearableClicked;
            wearableGridPairs[i].selector.OnSellClicked += controller.SellCollectible;
            selectorsByCategory.Add(wearableGridPairs[i].categoryFilter, wearableGridPairs[i].selector);
        }

        collectiblesItemSelector.OnItemClicked += controller.WearableClicked;
        collectiblesItemSelector.OnSellClicked += controller.SellCollectible;
        collectiblesItemSelector.OnRetryClicked += controller.RetryLoadOwnedWearables;

        skinColorSelector.OnColorSelectorChange += controller.SkinColorClicked;
        /* UNCOMMENT TO ENABLE SKIN COLOR PICKER
         * skinColorPicker.OnColorChanged += controller.SkinColorClicked;
         */
        //eyeColorSelector.OnColorSelectorChange += controller.EyesColorClicked;
        //eyeColorPicker.OnColorChanged += controller.EyesColorClicked;
        eyeColorPickerComponent.OnColorChanged += controller.EyesColorClicked;
        hairColorPickerComponent.OnColorChanged += controller.HairColorClicked;
    }

    internal static AvatarEditorHUDView Create(AvatarEditorHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<AvatarEditorHUDView>();
        view.Initialize(controller);
        return view;
    }

    public void UpdateSelectedBody(WearableItem bodyShape)
    {
        for (int i = 0; i < wearableGridPairs.Length; i++)
        {
            if (wearableGridPairs[i].categoryFilter == WearableLiterals.Categories.BODY_SHAPE)
            {
                wearableGridPairs[i].selector.UnselectAll();
                wearableGridPairs[i].selector.Select(bodyShape.id);
            }
            else
            {
                wearableGridPairs[i].selector.SetBodyShape(bodyShape.id);
            }
        }

        collectiblesItemSelector.SetBodyShape(bodyShape.id);
    }

    public void EquipWearable(WearableItem wearable)
    {
        selectorsByCategory[wearable.data.category].Select(wearable.id);
        SetWearableLoadingSpinner(wearable, true);
        collectiblesItemSelector.Select(wearable.id);
    }

    public void UnequipWearable(WearableItem wearable)
    {
        selectorsByCategory[wearable.data.category].Unselect(wearable.id);
        SetWearableLoadingSpinner(wearable, false);
        collectiblesItemSelector.Unselect(wearable.id);
    }

    internal void SetWearableLoadingSpinner(WearableItem wearable, bool isActive)
    {
        selectorsByCategory[wearable.data.category].SetWearableLoadingSpinner(wearable.id, isActive);
        collectiblesItemSelector.SetWearableLoadingSpinner(wearable.id, isActive);
        if (isActive)
            wearablesWithLoadingSpinner.Add(wearable);
        else
            wearablesWithLoadingSpinner.Remove(wearable);
    }

    internal void ClearWearablesLoadingSpinner()
    {
        foreach (WearableItem wearable in wearablesWithLoadingSpinner)
        {
            selectorsByCategory[wearable.data.category].SetWearableLoadingSpinner(wearable.id, false);
            collectiblesItemSelector.SetWearableLoadingSpinner(wearable.id, false);
        }

        wearablesWithLoadingSpinner.Clear();
    }

    public void SelectHairColor(Color hairColor) 
    {
        hairColorPickerComponent.SetColorSelector(hairColor);
        hairColorPickerComponent.UpdateSliderValues(hairColor);
    }

    public Color GetRandomColor()
    { 
        return Color.HSVToRGB(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
    }

    public void SelectSkinColor(Color skinColor) 
    { 
        skinColorSelector.Select(skinColor);
        /* UNCOMMENT TO ENABLE SKIN COLOR PICKER
         * skinColorPicker.UpdateSliderValues(skinColor);
         */
    }

    public void SelectEyeColor(Color eyesColor) 
    {
        eyeColorPickerComponent.SetColorSelector(eyesColor);
        eyeColorPickerComponent.UpdateSliderValues(eyesColor);
    }

    public void SetColors(List<Color> skinColors, List<Color> hairColors, List<Color> eyeColors)
    {
        skinColorSelector.Populate(skinColors);
        eyeColorPickerComponent.SetColorList(eyeColors);
        hairColorPickerComponent.SetColorList(hairColors);
    }

    public void UnselectAllWearables()
    {
        for (int i = 0; i < wearableGridPairs.Length; i++)
        {
            if (wearableGridPairs[i].categoryFilter != WearableLiterals.Categories.BODY_SHAPE)
            {
                wearableGridPairs[i].selector.UnselectAll();
            }
        }

        collectiblesItemSelector.UnselectAll();
    }

    public void UpdateAvatarPreview(AvatarModel avatarModel)
    {
        if (IsAvatarPreviewLoading(avatarModel))
            return;

        doneButton.interactable = false;
        loadingSpinnerGameObject.SetActive(true);
        characterPreviewController.UpdateModel(avatarModel,
            () =>
            {
                if (doneButton != null)
                    doneButton.interactable = true;

                loadingSpinnerGameObject?.SetActive(false);
                OnAvatarAppear?.Invoke(avatarModel);
                ClearWearablesLoadingSpinner();
                randomizeAnimator?.SetBool(RANDOMIZE_ANIMATOR_LOADING_BOOL, false);
            });
    }

    private bool IsAvatarPreviewLoading(AvatarModel avatarModel) { return avatarModel?.wearables == null || loadingSpinnerGameObject.activeSelf; }

    public void AddWearable(WearableItem wearableItem, int amount,
        Func<WearableItem, bool> hideOtherWearablesToastStrategy,
        Func<WearableItem, bool> replaceOtherWearablesToastStrategy)
    {
        if (wearableItem == null)
            return;

        if (!selectorsByCategory.ContainsKey(wearableItem.data.category))
        {
            Debug.LogError($"Category couldn't find selector for category: {wearableItem.data.category} ");
            return;
        }

        selectorsByCategory[wearableItem.data.category].AddItemToggle(wearableItem, amount,
            hideOtherWearablesToastStrategy, replaceOtherWearablesToastStrategy);
        if (wearableItem.IsCollectible())
        {
            collectiblesItemSelector.AddItemToggle(wearableItem, amount,
                hideOtherWearablesToastStrategy, replaceOtherWearablesToastStrategy);
        }
    }

    public void RemoveWearable(WearableItem wearableItem)
    {
        if (wearableItem == null)
            return;

        if (!selectorsByCategory.ContainsKey(wearableItem.data.category))
        {
            Debug.LogError($"Category couldn't find selector for category: {wearableItem.data.category} ");
            return;
        }

        selectorsByCategory[wearableItem.data.category].RemoveItemToggle(wearableItem.id);
        if (wearableItem.IsCollectible())
            collectiblesItemSelector.RemoveItemToggle(wearableItem.id);
    }

    public void RemoveAllWearables()
    {
        using (var enumerator = selectorsByCategory.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.RemoveAllItemToggle();
            }
        }

        collectiblesItemSelector.RemoveAllItemToggle();
    }

    private void OnRandomizeButton()
    {
        OnRandomize?.Invoke();
        controller.RandomizeWearables();
        randomizeAnimator?.SetBool(RANDOMIZE_ANIMATOR_LOADING_BOOL, true);
    }

    private void OnDoneButton()
    {
        doneButton.interactable = false;
        CoroutineStarter.Start(TakeSnapshotsAfterStopPreviewAnimation());

    }

    private IEnumerator TakeSnapshotsAfterStopPreviewAnimation()
    {
        // We need to stop the current preview animation in order to take a correct snapshot
        ResetPreviewEmote();
        yield return new WaitForSeconds(TIME_TO_RESET_PREVIEW_ANIMATION);
        characterPreviewController.TakeSnapshots(OnSnapshotsReady, OnSnapshotsFailed);
    }

    private void OnSnapshotsReady(Texture2D face256, Texture2D body)
    {
        doneButton.interactable = true;
        controller.SaveAvatar(face256, body);
    }

    private void OnSnapshotsFailed() { doneButton.interactable = true; }

    public void SetVisibility(bool visible)
    {
        characterPreviewController.camera.enabled = visible;
        avatarEditorCanvas.enabled = visible;
        avatarEditorCanvasGroup.blocksRaycasts = visible;

        if (visible && !isOpen)
            OnSetVisibility?.Invoke(visible);
        else if (!visible && isOpen)
            OnSetVisibility?.Invoke(visible);

        isOpen = visible;
    }

    public void CleanUp()
    {
        loadingSpinnerGameObject = null;
        randomizeAnimator = null;
        if (wearableGridPairs != null)
        {
            int nPairs = wearableGridPairs.Length;
            for (int i = 0; i < nPairs; i++)
            {
                var itemSelector = wearableGridPairs[i].selector;
                if (itemSelector != null)
                {
                    itemSelector.OnItemClicked -= controller.WearableClicked;
                    itemSelector.OnSellClicked -= controller.SellCollectible;
                }
            }
        }

        if (collectiblesItemSelector != null)
        {
            collectiblesItemSelector.OnItemClicked -= controller.WearableClicked;
            collectiblesItemSelector.OnSellClicked -= controller.SellCollectible;
            collectiblesItemSelector.OnRetryClicked -= controller.RetryLoadOwnedWearables;
        }

        if (skinColorSelector != null)
            skinColorSelector.OnColorSelectorChange -= controller.SkinColorClicked;
        /*  UNCOMMENT TO ENABLE SKIN COLOR PICKER
         * if (skinColorPicker != null)
         *  skinColorPicker.OnColorChanged -= controller.SkinColorClicked;
         */
        if (eyeColorPickerComponent != null)
            eyeColorPickerComponent.OnColorChanged -= controller.EyesColorClicked;
        if (hairColorPickerComponent != null)
            hairColorPickerComponent.OnColorChanged -= controller.HairColorClicked;

        if (this != null)
            Destroy(gameObject);

        if (characterPreviewController != null)
        {
            Destroy(characterPreviewController.gameObject);
            characterPreviewController = null;
        }

        sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.RemoveAllListeners();
        sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.RemoveAllListeners();
    }

    public void ShowCollectiblesLoadingSpinner(bool isActive) { collectiblesItemSelector.ShowLoading(isActive); }

    public void ShowCollectiblesLoadingRetry(bool isActive) { collectiblesItemSelector.ShowRetryLoading(isActive); }

    public void SetAsFullScreenMenuMode(Transform parentTransform)
    {
        if (parentTransform == null)
            return;

        transform.SetParent(parentTransform);
        transform.localScale = Vector3.one;

        RectTransform rectTransform = transform as RectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localPosition = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject != eyeColorPickerComponent.gameObject && eventData.pointerPressRaycast.gameObject != hairColorPickerComponent.gameObject)
        {
            eyeColorPickerComponent.SetActive(false);
            hairColorPickerComponent.SetActive(false);
        }
    }

    public void ShowSkinPopulatedList(bool show)
    {
        skinsPopulatedListContainer.SetActive(show);
        skinsEmptyListContainer.SetActive(!show);
        skinsConnectWalletButtonContainer.SetActive(show);
    }

    internal void ConfigureSectionSelector()
    {
        sectionTitle.text = AVATAR_SECTION_TITLE;

        sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.AddListener((isSelected) =>
        {
            avatarSection.SetActive(isSelected);
            randomizeButton.gameObject.SetActive(true);

            if (isSelected)
            {
                sectionTitle.text = AVATAR_SECTION_TITLE;
                ResetPreviewEmote();
            }

            emotesCustomizationDataStore.isEmotesCustomizationSelected.Set(false, notifyEvent: false);
        });
        sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.AddListener((isSelected) =>
        {
            emotesSection.SetActive(isSelected);
            randomizeButton.gameObject.SetActive(false);

            if (isSelected)
            {
                sectionTitle.text = EMOTES_SECTION_TITLE;
                ResetPreviewEmote();
            }

            characterPreviewController.SetFocus(CharacterPreviewController.CameraFocus.DefaultEditing);
            emotesCustomizationDataStore.isEmotesCustomizationSelected.Set(true, notifyEvent: false);
        });
    }

    internal void SetSectionActive(int sectionIndex, bool isActive) 
    { 
        sectionSelector.GetSection(sectionIndex).SetActive(isActive);
        sectionSelector.gameObject.SetActive(sectionSelector.GetAllSections().Count(x => x.IsActive()) > 1);
    }
    
    public void PlayPreviewEmote(string emoteId) { characterPreviewController.PlayEmote(emoteId, (long)Time.realtimeSinceStartup); }

    public void ResetPreviewEmote() { PlayPreviewEmote(RESET_PREVIEW_ANIMATION); }
}