using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface ITopActionsButtonsView
{
    event Action OnChangeModeClicked,
                 OnExtraClicked,
                 OnTranslateClicked,
                 OnRotateClicked,
                 OnScaleClicked,
                 OnResetClicked,
                 OnDuplicateClicked,
                 OnDeleteClicked,
                 OnLogOutClicked,
                 OnPointerExit;

    event Action<BaseEventData, string> OnChangeCameraModePointerEnter,
                                        OnTranslatePointerEnter,
                                        OnRotatePointerEnter,
                                        OnScalePointerEnter,
                                        OnResetPointerEnter,
                                        OnDuplicatePointerEnter,
                                        OnDeletePointerEnter,
                                        OnMoreActionsPointerEnter,
                                        OnLogoutPointerEnter;

    void ConfigureExtraActions(IExtraActionsController extraActionsController);
    void OnChangeModeClick(DCLAction_Trigger action);
    void OnDeleteClick(DCLAction_Trigger action);
    void OnDuplicateClick(DCLAction_Trigger action);
    void OnExtraClick(DCLAction_Trigger action);
    void OnLogOutClick(DCLAction_Trigger action);
    void OnResetClick(DCLAction_Trigger action);
    void OnRotateClick(DCLAction_Trigger action);
    void OnScaleClick(DCLAction_Trigger action);
    void OnTranslateClick(DCLAction_Trigger action);
}

public class TopActionsButtonsView : MonoBehaviour, ITopActionsButtonsView
{
    public event Action OnChangeModeClicked,
                        OnExtraClicked,
                        OnTranslateClicked,
                        OnRotateClicked,
                        OnScaleClicked,
                        OnResetClicked,
                        OnDuplicateClicked,
                        OnDeleteClicked,
                        OnLogOutClicked,
                        OnPointerExit;

    public event Action<BaseEventData, string> OnChangeCameraModePointerEnter,
                                               OnTranslatePointerEnter,
                                               OnRotatePointerEnter,
                                               OnScalePointerEnter,
                                               OnResetPointerEnter,
                                               OnDuplicatePointerEnter,
                                               OnDeletePointerEnter,
                                               OnMoreActionsPointerEnter,
                                               OnLogoutPointerEnter;

    [Header("Buttons")]
    [SerializeField] internal Button changeModeBtn;
    [SerializeField] internal Button extraBtn;
    [SerializeField] internal Button translateBtn;
    [SerializeField] internal Button rotateBtn;
    [SerializeField] internal Button scaleBtn;
    [SerializeField] internal Button resetBtn;
    [SerializeField] internal Button duplicateBtn;
    [SerializeField] internal Button deleteBtn;
    [SerializeField] internal Button logOutBtn;

    [Header("Input Actions")]
    [SerializeField] internal InputAction_Trigger toggleChangeCameraInputAction;
    [SerializeField] internal InputAction_Trigger toggleTranslateInputAction;
    [SerializeField] internal InputAction_Trigger toggleRotateInputAction;
    [SerializeField] internal InputAction_Trigger toggleScaleInputAction;
    [SerializeField] internal InputAction_Trigger toggleResetInputAction;
    [SerializeField] internal InputAction_Trigger toggleDuplicateInputAction;
    [SerializeField] internal InputAction_Trigger toggleDeleteInputAction;

    [Header("Event Triggers")]
    [SerializeField] internal EventTrigger changeCameraModeEventTrigger;
    [SerializeField] internal EventTrigger translateEventTrigger;
    [SerializeField] internal EventTrigger rotateEventTrigger;
    [SerializeField] internal EventTrigger scaleEventTrigger;
    [SerializeField] internal EventTrigger resetEventTrigger;
    [SerializeField] internal EventTrigger duplicateEventTrigger;
    [SerializeField] internal EventTrigger deleteEventTrigger;
    [SerializeField] internal EventTrigger moreActionsEventTrigger;
    [SerializeField] internal EventTrigger logoutEventTrigger;

