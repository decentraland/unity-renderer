using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IExtraActionsView
{
    event Action OnControlsClicked,
                 OnHideUIClicked,
                 OnTutorialClicked,
                 OnResetClicked,
                 OnResetCameraClicked;

    void OnControlsClick(DCLAction_Trigger action);
    void OnHideUIClick(DCLAction_Trigger action);
    void OnTutorialClick();
    void SetActive(bool isActive);
    void OnResetClick(DCLAction_Trigger action);
    void OnResetCameraClick(DCLAction_Trigger action);
    void SetResetButtonInteractable(bool isInteractable);
}

public class ExtraActionsView : MonoBehaviour, IExtraActionsView
{
    public event Action OnControlsClicked,
                        OnHideUIClicked,
                        OnTutorialClicked,
                        OnResetClicked,
                        OnResetCameraClicked;

    [Header("Buttons")]
    [SerializeField] internal Button hideUIBtn;
    [SerializeField] internal Button controlsBtn;
    [SerializeField] internal Button tutorialBtn;
    [SerializeField] internal Button resetBtn;
    [SerializeField] internal TMP_Text resetBtnText;
    [SerializeField] internal Button resetCameraBtn;

    [Header("Input Actions")]
    [SerializeField] internal InputAction_Trigger toggleUIVisibilityInputAction;
    [SerializeField] internal InputAction_Trigger toggleControlsVisibilityInputAction;
    [SerializeField] internal InputAction_Trigger toggleResetInputAction;
    [SerializeField] internal InputAction_Trigger toggleResetCameraInputAction;

    [Header("Other Configurations")]
    [SerializeField] internal Color disabledButtonTextColor;

    private DCLAction_Trigger dummyActionTrigger = new DCLAction_Trigger();
    private Color originalResetBtnColor;

    private const string VIEW_PATH = "GodMode/TopActionsButtons/ExtraActionsView";

    internal static ExtraActionsView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ExtraActionsView>();
        view.gameObject.name = "_ExtraActionsView";

        return view;
    }

    private void Awake()
    {
        hideUIBtn.onClick.AddListener(() => OnHideUIClick(dummyActionTrigger));
        controlsBtn.onClick.AddListener(() => OnControlsClick(dummyActionTrigger));
        resetBtn.onClick.AddListener(() => OnResetClick(dummyActionTrigger));
        resetCameraBtn.onClick.AddListener(() => OnResetCameraClick(dummyActionTrigger));
        tutorialBtn.onClick.AddListener(OnTutorialClick);

        toggleUIVisibilityInputAction.OnTriggered += OnHideUIClick;
        toggleControlsVisibilityInputAction.OnTriggered += OnControlsClick;
        toggleResetInputAction.OnTriggered += OnResetClick;
        toggleResetCameraInputAction.OnTriggered += OnResetCameraClick;

        originalResetBtnColor = resetBtnText.color;
        SetResetButtonInteractable(true);
    }

    private void OnDestroy()
    {
        hideUIBtn.onClick.RemoveAllListeners();
        controlsBtn.onClick.RemoveAllListeners();
        resetBtn.onClick.RemoveAllListeners();
        resetCameraBtn.onClick.RemoveAllListeners();
        tutorialBtn.onClick.RemoveListener(OnTutorialClick);

        toggleUIVisibilityInputAction.OnTriggered -= OnHideUIClick;
        toggleControlsVisibilityInputAction.OnTriggered -= OnControlsClick;
        toggleResetInputAction.OnTriggered -= OnResetClick;
        toggleResetCameraInputAction.OnTriggered -= OnResetCameraClick;
    }

    public void SetActive(bool isActive)
    {
        if (gameObject.activeSelf != isActive)
            gameObject.SetActive(isActive);
    }

    public void OnControlsClick(DCLAction_Trigger action) { OnControlsClicked?.Invoke(); }

    public void OnHideUIClick(DCLAction_Trigger action) { OnHideUIClicked?.Invoke(); }

    public void OnTutorialClick() { OnTutorialClicked?.Invoke(); }

    public void OnResetClick(DCLAction_Trigger action) { OnResetClicked?.Invoke(); }

    public void OnResetCameraClick(DCLAction_Trigger action) { OnResetCameraClicked?.Invoke(); }

    public void SetResetButtonInteractable(bool isInteractable)
    {
        resetBtn.interactable = isInteractable;
        resetBtnText.color = isInteractable ? originalResetBtnColor : disabledButtonTextColor;
    }
}