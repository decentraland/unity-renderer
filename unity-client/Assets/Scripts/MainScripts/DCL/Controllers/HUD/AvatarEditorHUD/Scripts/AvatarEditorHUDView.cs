using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[assembly: InternalsVisibleTo("AvatarEditorHUDTests")]

public class AvatarEditorHUDView : MonoBehaviour
{
    private const string VIEW_PATH = "AvatarEditorHUD";
    private const string VIEW_OBJECT_NAME = "_AvatarEditorHUD";

    public bool isOpen { get; private set; }

    [System.Serializable]
    public class AvatarEditorNavigationInfo
    {
        public Toggle toggle;
        public Canvas canvas;
        public bool enabledByDefault;
        public CharacterPreviewController.CameraFocus focus = CharacterPreviewController.CameraFocus.DefaultEditing;

        //To remove when we refactor this to avoid ToggleGroup issues when quitting application
        public void Initialize()
        {
            Application.quitting += () => toggle.onValueChanged.RemoveAllListeners();
        }
    }

    [System.Serializable]
    public class AvatarEditorWearableFilter
    {
        public string categoryFilter;
        public ItemSelector selector;
    }

    [SerializeField]
    internal InputAction_Trigger toggleAction;

    [SerializeField]
    internal InputAction_Trigger closeAction;

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
    internal ColorSelector eyeColorSelector;

    [SerializeField]
    internal ColorSelector hairColorSelector;

    [SerializeField]
    internal GameObject characterPreviewPrefab;

    [SerializeField]
    internal PreviewCameraRotation characterPreviewRotation;

    [SerializeField]
    internal GameObject loadingPanel;

    [SerializeField]
    internal Button randomizeButton;

    [SerializeField]
    internal Button doneButton;

    [SerializeField]
    internal Button exitButton;

    [Header("Collectibles")]
    [SerializeField]
    internal GameObject web3Container;

    [SerializeField]
    internal Button web3GoToMarketplaceButton;

    [SerializeField]
    internal GameObject noWeb3Container;

    [SerializeField]
    internal Button noWeb3GoToMarketplaceButton;

    internal static CharacterPreviewController characterPreviewController;
    private AvatarEditorHUDController controller;
    internal readonly Dictionary<string, ItemSelector> selectorsByCategory = new Dictionary<string, ItemSelector>();

    public event System.Action<AvatarModel> OnAvatarAppear;
    public event System.Action<bool> OnSetVisibility;
    public event System.Action OnRandomize;
    public event System.Action OnToggleActionTriggered;
    public event System.Action OnCloseActionTriggered;

    private void Awake()
    {
        toggleAction.OnTriggered += ToggleAction_OnTriggered;
        closeAction.OnTriggered += CloseAction_OnTriggered;

        if (characterPreviewController == null)
        {
            characterPreviewController = GameObject.Instantiate(characterPreviewPrefab).GetComponent<CharacterPreviewController>();
            characterPreviewController.name = "_CharacterPreviewController";
        }

        isOpen = false;
    }

    private void OnDestroy()
    {
        toggleAction.OnTriggered -= ToggleAction_OnTriggered;
        closeAction.OnTriggered -= CloseAction_OnTriggered;
    }

    private void ToggleAction_OnTriggered(DCLAction_Trigger action)
    {
        OnToggleActionTriggered?.Invoke();
    }

    private void CloseAction_OnTriggered(DCLAction_Trigger action)
    {
        OnCloseActionTriggered?.Invoke();
    }

    private void Initialize(AvatarEditorHUDController controller)
    {
        ItemToggle.getEquippedWearablesReplacedByFunc = controller.GetWearablesReplacedBy;
        this.controller = controller;
        gameObject.name = VIEW_OBJECT_NAME;

        randomizeButton.onClick.AddListener(OnRandomizeButton);
        doneButton.onClick.AddListener(OnDoneButton);
        exitButton.onClick.AddListener(OnExitButton);
        InitializeNavigationEvents();
        InitializeWearableChangeEvents();

        web3GoToMarketplaceButton.onClick.RemoveAllListeners();
        noWeb3GoToMarketplaceButton.onClick.RemoveAllListeners();
        web3GoToMarketplaceButton.onClick.AddListener(controller.GoToMarketplace);
        noWeb3GoToMarketplaceButton.onClick.AddListener(controller.GoToMarketplace);

        characterPreviewController.camera.enabled = false;
    }

    public void SetIsWeb3(bool isWeb3User)
    {
        web3Container.SetActive(isWeb3User);
        noWeb3Container.SetActive(!isWeb3User);
    }

    private void InitializeNavigationEvents()
    {
        for (int i = 0; i < navigationInfos.Length; i++)
        {
            InitializeNavigationInfo(navigationInfos[i]);
        }

        InitializeNavigationInfo(collectiblesNavigationInfo);

        characterPreviewRotation.OnHorizontalRotation += characterPreviewController.Rotate;
    }