    [Header("Tooltip Texts")]
    [SerializeField] internal string changeCameraModeTooltipText = "Change Camera (V)";
    [SerializeField] internal string translateTooltipText = "Translate (M)";
    [SerializeField] internal string rotateTooltipText = "Rotate (R)";
    [SerializeField] internal string scaleTooltipText = "Scale (G)";
    [SerializeField] internal string resetTooltipText = "Reset (Control+R)";
    [SerializeField] internal string duplicateTooltipText = "Duplicate (Control+D)";
    [SerializeField] internal string deleteTooltipText = "Delete (Del) or (Backspace)";
    [SerializeField] internal string moreActionsTooltipText = "Extra Actions";
    [SerializeField] internal string logoutTooltipText = "Exit from edition";

    [Header("Sub-Views")]
    [SerializeField] internal ExtraActionsView extraActionsView;

    private DCLAction_Trigger dummyActionTrigger = new DCLAction_Trigger();
    internal IExtraActionsController extraActionsController;

    private const string VIEW_PATH = "GodMode/TopActionsButtons/TopActionsButtonsView";

    internal static TopActionsButtonsView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TopActionsButtonsView>();
        view.gameObject.name = "_TopActionsButtonsView";

        return view;
    }

    private void Awake()
    {
        changeModeBtn.onClick.AddListener(() => OnChangeModeClick(dummyActionTrigger));
        translateBtn.onClick.AddListener(() => OnTranslateClick(dummyActionTrigger));
        rotateBtn.onClick.AddListener(() => OnRotateClick(dummyActionTrigger));
        scaleBtn.onClick.AddListener(() => OnScaleClick(dummyActionTrigger));
        resetBtn.onClick.AddListener(() => OnResetClick(dummyActionTrigger));
        duplicateBtn.onClick.AddListener(() => OnDuplicateClick(dummyActionTrigger));
        deleteBtn.onClick.AddListener(() => OnDeleteClick(dummyActionTrigger));
        logOutBtn.onClick.AddListener(() => OnLogOutClick(dummyActionTrigger));
        extraBtn.onClick.AddListener(() => OnExtraClick(dummyActionTrigger));

        BuilderInWorldUtils.ConfigureEventTrigger(
            changeCameraModeEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnChangeCameraModePointerEnter?.Invoke(eventData, changeCameraModeTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            changeCameraModeEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BuilderInWorldUtils.ConfigureEventTrigger(
            translateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnTranslatePointerEnter?.Invoke(eventData, translateTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            translateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BuilderInWorldUtils.ConfigureEventTrigger(
            rotateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnRotatePointerEnter?.Invoke(eventData, rotateTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            rotateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BuilderInWorldUtils.ConfigureEventTrigger(
            scaleEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnScalePointerEnter?.Invoke(eventData, scaleTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            scaleEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BuilderInWorldUtils.ConfigureEventTrigger(
            resetEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnResetPointerEnter?.Invoke(eventData, resetTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            resetEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BuilderInWorldUtils.ConfigureEventTrigger(
            duplicateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnDuplicatePointerEnter?.Invoke(eventData, duplicateTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            duplicateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BuilderInWorldUtils.ConfigureEventTrigger(
            deleteEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnDeletePointerEnter?.Invoke(eventData, deleteTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            deleteEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BuilderInWorldUtils.ConfigureEventTrigger(
            moreActionsEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnMoreActionsPointerEnter?.Invoke(eventData, moreActionsTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            moreActionsEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BuilderInWorldUtils.ConfigureEventTrigger(
            logoutEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnLogoutPointerEnter?.Invoke(eventData, logoutTooltipText));

        BuilderInWorldUtils.ConfigureEventTrigger(
            logoutEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        toggleChangeCameraInputAction.OnTriggered += OnChangeModeClick;
        toggleTranslateInputAction.OnTriggered += OnTranslateClick;
        toggleRotateInputAction.OnTriggered += OnRotateClick;
        toggleScaleInputAction.OnTriggered += OnScaleClick;
        toggleResetInputAction.OnTriggered += OnResetClick;
        toggleDuplicateInputAction.OnTriggered += OnDuplicateClick;
        toggleDeleteInputAction.OnTriggered += OnDeleteClick;
    }

    private void OnDestroy()
    {
        changeModeBtn.onClick.RemoveAllListeners();
        translateBtn.onClick.RemoveAllListeners();
        rotateBtn.onClick.RemoveAllListeners();
        scaleBtn.onClick.RemoveAllListeners();
        resetBtn.onClick.RemoveAllListeners();
        duplicateBtn.onClick.RemoveAllListeners();
        deleteBtn.onClick.RemoveAllListeners();
        logOutBtn.onClick.RemoveAllListeners();
        extraBtn.onClick.RemoveAllListeners();

        BuilderInWorldUtils.RemoveEventTrigger(changeCameraModeEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(changeCameraModeEventTrigger, EventTriggerType.PointerExit);
        BuilderInWorldUtils.RemoveEventTrigger(translateEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(translateEventTrigger, EventTriggerType.PointerExit);
        BuilderInWorldUtils.RemoveEventTrigger(rotateEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(rotateEventTrigger, EventTriggerType.PointerExit);
        BuilderInWorldUtils.RemoveEventTrigger(scaleEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(scaleEventTrigger, EventTriggerType.PointerExit);
        BuilderInWorldUtils.RemoveEventTrigger(resetEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(resetEventTrigger, EventTriggerType.PointerExit);
        BuilderInWorldUtils.RemoveEventTrigger(duplicateEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(duplicateEventTrigger, EventTriggerType.PointerExit);
        BuilderInWorldUtils.RemoveEventTrigger(deleteEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(deleteEventTrigger, EventTriggerType.PointerExit);
        BuilderInWorldUtils.RemoveEventTrigger(moreActionsEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(moreActionsEventTrigger, EventTriggerType.PointerExit);
        BuilderInWorldUtils.RemoveEventTrigger(logoutEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(logoutEventTrigger, EventTriggerType.PointerExit);

        toggleChangeCameraInputAction.OnTriggered -= OnChangeModeClick;
        toggleTranslateInputAction.OnTriggered -= OnTranslateClick;
        toggleRotateInputAction.OnTriggered -= OnRotateClick;
        toggleScaleInputAction.OnTriggered -= OnScaleClick;
        toggleResetInputAction.OnTriggered -= OnResetClick;
        toggleDuplicateInputAction.OnTriggered -= OnDuplicateClick;
        toggleDeleteInputAction.OnTriggered -= OnDeleteClick;

        if (extraActionsController != null)
            extraActionsController.Dispose();
    }

    public void ConfigureExtraActions(IExtraActionsController extraActionsController)
    {
        this.extraActionsController = extraActionsController;
        this.extraActionsController.Initialize(extraActionsView);
    }

    public void OnChangeModeClick(DCLAction_Trigger action)
    {
        OnChangeModeClicked?.Invoke();
    }

    public void OnExtraClick(DCLAction_Trigger action)
    {
        OnExtraClicked?.Invoke();
    }

    public void OnTranslateClick(DCLAction_Trigger action)
    {
        OnTranslateClicked?.Invoke();
    }

    public void OnRotateClick(DCLAction_Trigger action)
    {
        OnRotateClicked?.Invoke();
    }

    public void OnScaleClick(DCLAction_Trigger action)
    {
        OnScaleClicked?.Invoke();
    }

    public void OnResetClick(DCLAction_Trigger action)
    {
        OnResetClicked?.Invoke();
    }

    public void OnDuplicateClick(DCLAction_Trigger action)
    {
        OnDuplicateClicked?.Invoke();
    }

    public void OnDeleteClick(DCLAction_Trigger action)
    {
        OnDeleteClicked?.Invoke();
    }

    public void OnLogOutClick(DCLAction_Trigger action)
    {
        OnLogOutClicked?.Invoke();
    }
}
