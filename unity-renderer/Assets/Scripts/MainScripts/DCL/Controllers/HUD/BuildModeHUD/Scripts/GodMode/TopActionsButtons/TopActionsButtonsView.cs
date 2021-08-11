using System;
using DCL.Configuration;
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
                 OnUndoClicked,
                 OnRedoClicked,
                 OnDuplicateClicked,
                 OnDeleteClicked,
                 OnLogOutClicked,
                 OnSnapModeClicked,
                 OnPointerExit;

    event Action<BaseEventData, string> OnChangeCameraModePointerEnter,
                                        OnTranslatePointerEnter,
                                        OnRotatePointerEnter,
                                        OnScalePointerEnter,
                                        OnUndoPointerEnter,
                                        OnRedoPointerEnter,
                                        OnDuplicatePointerEnter,
                                        OnDeletePointerEnter,
                                        OnMoreActionsPointerEnter,
                                        OnLogoutPointerEnter,
                                        OnSnapModePointerEnter;

    void ConfigureExtraActions(IExtraActionsController extraActionsController);
    void OnChangeModeClick(DCLAction_Trigger action);
    void OnDeleteClick(DCLAction_Trigger action);
    void OnDuplicateClick(DCLAction_Trigger action);
    void OnExtraClick(DCLAction_Trigger action);
    void OnLogOutClick(DCLAction_Trigger action);
    void OnRotateClick(DCLAction_Trigger action);
    void OnScaleClick(DCLAction_Trigger action);
    void OnTranslateClick(DCLAction_Trigger action);
    void SetGizmosActive(string gizmos);
    void SetActionsInteractable(bool isActive);
    void SetUndoInteractable(bool isActive);
    void SetRedoInteractable(bool isActive);
    void SetSnapActive(bool isActive);
}

public class TopActionsButtonsView : MonoBehaviour, ITopActionsButtonsView
{
    public event Action OnChangeModeClicked,
                        OnExtraClicked,
                        OnTranslateClicked,
                        OnRotateClicked,
                        OnScaleClicked,
                        OnUndoClicked,
                        OnRedoClicked,
                        OnDuplicateClicked,
                        OnDeleteClicked,
                        OnLogOutClicked,
                        OnSnapModeClicked,
                        OnPointerExit;

    public event Action<BaseEventData, string> OnChangeCameraModePointerEnter,
                                               OnTranslatePointerEnter,
                                               OnRotatePointerEnter,
                                               OnScalePointerEnter,
                                               OnUndoPointerEnter,
                                               OnRedoPointerEnter,
                                               OnDuplicatePointerEnter,
                                               OnDeletePointerEnter,
                                               OnMoreActionsPointerEnter,
                                               OnLogoutPointerEnter,
                                               OnSnapModePointerEnter;

    [Header("Buttons")]
    [SerializeField] internal Button changeModeBtn;

    [SerializeField] internal Button extraBtn;
    [SerializeField] internal Button translateBtn;
    [SerializeField] internal Button rotateBtn;
    [SerializeField] internal Button scaleBtn;
    [SerializeField] internal Button undoBtn;
    [SerializeField] internal Button redoBtn;
    [SerializeField] internal Button duplicateBtn;
    [SerializeField] internal Button deleteBtn;
    [SerializeField] internal Button logOutBtn;
    [SerializeField] internal Button snapModeBtn;

    [Header("Input Actions")]
    [SerializeField] internal InputAction_Trigger toggleChangeCameraInputAction;

    [SerializeField] internal InputAction_Trigger toggleTranslateInputAction;
    [SerializeField] internal InputAction_Trigger toggleRotateInputAction;
    [SerializeField] internal InputAction_Trigger toggleScaleInputAction;
    [SerializeField] internal InputAction_Trigger toggleDuplicateInputAction;
    [SerializeField] internal InputAction_Trigger toggleDeleteInputAction;

    [Header("Event Triggers")]
    [SerializeField] internal EventTrigger changeCameraModeEventTrigger;

    [SerializeField] internal EventTrigger translateEventTrigger;
    [SerializeField] internal EventTrigger rotateEventTrigger;
    [SerializeField] internal EventTrigger scaleEventTrigger;
    [SerializeField] internal EventTrigger undoEventTrigger;
    [SerializeField] internal EventTrigger redoEventTrigger;
    [SerializeField] internal EventTrigger duplicateEventTrigger;
    [SerializeField] internal EventTrigger deleteEventTrigger;
    [SerializeField] internal EventTrigger moreActionsEventTrigger;
    [SerializeField] internal EventTrigger logoutEventTrigger;
    [SerializeField] internal EventTrigger snapModeEventTrigger;

