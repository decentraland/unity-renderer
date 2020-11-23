using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildModeHUDView : MonoBehaviour
{

    public SceneLimitInfoController sceneLimitInfoController;
    public SceneObjectCatalogController sceneObjectCatalogController;
    public ToolTipController toolTipController;

    public GameObject firstPersonCanvasGO, godModeCanvasGO, extraBtnsGO, shortCutsGO;
    public Button firstPersonChangeModeBtn,changeModeBtn,extraBtn,controlsBtn,closeControlsBtn,hideUIBtn,entityListBtn,closeEntityListBtn,catalogBtn,closeCatalogBtn;
    public Button translateBtn, rotateBtn, scaleBtn, resetBtn, duplicateBtn, deleteBtn,publishBtn;

    public Button tutorialBtn;
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

    [SerializeField] internal InputAction_Trigger quickBar1InputAction;
    [SerializeField] internal InputAction_Trigger quickBar2InputAction;
    [SerializeField] internal InputAction_Trigger quickBar3InputAction;
    [SerializeField] internal InputAction_Trigger quickBar4InputAction;
    [SerializeField] internal InputAction_Trigger quickBar5InputAction;
    [SerializeField] internal InputAction_Trigger quickBar6InputAction;
    [SerializeField] internal InputAction_Trigger quickBar7InputAction;
    [SerializeField] internal InputAction_Trigger quickBar8InputAction;
    [SerializeField] internal InputAction_Trigger quickBar9InputAction;

    public event Action OnControlsVisibilityAction, OnChangeUIVisbilityAction, OnTranslateSelectionAction, OnRotateSelectionAction, OnScaleSelectionAction, OnResetSelectedAction, OnDuplicateSelectionAction, OnDeleteSelectionAction;
    public event Action OnChangeModeAction,OnExtraBtnsClick,OnEntityListChangeVisibilityAction,OnSceneLimitInfoControllerChangeVisibilityAction, OnSceneCatalogControllerChangeVisibilityAction;
    public event Action<bool> OnSceneLimitInfoChangeVisibility;

    public event Action<SceneObject> OnSceneObjectSelected;
    public event Action OnStopInput, OnResumeInput,OnTutorialAction,OnPublishAction;

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

        quickBar1InputAction.OnTriggered += OnQuickBar1InputTriggedered;
        quickBar2InputAction.OnTriggered += OnQuickBar2InputTriggedered;
        quickBar3InputAction.OnTriggered += OnQuickBar3InputTriggedered;
        quickBar4InputAction.OnTriggered += OnQuickBar4InputTriggedered;
        quickBar5InputAction.OnTriggered += OnQuickBar5InputTriggedered;
        quickBar6InputAction.OnTriggered += OnQuickBar6InputTriggedered;
        quickBar7InputAction.OnTriggered += OnQuickBar7InputTriggedered;
        quickBar8InputAction.OnTriggered += OnQuickBar8InputTriggedered;
        quickBar9InputAction.OnTriggered += OnQuickBar9InputTriggedered;

       


        entityListBtn.onClick.AddListener(() => OnEntityListChangeVisibilityAction?.Invoke());
        closeEntityListBtn.onClick.AddListener(() => OnEntityListChangeVisibilityAction?.Invoke());
        

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
        sceneObjectCatalogController.OnResumeInput += () => OnResumeInput?.Invoke();
        sceneObjectCatalogController.OnStopInput += () => OnStopInput?.Invoke();

        tutorialBtn.onClick.AddListener(() => OnTutorialAction?.Invoke());
        publishBtn.onClick.AddListener(() => OnPublishAction?.Invoke()); 
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

        quickBar1InputAction.OnTriggered -= OnQuickBar1InputTriggedered;
        quickBar2InputAction.OnTriggered -= OnQuickBar2InputTriggedered;
        quickBar3InputAction.OnTriggered -= OnQuickBar3InputTriggedered;
        quickBar4InputAction.OnTriggered -= OnQuickBar4InputTriggedered;
        quickBar5InputAction.OnTriggered -= OnQuickBar5InputTriggedered;
        quickBar6InputAction.OnTriggered -= OnQuickBar6InputTriggedered;
        quickBar7InputAction.OnTriggered -= OnQuickBar7InputTriggedered;
        quickBar8InputAction.OnTriggered -= OnQuickBar8InputTriggedered;
        quickBar9InputAction.OnTriggered -= OnQuickBar9InputTriggedered;
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
    }

    public void SetGodModeView()
    {
        firstPersonCanvasGO.SetActive(false);
        godModeCanvasGO.SetActive(true);
    }

    public void HideToolTip()
    {
        toolTipController.Desactivate();
    }


    void QuickBarInput(int quickBarSlot)
    {
        sceneObjectCatalogController.QuickBarObjectSelected(quickBarSlot);
    }
    #region Triggers

    private void OnQuickBar9InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(8);
    }
    private void OnQuickBar8InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(7);
    }
    private void OnQuickBar7InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(6);
    }
    private void OnQuickBar6InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(5);
    }
    private void OnQuickBar5InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(4);
    }
    private void OnQuickBar4InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(3);
    }
    private void OnQuickBar3InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(2);
    }
    private void OnQuickBar2InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(1);
    }
    private void OnQuickBar1InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(0);
    }
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
