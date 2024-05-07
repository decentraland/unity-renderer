using DCL;
using DCL.ContentModeration;
using DCLServices.MapRendererV2.ConsumerUtils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapHUDView : MonoBehaviour
{
    public const string VIEW_PATH = "MinimapHUD";
    public const string VIEW_OBJECT_NAME = "_MinimapHUD";

    public event Action<string, bool> OnFavoriteToggleClicked;

    [Header("Information")] [SerializeField]
    private TextMeshProUGUI sceneNameText;

    [SerializeField] private TextMeshProUGUI playerPositionText;
    [SerializeField] internal ShowHideAnimator mainShowHideAnimator;
    [SerializeField] private Button openNavmapButton;

    [Header("Options")] [SerializeField] private Button optionsButton;
    [SerializeField] internal GameObject sceneOptionsPanel;
    [SerializeField] private ToggleComponentView toggleSceneUI;
    [SerializeField] internal Button reportSceneButton;
    [SerializeField] internal ContentModerationReportingButtonComponentView contentModerationReportButton;
    [SerializeField] internal ToggleComponentView setHomeScene;
    [SerializeField] internal FavoriteButtonComponentView favoriteToggle;
    [SerializeField] internal Image disableFavorite;
    [SerializeField] internal Button copyLocationButton;
    [SerializeField] internal ShowHideAnimator locationCopiedToast;

    [Header("Map Renderer")]
    public RectTransform mapRenderContainer;

    [field: SerializeField]
    internal RawImage mapRendererTargetImage { get; private set; }

    [field: SerializeField]
    internal PixelPerfectMapRendererTextureProvider pixelPerfectMapRendererTextureProvider { get; private set; }

    [field: SerializeField]
    internal int mapRendererVisibleParcels { get; private set; }

    internal IContentModerationReportingButtonComponentView contentModerationButton => contentModerationReportButton;

    public RectTransform mapViewport;

    public InputAction_Trigger toggleNavMapAction;
    private IMouseCatcher mouseCatcher;
    private HUDCanvasCameraModeController hudCanvasCameraModeController;
    private MinimapHUDController controller;

    public event Action OnCopyLocationRequested;

    private void Awake()
    {
        hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);
        favoriteToggle.OnFavoriteChange += (uuid, isFavorite) => OnFavoriteToggleClicked?.Invoke(uuid, isFavorite);
    }

    public void Initialize(MinimapHUDController controller)
    {
        this.controller = controller;
        mouseCatcher = SceneReferences.i?.mouseCatcher;
        gameObject.name = VIEW_OBJECT_NAME;
        sceneOptionsPanel.SetActive(false);

        optionsButton.onClick.AddListener(controller.ToggleOptions);
        toggleSceneUI.OnSelectedChanged += (isOn, id, name) => controller.ToggleSceneUI(isOn);
        reportSceneButton.onClick.AddListener(ReportScene);
        setHomeScene.OnSelectedChanged += (isOn, id, name) => SetHomeScene(isOn);
        openNavmapButton.onClick.AddListener(toggleNavMapAction.RaiseOnTriggered);
        copyLocationButton.onClick.AddListener(() => OnCopyLocationRequested?.Invoke());

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock += OnMouseLocked;

        /*var renderer = MapRenderer.i;

        if (renderer != null)
        {
            renderer.atlas.viewport = mapViewport;
            renderer.transform.SetParent(mapRenderContainer);
            renderer.transform.SetAsFirstSibling();
        }*/
    }

    private void ReportScene()
    {
        controller.ReportScene();
        controller.ToggleOptions();
    }

    private void SetHomeScene(bool isOn)
    {
        controller.SetHomeScene(isOn);
    }

    internal void OnMouseLocked()
    {
        sceneOptionsPanel.SetActive(false);
    }

    internal static MinimapHUDView Create(MinimapHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<MinimapHUDView>();
        view.Initialize(controller);
        return view;
    }

    internal void UpdateSetHomePanel(bool isHome)
    {
        setHomeScene.SetIsOnWithoutNotify(isHome);
    }

    internal void UpdateData(MinimapHUDModel model)
    {
        sceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? "Unnamed" : model.sceneName;
        playerPositionText.text = model.playerPosition;
    }

    public void ToggleOptions()
    {
        sceneOptionsPanel.SetActive(!sceneOptionsPanel.activeSelf);
    }

    public void SetVisibility(bool visible)
    {
        if (visible && !mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Show();
        else if (!visible && mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Hide();
    }

    public void SetCurrentFavoriteStatus(string uuid, bool isFavorite)
    {
        favoriteToggle.Configure(new FavoriteButtonComponentModel()
        {
            placeUUID = uuid,
            isFavorite = isFavorite
        });
    }

    public void SetIsAPlace(bool isAPlace)
    {
        favoriteToggle.gameObject.SetActive(isAPlace);
        disableFavorite.gameObject.SetActive(!isAPlace);
    }

    public void ShowLocationCopiedToast()
    {
        locationCopiedToast.gameObject.SetActive(true);
        locationCopiedToast.ShowDelayHide(3);
    }

    public void SetReportSceneButtonActive(bool isActive) =>
        reportSceneButton.gameObject.SetActive(isActive);

    private void OnDestroy()
    {
        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock -= OnMouseLocked;

        hudCanvasCameraModeController?.Dispose();
    }
}