    [Header("Tooltip Texts")]
    [SerializeField] internal string changeCameraModeTooltipText = "Change Camera (V)";

    [SerializeField] internal string translateTooltipText = "Translate (M)";
    [SerializeField] internal string rotateTooltipText = "Rotate (R)";
    [SerializeField] internal string scaleTooltipText = "Scale (G)";
    [SerializeField] internal string undoTooltipText = "Undo (Shift+Z)";
    [SerializeField] internal string redoTooltipText = "Redo (Shift+Y)";
    [SerializeField] internal string duplicateTooltipText = "Duplicate (Shift+D)";
    [SerializeField] internal string deleteTooltipText = "Delete (Del) or (Backspace)";
    [SerializeField] internal string moreActionsTooltipText = "Extra Actions";
    [SerializeField] internal string logoutTooltipText = "Exit from edition";
    [SerializeField] internal string snapModeTooltipText = "Toggle Snapping (O)";

    [Header("Sub-Views")]
    [SerializeField] internal ExtraActionsView extraActionsView;

    [Header("Images")]
    [SerializeField] internal Image translateGizmosBtnImg;

    [SerializeField] internal Image rotateGizmosBtnImg;
    [SerializeField] internal Image scaleGizmosBtnImg;
    [SerializeField] internal Image snapModeBtnImg;

    [Header("Colors")]
    [SerializeField] internal Color normalBtnImgColor;

    [SerializeField] internal Color selectedBtnImgColor;

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
        undoBtn.onClick.AddListener(OnUndoClick);
        redoBtn.onClick.AddListener(OnRedoClick);
        duplicateBtn.onClick.AddListener(() => OnDuplicateClick(dummyActionTrigger));
        deleteBtn.onClick.AddListener(() => OnDeleteClick(dummyActionTrigger));
        logOutBtn.onClick.AddListener(() => OnLogOutClick(dummyActionTrigger));
        extraBtn.onClick.AddListener(() => OnExtraClick(dummyActionTrigger));
        snapModeBtn.onClick.AddListener(() => OnSnapModeClick(dummyActionTrigger));

        BIWUtils.ConfigureEventTrigger(
            changeCameraModeEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnChangeCameraModePointerEnter?.Invoke(eventData, changeCameraModeTooltipText));

        BIWUtils.ConfigureEventTrigger(
            changeCameraModeEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            translateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnTranslatePointerEnter?.Invoke(eventData, translateTooltipText));

        BIWUtils.ConfigureEventTrigger(
            translateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            rotateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnRotatePointerEnter?.Invoke(eventData, rotateTooltipText));

        BIWUtils.ConfigureEventTrigger(
            rotateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            scaleEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnScalePointerEnter?.Invoke(eventData, scaleTooltipText));

        BIWUtils.ConfigureEventTrigger(
            scaleEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            undoEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnUndoPointerEnter?.Invoke(eventData, undoTooltipText));

        BIWUtils.ConfigureEventTrigger(
            undoEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            redoEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnRedoPointerEnter?.Invoke(eventData, redoTooltipText));

        BIWUtils.ConfigureEventTrigger(
            redoEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            duplicateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnDuplicatePointerEnter?.Invoke(eventData, duplicateTooltipText));

        BIWUtils.ConfigureEventTrigger(
            duplicateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            deleteEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnDeletePointerEnter?.Invoke(eventData, deleteTooltipText));

        BIWUtils.ConfigureEventTrigger(
            deleteEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            moreActionsEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnMoreActionsPointerEnter?.Invoke(eventData, moreActionsTooltipText));

        BIWUtils.ConfigureEventTrigger(
            moreActionsEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            logoutEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnLogoutPointerEnter?.Invoke(eventData, logoutTooltipText));

