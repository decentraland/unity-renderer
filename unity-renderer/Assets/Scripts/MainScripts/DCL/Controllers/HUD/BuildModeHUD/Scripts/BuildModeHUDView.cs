using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public interface IBuildModeHUDView
{
    GameObject viewGO { get; }
    SaveHUDView saveHud { get; }
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
    void SetPublishBtnAvailability(bool isAvailable, string feedbackMessage = "");
    void SetVisibilityOfCatalog(bool isVisible);
    void SetVisibilityOfControls(bool isVisible);
    void SetVisibilityOfExtraBtns(bool isVisible);
    void SetVisibilityOfSceneInfo(bool isVisible);
    void SetVisibilityOfInspector(bool isVisible);
}

public class BuildModeHUDView : MonoBehaviour, IBuildModeHUDView
{
    public GameObject viewGO => !isDestroyed ? gameObject : null;
    public SaveHUDView saveHud  => saveView;
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
    [SerializeField] internal TooltipView feedbackTtooltipView;
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
    [SerializeField] internal SaveHUDView saveView;
    [SerializeField] internal PublicationDetailsView newProjectDetailsView;
    [SerializeField] internal PublicationDetailsView publicationDetailsView;

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
        this.controllers.feedbackTooltipController.Initialize(feedbackTtooltipView);
        this.controllers.sceneCatalogController.Initialize(sceneCatalogView, this.controllers.quickBarController);
        this.controllers.quickBarController.Initialize(quickBarView, this.controllers.dragAndDropSceneObjectController);
        this.controllers.entityInformationController.Initialize(entityInformationView);
        this.controllers.firstPersonModeController.Initialize(firstPersonModeView, this.controllers.tooltipController);
        this.controllers.shortcutsController.Initialize(shortcutsView);
        this.controllers.publishPopupController.Initialize(publishPopupView);
        this.controllers.dragAndDropSceneObjectController.Initialize(this.controllers.sceneCatalogController, dragAndDropSceneObjectView);
        this.controllers.publishBtnController.Initialize(publishBtnView, this.controllers.tooltipController, this.controllers.feedbackTooltipController);
        this.controllers.inspectorBtnController.Initialize(inspectorBtnView, this.controllers.tooltipController);
        this.controllers.catalogBtnController.Initialize(catalogBtnView, this.controllers.tooltipController);
        this.controllers.inspectorController.Initialize(inspectorView);
        this.controllers.buildModeConfirmationModalController.Initialize(buildModeConfirmationModalView);
        this.controllers.topActionsButtonsController.Initialize(topActionsButtonsView, this.controllers.tooltipController);
        this.controllers.saveHUDController.Initialize(saveView);
        this.controllers.newProjectDetailsController.Initialize(newProjectDetailsView);
        this.controllers.publicationDetailsController.Initialize(publicationDetailsView);
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
        controllers.saveHUDController.Dispose();
        controllers.newProjectDetailsController.Dispose();
        controllers.publicationDetailsController.Dispose();
    }

    public void SetPublishBtnAvailability(bool isAvailable, string feedbackMessage = "")
    {
        controllers.publishBtnController.SetInteractable(isAvailable);
        controllers.publishBtnController.SetFeedbackMessage(feedbackMessage);
    }

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
        saveView.SetFirstPersonView();
        HideToolTip();
    }

    public void SetGodModeView()
    {
        firstPersonCanvasGO.SetActive(false);
        godModeCanvasGO.SetActive(true);
        saveView.SetGodModeView();
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

    public void SetVisibilityOfInspector(bool isVisible)
    {
        if (isVisible)
            controllers.inspectorController.OpenEntityList();
        else
            controllers.inspectorController.CloseList();
    }
}