    private void InitializeNavigationInfo(AvatarEditorNavigationInfo current)
    {
        current.Initialize();

        current.toggle.isOn = current.enabledByDefault;

        current.canvas.gameObject.SetActive(current.enabledByDefault);
        current.toggle.onValueChanged.AddListener((on) =>
        {
            current.canvas.gameObject.SetActive(@on);
            characterPreviewController.SetFocus(current.focus);
        });
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

        skinColorSelector.OnColorChanged += controller.SkinColorClicked;
        eyeColorSelector.OnColorChanged += controller.EyesColorClicked;
        hairColorSelector.OnColorChanged += controller.HairColorClicked;
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

    public void SelectWearable(WearableItem wearable)
    {
        selectorsByCategory[wearable.category].Select(wearable.id);
        collectiblesItemSelector.Select(wearable.id);
    }

    public void UnselectWearable(WearableItem wearable)
    {
        selectorsByCategory[wearable.category].Unselect(wearable.id);
        collectiblesItemSelector.Unselect(wearable.id);
    }

    public void SelectHairColor(Color hairColor)
    {
        hairColorSelector.Select(hairColor);
    }

    public void SelectSkinColor(Color skinColor)
    {
        skinColorSelector.Select(skinColor);
    }

    public void SelectEyeColor(Color eyesColor)
    {
        eyeColorSelector.Select(eyesColor);
    }

    public void SetColors(List<Color> skinColors, List<Color> hairColors, List<Color> eyeColors)
    {
        skinColorSelector.Populate(skinColors);
        eyeColorSelector.Populate(eyeColors);
        hairColorSelector.Populate(hairColors);
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
        if (avatarModel?.wearables == null)
            return;

        SetLoadingPanel(true);
        doneButton.interactable = false;
        characterPreviewController.UpdateModel(avatarModel,
            () =>
            {
                SetLoadingPanel(false);

                if (doneButton != null)
                    doneButton.interactable = true;

                OnAvatarAppear?.Invoke(avatarModel);
            });
    }

    private void SetLoadingPanel(bool active)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(active);
    }

    public void AddWearable(WearableItem wearableItem, int amount)
    {
        if (wearableItem == null)
            return;

        if (!selectorsByCategory.ContainsKey(wearableItem.category))
        {
            Debug.LogError($"Category couldn't find selector for category: {wearableItem.category} ");
            return;
        }

        selectorsByCategory[wearableItem.category].AddItemToggle(wearableItem, amount);
        if (wearableItem.IsCollectible())
        {
            collectiblesItemSelector.AddItemToggle(wearableItem, amount);
        }
    }

    public void RemoveWearable(WearableItem wearableItem)
    {
        if (wearableItem == null)
            return;

        if (!selectorsByCategory.ContainsKey(wearableItem.category))
        {
            Debug.LogError($"Category couldn't find selector for category: {wearableItem.category} ");
            return;
        }

        selectorsByCategory[wearableItem.category].RemoveItemToggle(wearableItem.id);
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
    }

    private void OnDoneButton()
    {
        doneButton.interactable = false;
        characterPreviewController.TakeSnapshots(OnSnapshotsReady, OnSnapshotsFailed);
    }

    private void OnExitButton()
    {
        OnCloseActionTriggered?.Invoke();
    }

    private void OnSnapshotsReady(Texture2D face, Texture2D face128, Texture2D face256, Texture2D body)
    {
        doneButton.interactable = true;
        controller.SaveAvatar(face, face128, face256, body);

        characterPreviewController.ResetRenderersLayer();
    }

    private void OnSnapshotsFailed()
    {
        doneButton.interactable = true;
        characterPreviewController.ResetRenderersLayer();
    }

    public void SetVisibility(bool visible)
    {
        characterPreviewController.camera.enabled = visible;
        avatarEditorCanvas.enabled = visible;
        avatarEditorCanvasGroup.blocksRaycasts = visible;

        if (visible && !isOpen)
        {
            AudioScriptableObjects.dialogOpen.Play(true);
            OnSetVisibility?.Invoke(visible);
        }
        else if (!visible && isOpen)
        {
            AudioScriptableObjects.dialogClose.Play(true);
            OnSetVisibility?.Invoke(visible);
        }

        isOpen = visible;
    }

    public void CleanUp()
    {
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
        }

        if (skinColorSelector != null)
            skinColorSelector.OnColorChanged -= controller.SkinColorClicked;
        if (eyeColorSelector != null)
            eyeColorSelector.OnColorChanged -= controller.EyesColorClicked;
        if (hairColorSelector != null)
            hairColorSelector.OnColorChanged -= controller.HairColorClicked;

        if (this != null)
            Destroy(gameObject);

        if (characterPreviewController != null)
        {
            Destroy(characterPreviewController.gameObject);
            characterPreviewController = null;
        }
    }
}