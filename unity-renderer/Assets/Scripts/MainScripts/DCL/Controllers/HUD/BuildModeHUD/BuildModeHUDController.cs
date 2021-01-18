using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;
using System;
using DCL.Controllers;

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

    public event Action<SceneObject> OnSceneObjectSelected;

    public event Action<DCLBuilderInWorldEntity> OnEntityClick;
    public event Action<DCLBuilderInWorldEntity> OnEntityDelete;
    public event Action<DCLBuilderInWorldEntity> OnEntityLock;
    public event Action<DCLBuilderInWorldEntity> OnEntityChangeVisibility;
    public event Action<DCLBuilderInWorldEntity> OnEntityRename;

    
    //Note(Adrian): This is used right now for tutorial purposes
    public event Action OnCatalogOpen;

    internal BuildModeHUDView view;
    BuilderInWorldEntityListController buildModeEntityListController;
    SceneObjectDropController sceneObjectDropController;

    bool areExtraButtonsVisible = false,isControlsVisible = false, isEntityListVisible = false, isSceneLimitInfoVisibile = false,isCatalogOpen = false;

    public BuildModeHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("BuildModeHUD")).GetComponent<BuildModeHUDView>();

        view.name = "_BuildModeHUD";
        view.gameObject.SetActive(false);

        sceneObjectDropController = new SceneObjectDropController();

        buildModeEntityListController = view.GetComponentInChildren<BuilderInWorldEntityListController>();
        sceneObjectDropController.catalogGroupListView = view.catalogGroupListView;



        buildModeEntityListController.OnEntityClick += (x) => OnEntityClick(x);
        buildModeEntityListController.OnEntityDelete += (x) => OnEntityDelete(x);
        buildModeEntityListController.OnEntityLock += (x) => OnEntityLock(x);
        buildModeEntityListController.OnEntityChangeVisibility += (x) => OnEntityChangeVisibility(x);
        buildModeEntityListController.OnEntityRename += (x) => OnEntityRename(x);

        buildModeEntityListController.CloseList();

        view.OnSceneObjectDrop += () => sceneObjectDropController.SceneObjectDropped();
        view.OnChangeModeAction += () => OnChangeModeAction?.Invoke();
        view.OnExtraBtnsClick += ChangeVisibilityOfExtraBtns;
        view.OnControlsVisibilityAction += ChangeVisibilityOfControls;
        view.OnChangeUIVisbilityAction += ChangeVisibilityOfUI;
        view.OnSceneLimitInfoChangeVisibility += ChangeVisibilityOfSceneInfo;
        view.OnSceneLimitInfoControllerChangeVisibilityAction += ChangeVisibilityOfSceneInfo;
        view.OnSceneCatalogControllerChangeVisibilityAction += ChangeVisibilityOfCatalog;


        view.OnTranslateSelectionAction += () => OnTranslateSelectedAction?.Invoke();
        view.OnRotateSelectionAction += () => OnRotateSelectedAction?.Invoke();
        view.OnScaleSelectionAction += () => OnScaleSelectedAction?.Invoke();
        view.OnResetSelectedAction += () => OnResetAction?.Invoke();
        view.OnDuplicateSelectionAction += () => OnDuplicateSelectedAction?.Invoke();
        view.OnDeleteSelectionAction += () => OnDeleteSelectedAction?.Invoke();

        sceneObjectDropController.OnSceneObjectDropped += SceneObjectSelected;
        view.OnSceneObjectSelected += SceneObjectSelected;
        view.OnStopInput += () => OnStopInput?.Invoke();
        view.OnResumeInput += () => OnResumeInput?.Invoke();


        view.OnEntityListChangeVisibilityAction += () => ChangeVisibilityOfEntityList();

        view.OnTutorialAction += () => OnTutorialAction?.Invoke();
        view.OnPublishAction += () => OnPublishAction?.Invoke();
        view.OnLogoutAction += () => OnLogoutAction?.Invoke();
    }

    public void PublishStart()
    {
        view.PublishStart();
    }

    public void PublishEnd(string message)
    {
        view.PublishEnd(message);
    }

    public void SetParcelScene(ParcelScene parcelScene)
    {
        view.sceneLimitInfoController.SetParcelScene(parcelScene);
    }

    public void SetPublishBtnAvailability(bool isAvailable)
    {
        view.SetPublishBtnAvailability(isAvailable);
    }

    #region Catalog

    public void RefreshCatalogAssetPack()
    {
        view.RefreshCatalogAssetPack();
    }

    public void RefreshCatalogContent()
    {
        view.RefreshCatalogContent();
    }

    void SceneObjectSelected(SceneObject sceneObject)
    {
        OnSceneObjectSelected?.Invoke(sceneObject);
        SetVisibilityOfCatalog(false);
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
        isCatalogOpen = !view.sceneObjectCatalogController.IsCatalogOpen();
        SetVisibilityOfCatalog(isCatalogOpen);
    }

    #endregion

    #region SceneLimitInfo

    public void ShowSceneLimitsPassed()
    {
        if (!isSceneLimitInfoVisibile)
            ChangeVisibilityOfSceneInfo();
    }

    public void UpdateSceneLimitInfo()
    {
        view.sceneLimitInfoController.UpdateInfo();
    }

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
        if(view != null)
            view.SetGodModeView();
    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> entityList)
    {
        buildModeEntityListController.SetEntityList(entityList);
    }

    public void ChangeVisibilityOfEntityList()
    {
        isEntityListVisible = !isEntityListVisible;
        if (isEntityListVisible)
        {
            OnEntityListVisible?.Invoke();
            buildModeEntityListController.OpenEntityList();
        }
        else
        {
            buildModeEntityListController.CloseList();
        }
    }

    public void ClearEntityList()
    {
        buildModeEntityListController.ClearList();
    }

    public void ChangeVisibilityOfControls()
    {
        isControlsVisible = !isControlsVisible;
        view.SetVisibilityOfControls(isControlsVisible);
    }

    public void ChangeVisibilityOfUI()
    {
        SetVisibility(!IsVisible());
    }

    public void ChangeVisibilityOfExtraBtns()
    {
        areExtraButtonsVisible = !areExtraButtonsVisible;
        view.SetVisibilityOfExtraBtns(areExtraButtonsVisible);
    }

    public void SetVisibility(bool visible)
    {
        if (!view)
            return;

        if (IsVisible() && !visible)
        {

            view.showHideAnimator.Hide();

            AudioScriptableObjects.fadeOut.Play(true);
        }
        else if (!IsVisible() && visible)
        {
            view.gameObject.SetActive(true);
            view.showHideAnimator.Show();
            AudioScriptableObjects.fadeIn.Play(true);
        }
    }

    public void Dispose()
    {
        if (view)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    public void ToggleVisibility()
    {
        SetVisibility(!IsVisible());
    }

    public bool IsVisible()
    {
        if (!view)
            return false;

        return view.showHideAnimator.isVisible;
    }
}
