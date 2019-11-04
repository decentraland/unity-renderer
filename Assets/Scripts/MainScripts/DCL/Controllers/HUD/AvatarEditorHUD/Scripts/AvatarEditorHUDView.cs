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

    [SerializeField] private Canvas avatarEditorCanvas;
    [SerializeField] private CanvasGroup avatarEditorCanvasGroup;
    [SerializeField] AvatarEditorNavigationInfo[] navigationInfos;
    [SerializeField] AvatarEditorWearableFilter[] wearableGridPairs;
    [SerializeField] ColorSelector skinColorSelector;
    [SerializeField] ColorSelector eyeColorSelector;
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
        ItemToggle.getEquippedWearablesReplacedByFunc = controller.GetWearablesReplacedBy;
        this.controller = controller;
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
            wearableGridPairs[i].selector.OnItemClicked += controller.WearableClicked;
            selectorsByCategory.Add(wearableGridPairs[i].categoryFilter, wearableGridPairs[i].selector);
        }

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
            if (wearableGridPairs[i].categoryFilter == WearableItem.bodyShapeCategory)
            {
                wearableGridPairs[i].selector.UnselectAll();
                wearableGridPairs[i].selector.Select(bodyShape.id);
            }
            else
            {
                wearableGridPairs[i].selector.SetBodyShape(bodyShape.id);
            }
        }
    }

    public void SelectWearable(WearableItem wearable)
    {
        selectorsByCategory[wearable.category].Select(wearable.id);
    }

    public void UnselectWearable(WearableItem wearable)
    {
        selectorsByCategory[wearable.category].Unselect(wearable.id);
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
            if (wearableGridPairs[i].categoryFilter != WearableItem.bodyShapeCategory)
            {
                wearableGridPairs[i].selector.UnselectAll();
            }
        }
    }

    public void UpdateAvatarPreview(AvatarModel avatarModel)
    {
        if (avatarModel?.wearables == null) return;

        SetLoadingPanel(true);
        characterPreviewController.UpdateModel(avatarModel, () => SetLoadingPanel(false));
    }

    private void SetLoadingPanel(bool active)
    {
        loadingPanel.SetActive(active);
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
        controller.RandomizeWearables();
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

    public void CleanUp()
    {
        Destroy(gameObject);
    }
}