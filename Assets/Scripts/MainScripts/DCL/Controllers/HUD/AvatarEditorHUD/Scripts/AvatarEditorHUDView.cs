using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarEditorHUDView : MonoBehaviour
{
    private const string VIEW_PATH = "AvatarEditorHUD";
    private const string VIEW_OBJECT_NAME = "_AvatarEditorHUD";

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

    public event Action<WearableItem> OnItemSelected;
    public event Action<string> OnItemDeselected;
    public event Action<Color> OnHairColorChanged;
    public event Action<Color> OnSkinColorChanged;
    public event Action<Color> OnEyeColorChanged;

    [SerializeField] private Canvas avatarEditorCanvas;
    [SerializeField] private CanvasGroup avatarEditorCanvasGroup;
    [SerializeField] AvatarEditorNavigationInfo[] navigationInfos;
    [SerializeField] AvatarEditorWearableFilter[] wearableGridPairs;
    [SerializeField] ColorList skinColorList;
    [SerializeField] ColorSelector skinColorSelector;
    [SerializeField] ColorList eyeColorList;
    [SerializeField] ColorSelector eyeColorSelector;
    [SerializeField] ColorList hairColorList;
    [SerializeField] ColorSelector hairColorSelector;
    [SerializeField] GameObject characterPreviewPrefab;
    [SerializeField] private PreviewCameraRotation characterPreviewRotation;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] Button randomizeButton;
    [SerializeField] Button doneButton;
    [SerializeField] Button exitButton;

    private CharacterPreviewController characterPreviewController;
    private AvatarEditorHUDController controller;
    private readonly Dictionary<string, ItemSelector> selectorsByCategory = new Dictionary<string, ItemSelector>();
    private readonly Dictionary<string, WearableItem> wearables = new Dictionary<string, WearableItem>();

    private void Awake()
    {
        characterPreviewController = GameObject.Instantiate(characterPreviewPrefab).GetComponent<CharacterPreviewController>();
        characterPreviewController.name = "_CharacterPreviewController";
    }

    private void Initialize(AvatarEditorHUDController controller)
    {
        this.controller = controller;
        ItemToggle.getWearablesReplacedByFunc = this.controller.GetWearablesReplacedBy;
        gameObject.name = VIEW_OBJECT_NAME;

        randomizeButton.onClick.AddListener(OnRandomizeButton);
        doneButton.onClick.AddListener(OnDoneButton);
        exitButton.onClick.AddListener(OnExitButton);
        InitializeNavigationEvents();
        InitializeWearableChangeEvents();
    }

    private void InitializeNavigationEvents()
    {
        for (int i = 0; i < navigationInfos.Length; i++)
        {
            AvatarEditorNavigationInfo current = navigationInfos[i];

            current.Initialize();
            current.toggle.isOn = current.enabledByDefault;
            current.canvas.enabled = current.enabledByDefault;
            current.toggle.onValueChanged.AddListener((on) =>
            {
                current.canvas.enabled = @on;
                characterPreviewController.SetFocus(current.focus);
            });
        }

        characterPreviewRotation.OnHorizontalRotation += characterPreviewController.Rotate;
    }

    private void InitializeWearableChangeEvents()
    {
        int nPairs = wearableGridPairs.Length;
        for (int i = 0; i < nPairs; i++)
        {
            wearableGridPairs[i].selector.OnItemEquipped += (x) => OnItemSelected?.Invoke(x);
            wearableGridPairs[i].selector.OnItemUnequipped += (x) => OnItemDeselected?.Invoke(x);

            selectorsByCategory.Add(wearableGridPairs[i].categoryFilter, wearableGridPairs[i].selector);
        }

        skinColorSelector.OnColorChanged += (x) => OnSkinColorChanged?.Invoke(x);
        skinColorSelector.Populate(skinColorList.colors);

        eyeColorSelector.OnColorChanged += (x) => OnEyeColorChanged?.Invoke(x);
        eyeColorSelector.Populate(eyeColorList.colors);

        hairColorSelector.OnColorChanged += (x) => OnHairColorChanged?.Invoke(x);
        hairColorSelector.Populate(hairColorList.colors);
    }

    internal static AvatarEditorHUDView Create(AvatarEditorHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<AvatarEditorHUDView>();
        view.Initialize(controller);
        return view;
    }

    internal void UpdateAvatarModel(AvatarModel avatarModel)
    {
        if (avatarModel?.wearables == null) return;

        using (var iterator = selectorsByCategory.GetEnumerator())
        {
            while (iterator.MoveNext()) iterator.Current.Value.Unselect();
        }
        UpdateSelectedBody(avatarModel);
        UpdateSelectedColors(avatarModel);
        UpdateSelectedWearables(avatarModel);

        UpdateAvatarPreview(avatarModel);
    }

    internal void UpdateAvatarPreview(AvatarModel avatarModel)
    {
        if (avatarModel?.wearables == null) return;

        SetLoadingPanel(true);
        characterPreviewController.UpdateModel(avatarModel, () => SetLoadingPanel(false));
    }

    private void SetLoadingPanel(bool active)
    {
        loadingPanel.SetActive(active);
    }

    internal void UpdateSelectedBody(AvatarModel avatarModel)
    {
        for (int i = 0; i < wearableGridPairs.Length; i++)
        {
            if (wearableGridPairs[i].categoryFilter == WearableItem.bodyShapeCategory)
            {
                wearableGridPairs[i].selector.Select(avatarModel.bodyShape);
            }
            else
            {
                wearableGridPairs[i].selector.SetBodyShape(avatarModel.bodyShape);
            }
        }
    }

    internal void UpdateSelectedColors(AvatarModel avatarModel)
    {
        skinColorSelector.Select(avatarModel.skinColor);
        hairColorSelector.Select(avatarModel.hairColor);
        eyeColorSelector.Select(avatarModel.eyeColor);
    }

    private void UpdateSelectedWearables(AvatarModel avatarModel)
    {
        // If the wearables.Count is cached, the first line inside the loop causes an index out of range exception
        for (int i = 0; i < avatarModel.wearables.Count; i++)
        {
            WearableItem wearableItem = CatalogController.wearableCatalog.Get(avatarModel.wearables[i]);
            if (wearableItem != null)
            {
                selectorsByCategory[wearableItem.category].Select(avatarModel.wearables[i]);
            }
        }
    }

    public void AddWearable(WearableItem wearableItem)
    {
        if (wearableItem == null) return;

        if (!selectorsByCategory.ContainsKey(wearableItem.category))
        {
            Debug.LogError($"Category couldn't find selector for category: {wearableItem.category} ");
            return;
        }

        selectorsByCategory[wearableItem.category].AddItemToggle(wearableItem);
        wearables.Remove(wearableItem.id);
    }

    public void RemoveWearable(WearableItem wearableItem)
    {
        if (wearableItem == null) return;

        if (!selectorsByCategory.ContainsKey(wearableItem.category))
        {
            Debug.LogError($"Category couldn't find selector for category: {wearableItem.category} ");
            return;
        }

        selectorsByCategory[wearableItem.category].RemoveItemToggle(wearableItem.id);
    }

    private void OnRandomizeButton()
    {
        using (var enumerator = selectorsByCategory.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Key != WearableItem.bodyShapeCategory)
                {
                    enumerator.Current.Value.SelectRandom();
                }
            }
        }

        hairColorSelector.SelectRandom();
        eyeColorSelector.SelectRandom();
    }

    private void OnDoneButton()
    {
        doneButton.interactable = false;
        characterPreviewController.TakeSnapshots(OnSnapshotsReady);
    }

    private void OnExitButton()
    {
        controller.DiscardAndClose();
    }

    private void OnSnapshotsReady(Texture2D face, Texture2D body)
    {
        doneButton.interactable = true;
        controller.SaveAvatar(face, body);
    }

    public void SetVisibility(bool visible)
    {
        avatarEditorCanvas.enabled = visible;
        avatarEditorCanvasGroup.blocksRaycasts = visible;
    }
}