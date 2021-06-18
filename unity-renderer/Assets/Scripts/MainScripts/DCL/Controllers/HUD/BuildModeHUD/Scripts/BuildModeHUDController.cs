using DCL.Configuration;
using DCL.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeHUDController : IHUD
{
    public event Action OnChangeModeAction;
    public event Action OnTranslateSelectedAction;
    public event Action OnRotateSelectedAction;
    public event Action OnScaleSelectedAction;
    public event Action OnResetAction;
    public event Action OnResetCameraAction;
    public event Action OnUndoAction;
    public event Action OnRedoAction;
    public event Action OnDuplicateSelectedAction;
    public event Action OnDeleteSelectedAction;
    public event Action OnEntityListVisible;
    public event Action OnStopInput;
    public event Action OnResumeInput;
    public event Action OnTutorialAction;
    public event Action OnPublishAction;
    public event Action<string, string, string> OnConfirmPublishAction;
    public event Action OnLogoutAction;
    public event Action OnChangeSnapModeAction;
    public event Action<CatalogItem> OnCatalogItemSelected;
    public event Action<CatalogItem> OnCatalogItemDropped;
    public event Action<DCLBuilderInWorldEntity> OnEntityClick;
    public event Action<DCLBuilderInWorldEntity> OnEntityDelete;
    public event Action<DCLBuilderInWorldEntity> OnEntityLock;
    public event Action<DCLBuilderInWorldEntity> OnEntityChangeVisibility;
    public event Action<DCLBuilderInWorldEntity, string> OnEntityRename;
    public event Action<DCLBuilderInWorldEntity> OnEntitySmartItemComponentUpdate;
    public event Action<Vector3> OnSelectedObjectPositionChange;
    public event Action<Vector3> OnSelectedObjectRotationChange;
    public event Action<Vector3> OnSelectedObjectScaleChange;
    public event Action OnCatalogOpen; // Note(Adrian): This is used right now for tutorial purposes

    internal IBuildModeHUDView view;

    internal bool areExtraButtonsVisible = false;
    internal bool isControlsVisible = false;
    internal bool isEntityListVisible = false;
    internal bool isSceneLimitInfoVisibile = false;
    internal bool isCatalogOpen = false;

    internal BuildModeHUDInitializationModel controllers;
    internal CatalogItemDropController catalogItemDropController;

    internal static readonly Vector3 catalogItemTooltipOffset = new Vector3 (0, 25, 0);

    private Coroutine publishProgressCoroutine = null;

    public void Initialize()
    {
        CreateBuildModeControllers();
        CreateMainView();
        ConfigureSceneCatalogController();
        ConfigureEntityInformationController();
        ConfigureFirstPersonModeController();
        ConfigureShortcutsController();
        ConfigureDragAndDropSceneObjectController();
        ConfigurePublishBtnController();
        ConfigureInspectorBtnController();
        ConfigureCatalogBtnController();
        ConfigureInspectorController();
        ConfigureTopActionsButtonsController();
        ConfigureCatalogItemDropController();
        ConfigureSaveHUDController();
        ConfigurePublicationDetailsController();
    }

    public void Initialize(BuildModeHUDInitializationModel controllers)
    {
        this.controllers = controllers;
        catalogItemDropController = new CatalogItemDropController();

        CreateMainView();
    }

    internal void CreateBuildModeControllers()
    {
        controllers = new BuildModeHUDInitializationModel
        {
            tooltipController = new TooltipController(),
            feedbackTooltipController = new TooltipController(),
            sceneCatalogController = new SceneCatalogController(),
            quickBarController = new QuickBarController(),
            entityInformationController = new EntityInformationController(),
            firstPersonModeController = new FirstPersonModeController(),
            shortcutsController = new ShortcutsController(),
            publishPopupController = new PublishPopupController(),
            dragAndDropSceneObjectController = new DragAndDropSceneObjectController(),
            publishBtnController = new PublishBtnController(),
            inspectorBtnController = new InspectorBtnController(),
            catalogBtnController = new CatalogBtnController(),
            inspectorController = new InspectorController(),
            buildModeConfirmationModalController = new BuildModeConfirmationModalController(),
            topActionsButtonsController = new TopActionsButtonsController(),
            saveHUDController = new SaveHUDController(),
            publicationDetailsController = new PublicationDetailsController()
        };

        catalogItemDropController = new CatalogItemDropController();
    }

    internal void CreateMainView()
    {
        view = CreateView();

        if (view.viewGO != null)
            view.viewGO.SetActive(false);

        view.Initialize(controllers);
    }

    private void ConfigureSceneCatalogController()
    {
        controllers.sceneCatalogController.OnHideCatalogClicked += ChangeVisibilityOfCatalog;
        controllers.sceneCatalogController.OnCatalogItemSelected += CatalogItemSelected;
        controllers.sceneCatalogController.OnStopInput += StopInput;

        controllers.sceneCatalogController.OnResumeInput += () => OnResumeInput?.Invoke();
        controllers.sceneCatalogController.OnPointerEnterInCatalogItemAdapter += ShowTooltipForCatalogItemAdapter;
        controllers.sceneCatalogController.OnPointerExitInCatalogItemAdapter += (x, y) => controllers.tooltipController.HideTooltip();
    }

    private void ConfigureEntityInformationController()
    {
        controllers.entityInformationController.OnPositionChange += (x) => OnSelectedObjectPositionChange?.Invoke(x);
        controllers.entityInformationController.OnRotationChange += (x) => OnSelectedObjectRotationChange?.Invoke(x);
        controllers.entityInformationController.OnScaleChange += (x) => OnSelectedObjectScaleChange?.Invoke(x);
        controllers.entityInformationController.OnNameChange += (entity, newName) => OnEntityRename?.Invoke(entity, newName);
        controllers.entityInformationController.OnSmartItemComponentUpdate += (entity) => OnEntitySmartItemComponentUpdate?.Invoke(entity);
        controllers.entityInformationController.OnDisable += OnEntityInformationDisable;
    }

    private void ConfigureFirstPersonModeController() { controllers.firstPersonModeController.OnClick += () => OnChangeModeAction?.Invoke(); }

    private void ConfigureShortcutsController() { controllers.shortcutsController.OnCloseClick += ChangeVisibilityOfControls; }

    private void ConfigureDragAndDropSceneObjectController() { controllers.dragAndDropSceneObjectController.OnDrop += () => SceneObjectDroppedInView(); }

    private void ConfigurePublishBtnController() { controllers.publishBtnController.OnClick += () => OnPublishAction?.Invoke(); }

    private void ConfigureInspectorBtnController() { controllers.inspectorBtnController.OnClick += () => ChangeVisibilityOfEntityList(); }

    private void ConfigureCatalogBtnController() { controllers.catalogBtnController.OnClick += ChangeVisibilityOfCatalog; }

    private void ConfigureInspectorController()
    {
        controllers.inspectorController.OnEntityClick += (x) => OnEntityClick(x);
        controllers.inspectorController.OnEntityDelete += (x) => OnEntityDelete(x);
        controllers.inspectorController.OnEntityLock += (x) => OnEntityLock(x);
        controllers.inspectorController.OnEntityChangeVisibility += (x) => OnEntityChangeVisibility(x);
        controllers.inspectorController.OnEntityRename += (entity, newName) => OnEntityRename(entity, newName);
        controllers.inspectorController.SetCloseButtonsAction(ChangeVisibilityOfEntityList);
    }

    private void ConfigureTopActionsButtonsController()
    {
        controllers.topActionsButtonsController.OnChangeModeClick += () => OnChangeModeAction?.Invoke();
        controllers.topActionsButtonsController.OnExtraClick += ChangeVisibilityOfExtraBtns;
        controllers.topActionsButtonsController.OnTranslateClick += () => OnTranslateSelectedAction?.Invoke();
        controllers.topActionsButtonsController.OnRotateClick += () => OnRotateSelectedAction?.Invoke();
        controllers.topActionsButtonsController.OnScaleClick += () => OnScaleSelectedAction?.Invoke();
        controllers.topActionsButtonsController.OnUndoClick += () => OnUndoAction?.Invoke();
        controllers.topActionsButtonsController.OnRedoClick += () => OnRedoAction?.Invoke();
        controllers.topActionsButtonsController.OnDuplicateClick += () => OnDuplicateSelectedAction?.Invoke();
        controllers.topActionsButtonsController.OnDeleteClick += () => OnDeleteSelectedAction?.Invoke();
        controllers.topActionsButtonsController.OnSnapModeClick += () => OnChangeSnapModeAction?.Invoke();
        controllers.topActionsButtonsController.OnLogOutClick += ExitStart;
        controllers.topActionsButtonsController.extraActionsController.OnControlsClick += ChangeVisibilityOfControls;
        controllers.topActionsButtonsController.extraActionsController.OnHideUIClick += ChangeVisibilityOfUI;
        controllers.topActionsButtonsController.extraActionsController.OnResetClick += () => OnResetAction?.Invoke();
        controllers.topActionsButtonsController.extraActionsController.OnTutorialClick += () => OnTutorialAction?.Invoke();
        controllers.topActionsButtonsController.extraActionsController.OnResetCameraClick += () => OnResetCameraAction?.Invoke();

        controllers.topActionsButtonsController.extraActionsController.SetResetButtonInteractable(false);
    }

    private void ConfigureCatalogItemDropController()
    {
        catalogItemDropController.catalogGroupListView = view.sceneCatalog.catalogGroupListView;
        catalogItemDropController.OnCatalogItemDropped += CatalogItemDropped;
    }

    private void ConfigureSaveHUDController() { OnLogoutAction += controllers.saveHUDController.StopAnimation; }

    private void ConfigurePublicationDetailsController()
    {
        controllers.publicationDetailsController.OnCancel += CancelPublicationDetails;
        controllers.publicationDetailsController.OnPublish += ConfirmPublicationDetails;
    }

    public void SceneSaved() { controllers.saveHUDController.SceneStateSave(); }

    public void SetBuilderProjectInfo(string projectName, string projectDescription)
    {
        if (!string.IsNullOrEmpty(projectName))
            controllers.publicationDetailsController.SetCustomPublicationInfo(projectName, projectDescription);
        else
            controllers.publicationDetailsController.SetDefaultPublicationInfo();
    }

    public void SetBuilderProjectScreenshot(Texture2D screenshot) { controllers.publicationDetailsController.SetPublicationScreenshot(screenshot); }

    public void PublishStart() { controllers.publicationDetailsController.SetActive(true); }

    internal void ConfirmPublicationDetails(string sceneName, string sceneDescription)
    {
        UnsubscribeConfirmationModal();

        controllers.buildModeConfirmationModalController.OnCancelExit += CancelPublishModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit += ConfirmPublishModal;

        controllers.buildModeConfirmationModalController.Configure(
            BuilderInWorldSettings.PUBLISH_MODAL_TITLE,
            BuilderInWorldSettings.PUBLISH_MODAL_SUBTITLE,
            BuilderInWorldSettings.PUBLISH_MODAL_CANCEL_BUTTON,
            BuilderInWorldSettings.PUBLISH_MODAL_CONFIRM_BUTTON);
        controllers.buildModeConfirmationModalController.SetActive(true, BuildModeModalType.PUBLISH);
        controllers.publicationDetailsController.SetActive(false);
    }

    internal void CancelPublicationDetails() { controllers.publicationDetailsController.SetActive(false); }

    internal void CancelPublishModal(BuildModeModalType modalType)
    {
        if (modalType != BuildModeModalType.PUBLISH)
            return;

        controllers.buildModeConfirmationModalController.SetActive(false, BuildModeModalType.PUBLISH);
        controllers.publicationDetailsController.SetActive(true);

        controllers.buildModeConfirmationModalController.OnCancelExit -= CancelPublishModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit -= ConfirmPublishModal;
    }

    internal void ConfirmPublishModal(BuildModeModalType modalType)
    {
        if (modalType != BuildModeModalType.PUBLISH)
            return;

        controllers.publishPopupController.PublishStart();

        Texture2D sceneScreenshotTexture = controllers.publicationDetailsController.GetSceneScreenshotTexture();

        OnConfirmPublishAction?.Invoke(
            controllers.publicationDetailsController.GetSceneName(),
            controllers.publicationDetailsController.GetSceneDescription(),
            sceneScreenshotTexture != null ? Convert.ToBase64String(sceneScreenshotTexture.EncodeToJPG(90)) : "");

        // NOTE (Santi): This is temporal until we implement the way of return the publish progress from the kernel side.
        //               Meanwhile we will display a fake progress.
        publishProgressCoroutine = CoroutineStarter.Start(FakePublishProgress());

        controllers.buildModeConfirmationModalController.OnCancelExit -= CancelPublishModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit -= ConfirmPublishModal;
    }

    private IEnumerator FakePublishProgress()
    {
        while (true)
        {
            float newPercentage = Mathf.Clamp(
                controllers.publishPopupController.currentProgress + UnityEngine.Random.Range(10f, 30f),
                controllers.publishPopupController.currentProgress,
                99f);

            controllers.publishPopupController.SetPercentage(newPercentage);

            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
        }
    }

    public void ExitStart()
    {
        UnsubscribeConfirmationModal();

        controllers.buildModeConfirmationModalController.OnCancelExit += CancelExitModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit += ConfirmExitModal;

        controllers.buildModeConfirmationModalController.Configure(
            BuilderInWorldSettings.EXIT_MODAL_TITLE,
            BuilderInWorldSettings.EXIT_MODAL_SUBTITLE,
            BuilderInWorldSettings.EXIT_MODAL_CANCEL_BUTTON,
            BuilderInWorldSettings.EXIT_MODAL_CONFIRM_BUTTON);
        controllers.buildModeConfirmationModalController.SetActive(true, BuildModeModalType.EXIT);
    }

    internal void CancelExitModal(BuildModeModalType modalType)
    {
        if (modalType != BuildModeModalType.EXIT)
            return;

        controllers.buildModeConfirmationModalController.SetActive(false, BuildModeModalType.EXIT);

        controllers.buildModeConfirmationModalController.OnCancelExit -= CancelExitModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit -= ConfirmExitModal;
    }

    internal void ConfirmExitModal(BuildModeModalType modalType)
    {
        if (modalType != BuildModeModalType.EXIT)
            return;

        OnLogoutAction?.Invoke();

        controllers.buildModeConfirmationModalController.OnCancelExit -= CancelExitModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit -= ConfirmExitModal;

        controllers.publicationDetailsController.SetDefaultPublicationInfo();
    }

    private void UnsubscribeConfirmationModal()
    {
        controllers.buildModeConfirmationModalController.OnCancelExit -= CancelPublishModal;
        controllers.buildModeConfirmationModalController.OnCancelExit -= CancelExitModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit -= ConfirmPublishModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit -= ConfirmExitModal;
    }

    public void PublishEnd(bool isOk, string message)
    {
        if (publishProgressCoroutine != null)
        {
            CoroutineStarter.Stop(publishProgressCoroutine);
            publishProgressCoroutine = null;
        }

        controllers.publishPopupController.PublishEnd(isOk, message);
    }

    public void SetGizmosActive(string gizmos) { controllers.topActionsButtonsController.SetGizmosActive(gizmos); }
    public void SetParcelScene(ParcelScene parcelScene) { controllers.inspectorController.sceneLimitsController.SetParcelScene(parcelScene); }

    public void SetPublishBtnAvailability(bool isAvailable, string feedbackMessage = "") { view.SetPublishBtnAvailability(isAvailable, feedbackMessage); }

    #region Catalog

    private void ShowTooltipForCatalogItemAdapter(PointerEventData data, CatalogItemAdapter adapter)
    {
        controllers.tooltipController.SetTooltipText(adapter.GetContent().name);
        controllers.tooltipController.ShowTooltip(data, catalogItemTooltipOffset);
    }

    public void RefreshCatalogAssetPack() { view.RefreshCatalogAssetPack(); }

    public void RefreshCatalogContent() { view.RefreshCatalogContent(); }

    public void StopInput()
    {
        OnStopInput?.Invoke();
        ToggleCatalogIfExpanded();
    }

    public void CatalogItemSelected(CatalogItem catalogItem)
    {
        OnCatalogItemSelected?.Invoke(catalogItem);
        ToggleCatalogIfExpanded();
    }

    public void CatalogItemDropped(CatalogItem catalogItem)
    {
        OnCatalogItemDropped?.Invoke(catalogItem);
        ToggleCatalogIfExpanded();
    }

    private void ToggleCatalogIfExpanded()
    {
        if (controllers.sceneCatalogController.IsCatalogExpanded())
            controllers.sceneCatalogController.ToggleCatalogExpanse();
    }

    public void SetVisibilityOfCatalog(bool isVisible)
    {
        isCatalogOpen = isVisible;
        view.SetVisibilityOfCatalog(isCatalogOpen);

        if (isVisible)
            OnCatalogOpen?.Invoke();
    }

    public void ChangeVisibilityOfCatalog()
    {
        isCatalogOpen = !controllers.sceneCatalogController.IsCatalogOpen();
        SetVisibilityOfCatalog(isCatalogOpen);
    }

    #endregion

    #region SceneLimitInfo

    public void ShowSceneLimitsPassed()
    {
        if (!isSceneLimitInfoVisibile)
            ChangeVisibilityOfSceneInfo();
    }

    public void UpdateSceneLimitInfo() { controllers.inspectorController.sceneLimitsController.UpdateInfo(); }

    public void ChangeVisibilityOfSceneInfo(bool shouldBeVisibile)
    {
        isSceneLimitInfoVisibile = shouldBeVisibile;
        view.SetVisibilityOfSceneInfo(isSceneLimitInfoVisibile);
    }

    public void ChangeVisibilityOfSceneInfo()
    {
        isSceneLimitInfoVisibile = !isSceneLimitInfoVisibile;
        view.SetVisibilityOfSceneInfo(isSceneLimitInfoVisibile);
    }

    #endregion

    public void ActivateFirstPersonModeUI()
    {
        if (view != null)
            view.SetFirstPersonView();
    }

    public void ActivateGodModeUI()
    {
        if (view != null)
            view.SetGodModeView();
    }

    #region EntityInformation

    public void EntityInformationSetEntity(DCLBuilderInWorldEntity entity, ParcelScene scene) { controllers.entityInformationController.SetEntity(entity, scene); }

    public void ShowEntityInformation(bool activateTransparencyMode = false)
    {
        controllers.entityInformationController.Enable();
        controllers.entityInformationController.SetTransparencyMode(activateTransparencyMode);
        controllers.sceneCatalogController.CloseCatalog();
        controllers.tooltipController.HideTooltip();

        if (activateTransparencyMode)
            controllers.catalogBtnController.SetActive(false);
    }

    public void HideEntityInformation()
    {
        controllers.entityInformationController.Disable();
        controllers.catalogBtnController.SetActive(true);

        if (isCatalogOpen)
            controllers.sceneCatalogController.SetActive(true);
    }

    private void OnEntityInformationDisable()
    {
        if (isCatalogOpen)
            controllers.sceneCatalogController.SetActive(true);
    }

    #endregion

    public void SetEntityList(List<DCLBuilderInWorldEntity> entityList)
    {
        controllers.inspectorController.SetEntityList(entityList);

        if (view.entityInformation != null)
            view.entityInformation.smartItemListView.SetEntityList(entityList);
    }

    public void ChangeVisibilityOfEntityList()
    {
        isEntityListVisible = !isEntityListVisible;
        controllers.saveHUDController?.SetSaveViewByEntityListOpen(isEntityListVisible);
        if (isEntityListVisible)
        {
            OnEntityListVisible?.Invoke();
            controllers.inspectorController.OpenEntityList();
        }
        else
        {
            controllers.inspectorController.CloseList();
        }
    }

    public void ClearEntityList() { controllers.inspectorController.ClearList(); }

    public void ChangeVisibilityOfControls()
    {
        isControlsVisible = !isControlsVisible;
        view.SetVisibilityOfControls(isControlsVisible);
    }

    public void ChangeVisibilityOfUI() { SetVisibility(!IsVisible()); }

    #region TopButtons

    public void ChangeVisibilityOfExtraBtns()
    {
        areExtraButtonsVisible = !areExtraButtonsVisible;
        view.SetVisibilityOfExtraBtns(areExtraButtonsVisible);
    }

    public void HideExtraBtns()
    {
        areExtraButtonsVisible = false;
        controllers.topActionsButtonsController.extraActionsController.SetActive(areExtraButtonsVisible);
    }

    public void SetActionsButtonsInteractable(bool isInteractable) { controllers.topActionsButtonsController.SetActionsInteractable(isInteractable); }

    public void SetSnapModeActive(bool isActive) { controllers.topActionsButtonsController.SetSnapActive(isActive); }

    public void SetUndoButtonInteractable(bool isInteractable) { controllers.topActionsButtonsController.SetUndoInteractable(isInteractable); }
    public void SetRedoButtonInteractable(bool isInteractable) { controllers.topActionsButtonsController.SetRedoInteractable(isInteractable); }

    #endregion

    public void SetVisibility(bool visible)
    {
        if (view == null)
            return;

        if (IsVisible() && !visible)
        {
            view.AnimatorShow(false);
            AudioScriptableObjects.fadeOut.Play(true);
        }
        else if (!IsVisible() && visible)
        {
            view.SetActive(true);
            view.AnimatorShow(true);
            AudioScriptableObjects.fadeIn.Play(true);
        }
    }

    public void Dispose()
    {
        if (view == null)
            return;
        else if (view.viewGO != null)
            UnityEngine.Object.Destroy(view.viewGO);
    }

    public void ToggleVisibility() { SetVisibility(!IsVisible()); }

    public bool IsVisible()
    {
        if (view == null)
            return false;

        return view.isShowHideAnimatorVisible;
    }

    public void SceneObjectDroppedInView() { catalogItemDropController.CatalogitemDropped(); }

    internal virtual IBuildModeHUDView CreateView() => BuildModeHUDView.Create();

    public void UpdateEntitiesSelection(int numberOfSelectedEntities)
    {
        controllers.entityInformationController.UpdateEntitiesSelection(numberOfSelectedEntities);
        controllers.topActionsButtonsController.extraActionsController.SetResetButtonInteractable(numberOfSelectedEntities > 0);
    }

    #region Inspector

    public void SetVisibilityOfInspector(bool isVisible) { view.SetVisibilityOfInspector(isVisible); }

    #endregion

}