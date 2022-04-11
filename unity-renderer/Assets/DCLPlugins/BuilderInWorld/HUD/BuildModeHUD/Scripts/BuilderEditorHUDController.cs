using DCL.Configuration;
using DCL.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DCL;
using DCL.Builder;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderEditorHUDController : IHUD, IBuilderEditorHUDController
{
    public event Action<string, string> OnProjectNameAndDescriptionChanged;
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
    public event Action OnStartExitAction;
    public event Action OnLogoutAction;
    public event Action OnChangeSnapModeAction;
    public event Action<CatalogItem> OnCatalogItemSelected;
    public event Action<CatalogItem> OnCatalogItemDropped;
    public event Action<BIWEntity> OnEntityClick;
    public event Action<BIWEntity> OnEntityDelete;
    public event Action<BIWEntity> OnEntityLock;
    public event Action<BIWEntity> OnEntityChangeVisibility;
    public event Action<BIWEntity, string> OnEntityRename;
    public event Action<BIWEntity> OnEntitySmartItemComponentUpdate;
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

    private float timeFromLastClickOnExtraButtons = 0f;
    internal IContext context;

    public void Initialize(IContext context)
    {
        this.context = context;
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
        ConfigureSaveHUDController();
        ConfigureQuickBarController();
        ConfigureNewProjectController();
    }

    public void Initialize(BuildModeHUDInitializationModel controllers, IContext context)
    {
        this.context = context;
        this.controllers = controllers;

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
            dragAndDropSceneObjectController = new DragAndDropSceneObjectController(),
            publishBtnController = new PublishBtnController(),
            inspectorBtnController = new InspectorBtnController(),
            catalogBtnController = new CatalogBtnController(),
            inspectorController = new InspectorController(),
            buildModeConfirmationModalController = new BuildModeConfirmationModalController(),
            topActionsButtonsController = new TopActionsButtonsController(),
            saveHUDController = new SaveHUDController(),
            newProjectDetailsController = new NewLandProjectDetailController(),
        };
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

    private void ConfigureDragAndDropSceneObjectController()
    {
        controllers.dragAndDropSceneObjectController.OnStopInput += StopInput;
        controllers.dragAndDropSceneObjectController.OnResumeInput += () => OnResumeInput?.Invoke();
        controllers.dragAndDropSceneObjectController.OnCatalogItemDropped += CatalogItemDropped;
    }

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

    private void ConfigureSaveHUDController() { OnLogoutAction += controllers.saveHUDController.StopAnimation; }

    private void ConfigureQuickBarController() { controllers.quickBarController.OnCatalogItemAssigned += QuickBarCatalogItemAssigned; }
    private void ConfigureNewProjectController() { controllers.newProjectDetailsController.OnNameAndDescriptionSet += NameAndDescriptionOfNewProjectSet; }
    private void QuickBarCatalogItemAssigned(CatalogItem item) { BIWAnalytics.QuickAccessAssigned(item, GetCatalogSectionSelected().ToString()); }

    private void NameAndDescriptionOfNewProjectSet(string name, string description) => OnProjectNameAndDescriptionChanged?.Invoke(name, description); 

    public void SceneSaved() { controllers.saveHUDController.SceneStateSave(); }

    public void NewSceneForLand(IBuilderScene sceneWithSceenshot) { controllers.newProjectDetailsController.SetActive(true); }

    internal void CancelNewProjectDetails() { controllers.newProjectDetailsController.SetActive(false); }

    public void ConfigureConfirmationModal(string title, string subTitle, string cancelButtonText, string confirmButtonText)
    {
        controllers.buildModeConfirmationModalController.Configure(
            title,
            subTitle,
            cancelButtonText,
            confirmButtonText);
    }

    public void UnsuscribeFromEvents()
    {
        UnsubscribeConfirmationModal();

        controllers.newProjectDetailsController.OnNameAndDescriptionSet -= NameAndDescriptionOfNewProjectSet;
        controllers.quickBarController.OnCatalogItemAssigned -= QuickBarCatalogItemAssigned;
        OnLogoutAction -= controllers.saveHUDController.StopAnimation;
    }

    public void ExitStart()
    {
        if (controllers.buildModeConfirmationModalController.IsViewActive())
            return;
        UnsubscribeConfirmationModal();

        controllers.buildModeConfirmationModalController.OnCancelExit += CancelExitModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit += ConfirmExitModal;

        controllers.buildModeConfirmationModalController.SetActive(true, BuildModeModalType.EXIT);

        OnStartExitAction?.Invoke();
    }

    internal void CancelExitModal(BuildModeModalType modalType)
    {
        if (modalType != BuildModeModalType.EXIT)
            return;

        controllers.buildModeConfirmationModalController.SetActive(false, BuildModeModalType.EXIT);

        UnsubscribeConfirmationModal();
    }

    internal void ConfirmExitModal(BuildModeModalType modalType)
    {
        if (modalType != BuildModeModalType.EXIT)
            return;

        OnLogoutAction?.Invoke();

        UnsubscribeConfirmationModal();
    }

    private void UnsubscribeConfirmationModal()
    {
        controllers.buildModeConfirmationModalController.OnCancelExit -= CancelExitModal;
        controllers.buildModeConfirmationModalController.OnConfirmExit -= ConfirmExitModal;
    }

    public void SetGizmosActive(string gizmos) { controllers.topActionsButtonsController.SetGizmosActive(gizmos); }
    public void SetParcelScene(IParcelScene IParcelScene) { controllers.inspectorController.sceneLimitsController.SetParcelScene(IParcelScene); }

    public void SetPublishBtnAvailability(bool isAvailable, string feedbackMessage = "") { view.SetPublishBtnAvailability(isAvailable, feedbackMessage); }

    #region Catalog

    public BuildModeCatalogSection GetCatalogSectionSelected() { return controllers.sceneCatalogController.GetCurrentSection(); }

    private void ShowTooltipForCatalogItemAdapter(PointerEventData data, CatalogItemAdapter adapter)
    {
        controllers.tooltipController.SetTooltipText(adapter.GetContent().name);
        controllers.tooltipController.ShowTooltip(data);
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

    public void EntityInformationSetEntity(BIWEntity entity, IParcelScene scene) { controllers.entityInformationController.SetEntity(entity, scene); }

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

    public void SetEntityList(List<BIWEntity> entityList)
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

    public void ChangeVisibilityOfUI()
    {
        DataStore.i.builderInWorld.showTaskBar.Set(!IsVisible());
        SetVisibility(!IsVisible());
    }

    #region TopButtons

    public void ChangeVisibilityOfExtraBtns()
    {
        areExtraButtonsVisible = !areExtraButtonsVisible;
        view.SetVisibilityOfExtraBtns(areExtraButtonsVisible);
        timeFromLastClickOnExtraButtons = Time.realtimeSinceStartup;
    }

    public void HideExtraBtns()
    {
        if ((Time.realtimeSinceStartup - timeFromLastClickOnExtraButtons) <= 0.1f)
            return;

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
        UnsuscribeFromEvents();
        controllers.Dispose();

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

    internal virtual IBuildModeHUDView CreateView() => BuildModeHUDView.Create();

    public void UpdateEntitiesSelection(int numberOfSelectedEntities)
    {
        controllers.entityInformationController.UpdateEntitiesSelection(numberOfSelectedEntities);
        controllers.topActionsButtonsController.extraActionsController.SetResetButtonInteractable(numberOfSelectedEntities > 0);
    }

    #region Inspector

    public void SetVisibilityOfInspector(bool isVisible)
    {
        isEntityListVisible = isVisible;
        view.SetVisibilityOfInspector(isVisible);
    }

    #endregion

}