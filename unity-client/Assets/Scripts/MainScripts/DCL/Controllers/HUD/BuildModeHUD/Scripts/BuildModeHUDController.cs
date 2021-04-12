using DCL.Controllers;
using System;
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
    public event Action OnDuplicateSelectedAction;
    public event Action OnDeleteSelectedAction;
    public event Action OnEntityListVisible;
    public event Action OnStopInput;
    public event Action OnResumeInput;
    public event Action OnTutorialAction;
    public event Action OnPublishAction;
    public event Action OnLogoutAction;
    public event Action<CatalogItem> OnCatalogItemSelected;
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
            topActionsButtonsController = new TopActionsButtonsController()
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
        controllers.topActionsButtonsController.OnResetClick += () => OnResetAction?.Invoke();
        controllers.topActionsButtonsController.OnDuplicateClick += () => OnDuplicateSelectedAction?.Invoke();
        controllers.topActionsButtonsController.OnDeleteClick += () => OnDeleteSelectedAction?.Invoke();
        controllers.topActionsButtonsController.OnLogOutClick += () => OnLogoutAction?.Invoke();
        controllers.topActionsButtonsController.extraActionsController.OnControlsClick += ChangeVisibilityOfControls;
        controllers.topActionsButtonsController.extraActionsController.OnHideUIClick += ChangeVisibilityOfUI;
        controllers.topActionsButtonsController.extraActionsController.OnTutorialClick += () => OnTutorialAction?.Invoke();
    }

    private void ConfigureCatalogItemDropController()
    {
        catalogItemDropController.catalogGroupListView = view.sceneCatalog.catalogGroupListView;
        catalogItemDropController.OnCatalogItemDropped += CatalogItemSelected;
    }

    public void PublishStart() { view.PublishStart(); }

    public void PublishEnd(string message) { view.PublishEnd(message); }

    public void SetParcelScene(ParcelScene parcelScene) { controllers.inspectorController.sceneLimitsController.SetParcelScene(parcelScene); }

    public void SetPublishBtnAvailability(bool isAvailable) { view.SetPublishBtnAvailability(isAvailable); }

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

        if (controllers.sceneCatalogController.IsCatalogExpanded())
            controllers.sceneCatalogController.ToggleCatalogExpanse();
    }

    public void CatalogItemSelected(CatalogItem catalogItem)
    {
        OnCatalogItemSelected?.Invoke(catalogItem);

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

    public void ShowEntityInformation() { controllers.entityInformationController.Enable(); }

    public void HideEntityInformation() { controllers.entityInformationController.Disable(); }

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

    public void ChangeVisibilityOfExtraBtns()
    {
        areExtraButtonsVisible = !areExtraButtonsVisible;
        view.SetVisibilityOfExtraBtns(areExtraButtonsVisible);
    }

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
}