        BIWUtils.ConfigureEventTrigger(
            logoutEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        BIWUtils.ConfigureEventTrigger(
            snapModeEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnSnapModePointerEnter?.Invoke(eventData, snapModeTooltipText));

        BIWUtils.ConfigureEventTrigger(
            snapModeEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        //TODO: This should be reactivate when we activate the first person camera
        //  toggleChangeCameraInputAction.OnTriggered += OnChangeModeClick;
        toggleTranslateInputAction.OnTriggered += OnTranslateClick;
        toggleRotateInputAction.OnTriggered += OnRotateClick;
        toggleScaleInputAction.OnTriggered += OnScaleClick;
        toggleDuplicateInputAction.OnTriggered += OnDuplicateClick;
        toggleDeleteInputAction.OnTriggered += OnDeleteClick;
    }

    private void OnDestroy()
    {
        changeModeBtn.onClick.RemoveAllListeners();
        translateBtn.onClick.RemoveAllListeners();
        rotateBtn.onClick.RemoveAllListeners();
        scaleBtn.onClick.RemoveAllListeners();
        undoBtn.onClick.RemoveAllListeners();
        redoBtn.onClick.RemoveAllListeners();
        duplicateBtn.onClick.RemoveAllListeners();
        deleteBtn.onClick.RemoveAllListeners();
        logOutBtn.onClick.RemoveAllListeners();
        extraBtn.onClick.RemoveAllListeners();
        snapModeBtn.onClick.RemoveAllListeners();

        BIWUtils.RemoveEventTrigger(changeCameraModeEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(changeCameraModeEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(translateEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(translateEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(rotateEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(rotateEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(scaleEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(scaleEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(undoEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(undoEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(redoEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(redoEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(duplicateEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(duplicateEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(deleteEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(deleteEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(moreActionsEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(moreActionsEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(logoutEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(logoutEventTrigger, EventTriggerType.PointerExit);
        BIWUtils.RemoveEventTrigger(snapModeEventTrigger, EventTriggerType.PointerEnter);
        BIWUtils.RemoveEventTrigger(snapModeEventTrigger, EventTriggerType.PointerExit);

        toggleChangeCameraInputAction.OnTriggered -= OnChangeModeClick;
        toggleTranslateInputAction.OnTriggered -= OnTranslateClick;
        toggleRotateInputAction.OnTriggered -= OnRotateClick;
        toggleScaleInputAction.OnTriggered -= OnScaleClick;
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

    public void OnChangeModeClick(DCLAction_Trigger action) { OnChangeModeClicked?.Invoke(); }

    public void OnExtraClick(DCLAction_Trigger action) { OnExtraClicked?.Invoke(); }

    public void OnTranslateClick(DCLAction_Trigger action) { OnTranslateClicked?.Invoke(); }
    public void OnSnapModeClick(DCLAction_Trigger action) { OnSnapModeClicked?.Invoke(); }

    public void SetSnapActive(bool isActive) { snapModeBtnImg.color = isActive ? selectedBtnImgColor : normalBtnImgColor; }

    public void SetGizmosActive(string gizmos)
    {
        translateGizmosBtnImg.color = normalBtnImgColor;
        rotateGizmosBtnImg.color = normalBtnImgColor;
        scaleGizmosBtnImg.color = normalBtnImgColor;

        switch (gizmos)
        {
            case BIWSettings.TRANSLATE_GIZMO_NAME:
                translateGizmosBtnImg.color = selectedBtnImgColor;
                break;
            case BIWSettings.ROTATE_GIZMO_NAME:
                rotateGizmosBtnImg.color = selectedBtnImgColor;
                break;
            case BIWSettings.SCALE_GIZMO_NAME:
                scaleGizmosBtnImg.color = selectedBtnImgColor;
                break;
        }
    }

    public void SetActionsInteractable(bool isActive)
    {
        duplicateBtn.interactable = isActive;
        deleteBtn.interactable = isActive;
    }

    public void SetUndoInteractable(bool isActive) { undoBtn.interactable = isActive; }

    public void SetRedoInteractable(bool isActive) { redoBtn.interactable = isActive; }

    public void OnRotateClick(DCLAction_Trigger action) { OnRotateClicked?.Invoke(); }

    public void OnScaleClick(DCLAction_Trigger action) { OnScaleClicked?.Invoke(); }

    public void OnUndoClick() { OnUndoClicked?.Invoke(); }
    public void OnRedoClick() { OnRedoClicked?.Invoke(); }

    public void OnDuplicateClick(DCLAction_Trigger action) { OnDuplicateClicked?.Invoke(); }

    public void OnDeleteClick(DCLAction_Trigger action) { OnDeleteClicked?.Invoke(); }

    public void OnLogOutClick(DCLAction_Trigger action) { OnLogOutClicked?.Invoke(); }
}