using System;
using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapHUDView : MonoBehaviour
{
    public const string VIEW_PATH = "MinimapHUD";
    public const string VIEW_OBJECT_NAME = "_MinimapHUD";

    private int START_MENU_HOVER_BOOL = Animator.StringToHash("hover");
    private int START_MENU_PRESSED_TRIGGER = Animator.StringToHash("pressed");

    [Header("Information")] [SerializeField]
    private TextMeshProUGUI sceneNameText;

    [SerializeField] private TextMeshProUGUI playerPositionText;
    [SerializeField] internal ShowHideAnimator mainShowHideAnimator;
    [SerializeField] private Button openNavmapButton;

    [Header("Options")] [SerializeField] private Button optionsButton;
    [SerializeField] private GameObject sceneOptionsPanel;
    [SerializeField] private ToggleComponentView toggleSceneUI;
    [SerializeField] private Button addBookmarkButton;
    [SerializeField] private Button reportSceneButton;
    [SerializeField] internal UsersAroundListHUDButtonView usersAroundListHudButton;

    [Header("Map Renderer")] public RectTransform mapRenderContainer;
    public RectTransform mapViewport;

    public static System.Action<MinimapHUDModel> OnUpdateData;
    public static System.Action OnOpenNavmapClicked;
    public InputAction_Trigger toggleNavMapAction;
    private IMouseCatcher mouseCatcher;
    private HUDCameraCanvasHelper hudCameraCanvasHelper;

    private void Awake() { hudCameraCanvasHelper = new HUDCameraCanvasHelper(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera); }

    public void Initialize(MinimapHUDController controller)
    {
        mouseCatcher = SceneReferences.i?.mouseCatcher;
        gameObject.name = VIEW_OBJECT_NAME;
        sceneOptionsPanel.SetActive(false);

        optionsButton.onClick.AddListener(controller.ToggleOptions);
        toggleSceneUI.OnSelectedChanged += (isOn, id, name) => controller.ToggleSceneUI(isOn);
        addBookmarkButton.onClick.AddListener(controller.AddBookmark);
        reportSceneButton.onClick.AddListener(controller.ReportScene);
        openNavmapButton.onClick.AddListener(toggleNavMapAction.RaiseOnTriggered);

        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock += OnMouseLocked;

        var renderer = MapRenderer.i;

        if (renderer != null)
        {
            renderer.atlas.viewport = mapViewport;
            renderer.transform.SetParent(mapRenderContainer);
            renderer.transform.SetAsFirstSibling();
        }
        usersAroundListHudButton.gameObject.SetActive(false);
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

    internal void UpdateData(MinimapHUDModel model)
    {
        sceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? "Unnamed" : model.sceneName;
        playerPositionText.text = model.playerPosition;
    }

    public void ToggleOptions() { sceneOptionsPanel.SetActive(!sceneOptionsPanel.activeSelf); }

    public void SetVisibility(bool visible)
    {
        if (visible && !mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Show();
        else if (!visible && mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Hide();
    }

    private void OnDestroy()
    {
        if(mouseCatcher != null)
            mouseCatcher.OnMouseLock -= OnMouseLocked;
        hudCameraCanvasHelper?.Dispose();
    }
}