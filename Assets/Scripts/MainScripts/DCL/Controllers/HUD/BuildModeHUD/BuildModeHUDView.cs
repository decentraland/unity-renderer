using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildModeHUDView : MonoBehaviour
{
    public SmartItemListView smartItemListView;
    public SceneLimitInfoController sceneLimitInfoController;
    public SceneObjectCatalogController sceneObjectCatalogController;
    public EntityInformationController entityInformationController;
    public ToolTipController toolTipController;
    public QuickBarView quickBarView;
    public BuilderInWorldEntityListController entityListController;
    public CatalogGroupListView catalogGroupListView;

    public GameObject firstPersonCanvasGO, godModeCanvasGO, extraBtnsGO, shortCutsGO;
    public Button firstPersonChangeModeBtn,changeModeBtn,extraBtn,controlsBtn,closeControlsBtn,hideUIBtn,entityListBtn,catalogBtn,closeCatalogBtn;
    public Button translateBtn, rotateBtn, scaleBtn, resetBtn, duplicateBtn, deleteBtn,publishBtn;
    public Button[] closeEntityListBtns;
    public Button tutorialBtn;
    public Button logOutBtn;

    public GameObject publishGO;
    public GameObject publishingGO;
    public GameObject publishingFinishedGO;

    public TextMeshProUGUI publishStatusTxt;

    [SerializeField] internal ShowHideAnimator showHideAnimator;
    [SerializeField] internal InputAction_Trigger toggleUIVisibilityInputAction;
    [SerializeField] internal InputAction_Trigger toggleControlsVisibilityInputAction;
    [SerializeField] internal InputAction_Trigger toggleTranslateInputAction;
    [SerializeField] internal InputAction_Trigger toggleRotateInputAction;
    [SerializeField] internal InputAction_Trigger toggleScaleInputAction;
    [SerializeField] internal InputAction_Trigger toggleDuplicateInputAction;
    [SerializeField] internal InputAction_Trigger toggleDeleteInputAction;
    [SerializeField] internal InputAction_Trigger toggleChangeCameraInputAction;
    [SerializeField] internal InputAction_Trigger toggleResetInputAction;
    [SerializeField] internal InputAction_Trigger toggleOpenEntityListInputAction;
    [SerializeField] internal InputAction_Trigger toggleSceneInfoInputAction;
    [SerializeField] internal InputAction_Trigger toggleCatalogInputAction;


    public event Action OnControlsVisibilityAction, OnChangeUIVisbilityAction, OnTranslateSelectionAction, OnRotateSelectionAction, OnScaleSelectionAction, OnResetSelectedAction, OnDuplicateSelectionAction, OnDeleteSelectionAction;
    public event Action OnChangeModeAction,OnExtraBtnsClick,OnEntityListChangeVisibilityAction,OnSceneLimitInfoControllerChangeVisibilityAction, OnSceneCatalogControllerChangeVisibilityAction;
    public event Action<bool> OnSceneLimitInfoChangeVisibility;

    public event Action<SceneObject> OnSceneObjectSelected;
    public event Action OnStopInput, OnResumeInput,OnTutorialAction,OnPublishAction;
    public event Action OnLogoutAction;
    public event Action OnSceneObjectDrop;

    private void Awake()
    {
        toggleUIVisibilityInputAction.OnTriggered += OnUIVisiblityToggleActionTriggered;
        toggleControlsVisibilityInputAction.OnTriggered += OnControlsToggleActionTriggered;

        toggleChangeCameraInputAction.OnTriggered += OnChangeModeActionTriggered;
        toggleTranslateInputAction.OnTriggered += OnTranslateActionTriggered;
        toggleRotateInputAction.OnTriggered += OnRotateActionTriggered;
        toggleScaleInputAction.OnTriggered += OnScaleActionTriggered;
        toggleResetInputAction.OnTriggered += OnResetActionTriggered;
        toggleDuplicateInputAction.OnTriggered += OnDuplicateActionTriggered;
        toggleDeleteInputAction.OnTriggered += OnDeleteActionTriggered;
        toggleOpenEntityListInputAction.OnTriggered += OnEntityListActionTriggered;
        toggleSceneInfoInputAction.OnTriggered += OnSceneLimitInfoControllerChangeVisibilityTriggered;
        toggleCatalogInputAction.OnTriggered += OnSceneCatalogControllerChangeVisibilityTriggered;


        entityListBtn.onClick.AddListener(() => OnEntityListChangeVisibilityAction?.Invoke());

        foreach (Button closeEntityListBtn in closeEntityListBtns)
        {
            closeEntityListBtn.onClick.AddListener(() => OnEntityListChangeVisibilityAction?.Invoke());
        }


        catalogBtn.onClick.AddListener(() => OnSceneCatalogControllerChangeVisibilityAction?.Invoke());
        closeCatalogBtn.onClick.AddListener(() => OnSceneCatalogControllerChangeVisibilityAction?.Invoke());


        changeModeBtn.onClick.AddListener(() => OnChangeModeAction?.Invoke());
        firstPersonChangeModeBtn.onClick.AddListener(() => OnChangeModeAction?.Invoke());
        extraBtn.onClick.AddListener(() => OnExtraBtnsClick?.Invoke());
        controlsBtn.onClick.AddListener(() => OnControlsVisibilityAction?.Invoke());
        closeControlsBtn.onClick.AddListener(() => OnControlsVisibilityAction?.Invoke());
        hideUIBtn.onClick.AddListener(() => OnChangeUIVisbilityAction?.Invoke());

        translateBtn.onClick.AddListener(() => OnTranslateSelectionAction?.Invoke());
        rotateBtn.onClick.AddListener(() => OnRotateSelectionAction?.Invoke());
        scaleBtn.onClick.AddListener(() => OnScaleSelectionAction?.Invoke());
        resetBtn.onClick.AddListener(() => OnResetSelectedAction?.Invoke());
        duplicateBtn.onClick.AddListener(() => OnDuplicateSelectionAction?.Invoke());
        deleteBtn.onClick.AddListener(() => OnDeleteSelectionAction?.Invoke());

        sceneObjectCatalogController.OnSceneObjectSelected += (x) => OnSceneObjectSelected?.Invoke(x);
        catalogGroupListView.OnResumeInput += () => OnResumeInput?.Invoke();
        catalogGroupListView.OnStopInput += () => OnStopInput?.Invoke();

        tutorialBtn.onClick.AddListener(() => OnTutorialAction?.Invoke());
        publishBtn.onClick.AddListener(() => OnPublishAction?.Invoke());
        logOutBtn.onClick.AddListener(() => OnLogoutAction?.Invoke());
    }

    private void OnDestroy()
    {
        toggleUIVisibilityInputAction.OnTriggered -= OnUIVisiblityToggleActionTriggered;
        toggleControlsVisibilityInputAction.OnTriggered -= OnControlsToggleActionTriggered;

        toggleChangeCameraInputAction.OnTriggered -= OnChangeModeActionTriggered;
        toggleTranslateInputAction.OnTriggered -= OnTranslateActionTriggered;
        toggleRotateInputAction.OnTriggered -= OnRotateActionTriggered;
        toggleScaleInputAction.OnTriggered -= OnScaleActionTriggered;
        toggleResetInputAction.OnTriggered -= OnResetActionTriggered;
        toggleDuplicateInputAction.OnTriggered -= OnDuplicateActionTriggered;
        toggleDeleteInputAction.OnTriggered -= OnDeleteActionTriggered;

        toggleOpenEntityListInputAction.OnTriggered -= OnEntityListActionTriggered;
        toggleSceneInfoInputAction.OnTriggered -= OnSceneLimitInfoControllerChangeVisibilityTriggered;
        toggleCatalogInputAction.OnTriggered -= OnSceneCatalogControllerChangeVisibilityTriggered;
    }

    public void PublishStart()
    {
        publishGO.SetActive(true);
        publishingGO.SetActive(true);
        publishingFinishedGO.SetActive(false);
    }

    public void PublishEnd(string message)
    {
        publishingGO.SetActive(false);
        publishingFinishedGO.SetActive(true);
        publishStatusTxt.text = message;
    }

    public void SetPublishBtnAvailability(bool isAvailable)
    {
        publishBtn.interactable = isAvailable;
    }

    public void RefreshCatalogAssetPack()
    {
        sceneObjectCatalogController.RefreshAssetPack();
    }

    public void RefreshCatalogContent()
    {
        sceneObjectCatalogController.RefreshCatalog();
    }

    public void SceneObjectDroppedInView()
    {
        OnSceneObjectDrop?.Invoke();
    }

    public void SetVisibilityOfCatalog(bool isVisible)
    {
        if (isVisible)
            sceneObjectCatalogController.OpenCatalog();
        else
            sceneObjectCatalogController.CloseCatalog();
    }

    public void ChangeVisibilityOfSceneLimit(bool shouldBeVisible)
    {
        OnSceneLimitInfoChangeVisibility?.Invoke(shouldBeVisible);
    }

    public void SetVisibilityOfSceneInfo(bool isVisible)
    {
        if (!isVisible)
        {
            sceneLimitInfoController.Disable();
        }
        else
        {
            sceneLimitInfoController.Enable();
        }
    }

    public void SetVisibilityOfControls(bool isVisible)
    {
        shortCutsGO.SetActive(isVisible);
    }

    public void SetVisibilityOfExtraBtns(bool isVisible)
    {
        extraBtnsGO.SetActive(isVisible);
    }

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

    public void HideToolTip()
    {
        toolTipController.Stop();
    }

    #region Triggers

    private void OnSceneCatalogControllerChangeVisibilityTriggered(DCLAction_Trigger action)
    {
        OnSceneCatalogControllerChangeVisibilityAction?.Invoke();
    }

    private void OnSceneLimitInfoControllerChangeVisibilityTriggered(DCLAction_Trigger action)
    {
        OnSceneLimitInfoControllerChangeVisibilityAction?.Invoke();
    }

    private void OnEntityListActionTriggered(DCLAction_Trigger action)
    {
        OnEntityListChangeVisibilityAction?.Invoke();
    }
    private void OnResetActionTriggered(DCLAction_Trigger action)
    {
        OnResetSelectedAction?.Invoke();
    }

    private void OnChangeModeActionTriggered(DCLAction_Trigger action)
    {
        OnChangeModeAction?.Invoke();
    }

    private void OnDeleteActionTriggered(DCLAction_Trigger action)
    {
        OnDeleteSelectionAction?.Invoke();
    }

    private void OnDuplicateActionTriggered(DCLAction_Trigger action)
    {
        OnDuplicateSelectionAction?.Invoke();
    }

    private void OnScaleActionTriggered(DCLAction_Trigger action)
    {
        OnScaleSelectionAction?.Invoke();
    }

    private void OnRotateActionTriggered(DCLAction_Trigger action)
    {
        OnRotateSelectionAction?.Invoke();
    }

    private void OnTranslateActionTriggered(DCLAction_Trigger action)
    {
        OnTranslateSelectionAction?.Invoke();
    }

    private void OnControlsToggleActionTriggered(DCLAction_Trigger action)
    {
        OnControlsVisibilityAction?.Invoke();
    }

    private void OnUIVisiblityToggleActionTriggered(DCLAction_Trigger action)
    {
        OnChangeUIVisbilityAction?.Invoke();
    }

    #endregion
}
