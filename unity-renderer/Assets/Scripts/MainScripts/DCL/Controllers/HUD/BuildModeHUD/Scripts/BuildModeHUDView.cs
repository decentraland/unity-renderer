using UnityEngine;

public interface IBuildModeHUDView
{
    GameObject viewGO { get; }
    SceneCatalogView sceneCatalog { get; }
    EntityInformationView entityInformation { get; }
    bool isShowHideAnimatorVisible { get; }

    void AnimatorShow(bool isVisible);
    void HideToolTip();
    void Initialize(BuildModeHUDInitializationModel controllers);
    void RefreshCatalogAssetPack();
    void RefreshCatalogContent();
    void SetActive(bool isActive);
    void SetFirstPersonView();
    void SetGodModeView();
    void SetPublishBtnAvailability(bool isAvailable);
    void SetVisibilityOfCatalog(bool isVisible);
    void SetVisibilityOfControls(bool isVisible);
    void SetVisibilityOfExtraBtns(bool isVisible);
    void SetVisibilityOfSceneInfo(bool isVisible);
}

public class BuildModeHUDView : MonoBehaviour, IBuildModeHUDView
{
    public GameObject viewGO => !isDestroyed ? gameObject : null;
    public SceneCatalogView sceneCatalog => sceneCatalogView;
    public EntityInformationView entityInformation => entityInformationView;
    public bool isShowHideAnimatorVisible => showHideAnimator.isVisible;

    [Header("Main Containers")]
    [SerializeField] internal GameObject firstPersonCanvasGO;
    [SerializeField] internal GameObject godModeCanvasGO;

    [Header("Animator")]
    [SerializeField] internal ShowHideAnimator showHideAnimator;

    [Header("UI Modules")]
    [SerializeField] internal TooltipView tooltipView;
    [SerializeField] internal QuickBarView quickBarView;
    [SerializeField] internal SceneCatalogView sceneCatalogView;
    [SerializeField] internal EntityInformationView entityInformationView;
    [SerializeField] internal FirstPersonModeView firstPersonModeView;
    [SerializeField] internal ShortcutsView shortcutsView;
    [SerializeField] internal PublishPopupView publishPopupView;
    [SerializeField] internal DragAndDropSceneObjectView dragAndDropSceneObjectView;
    [SerializeField] internal PublishBtnView publishBtnView;
    [SerializeField] internal InspectorBtnView inspectorBtnView;
    [SerializeField] internal CatalogBtnView catalogBtnView;
    [SerializeField] internal InspectorView inspectorView;
    [SerializeField] internal TopActionsButtonsView topActionsButtonsView;
    [SerializeField] internal BuildModeConfirmationModalView buildModeConfirmationModalView;

    private bool isDestroyed = false;
    internal BuildModeHUDInitializationModel controllers;

    private const string VIEW_PATH = "BuildModeHUD";

    internal static BuildModeHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<BuildModeHUDView>();
        view.gameObject.name = "_BuildModeHUD";

        return view;
    }

    public void Initialize(BuildModeHUDInitializationModel controllers)
    {
        this.controllers = controllers;
        this.controllers.tooltipController.Initialize(tooltipView);
        this.controllers.sceneCatalogController.Initialize(sceneCatalogView, this.controllers.quickBarController);
        this.controllers.quickBarController.Initialize(quickBarView, this.controllers.sceneCatalogController);
        this.controllers.entityInformationController.Initialize(entityInformationView);
        this.controllers.firstPersonModeController.Initialize(firstPersonModeView, this.controllers.tooltipController);
        this.controllers.shortcutsController.Initialize(shortcutsView);
        this.controllers.publishPopupController.Initialize(publishPopupView);
        this.controllers.dragAndDropSceneObjectController.Initialize(dragAndDropSceneObjectView);
        this.controllers.publishBtnController.Initialize(publishBtnView, this.controllers.tooltipController);
        this.controllers.inspectorBtnController.Initialize(inspectorBtnView, this.controllers.tooltipController);
        this.controllers.catalogBtnController.Initialize(catalogBtnView, this.controllers.tooltipController);
        this.controllers.inspectorController.Initialize(inspectorView);
        this.controllers.buildModeConfirmationModalController.Initialize(buildModeConfirmationModalView);
        this.controllers.topActionsButtonsController.Initialize(topActionsButtonsView, this.controllers.tooltipController, this.controllers.buildModeConfirmationModalController);
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        controllers.tooltipController.Dispose();
        controllers.quickBarController.Dispose();
        controllers.sceneCatalogController.Dispose();
        controllers.entityInformationController.Dispose();
        controllers.firstPersonModeController.Dispose();
        controllers.shortcutsController.Dispose();
        controllers.publishPopupController.Dispose();
        controllers.dragAndDropSceneObjectController.Dispose();
        controllers.publishBtnController.Dispose();
        controllers.inspectorBtnController.Dispose();
        controllers.catalogBtnController.Dispose();
        controllers.inspectorController.Dispose();
        controllers.buildModeConfirmationModalController.Dispose();
        controllers.topActionsButtonsController.Dispose();
    }

    public void SetPublishBtnAvailability(bool isAvailable) { controllers.publishBtnController.SetInteractable(isAvailable); }

    public void RefreshCatalogAssetPack() { controllers.sceneCatalogController.RefreshAssetPack(); }

    public void RefreshCatalogContent() { controllers.sceneCatalogController.RefreshCatalog(); }

    public void SetVisibilityOfCatalog(bool isVisible)
    {
        if (isVisible)
            controllers.sceneCatalogController.OpenCatalog();
        else
            controllers.sceneCatalogController.CloseCatalog();
    }

    public void SetVisibilityOfSceneInfo(bool isVisible)
    {
        if (isVisible)
            controllers.inspectorController.sceneLimitsController.Enable();
        else
            controllers.inspectorController.sceneLimitsController.Disable();
    }

    public void SetVisibilityOfControls(bool isVisible) { controllers.shortcutsController.SetActive(isVisible); }

    public void SetVisibilityOfExtraBtns(bool isVisible) { controllers.topActionsButtonsController.SetExtraActionsActive(isVisible); }

    public void SetFirstPersonView()
    {
        firstPersonCanvasGO.SetActive(true);
        godModeCanvasGO.SetActive(false);
        HideToolTip();
    }

    public void SetGodModeView()
    {
        firstPersonCanvasGO.SetActive(false);
        godModeCanvasGO.SetActive(true);
        HideToolTip();
    }

    public void HideToolTip() { controllers.tooltipController.HideTooltip(); }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void AnimatorShow(bool isVisible)
    {
        if (isVisible)
            showHideAnimator.Show();
        else
            showHideAnimator.Hide();
    }
}