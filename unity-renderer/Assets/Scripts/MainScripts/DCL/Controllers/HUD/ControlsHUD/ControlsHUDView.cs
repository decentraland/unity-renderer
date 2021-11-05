using UnityEngine;
using System;

public class ControlsHUDView : MonoBehaviour
{
    [SerializeField] internal InputAction_Trigger closeAction;
    [SerializeField] internal ShowHideAnimator showHideAnimator;
    [SerializeField] internal Button_OnPointerDown closeButton;
    [SerializeField] internal GameObject voiceChatButton;
    [SerializeField] internal GameObject builderInWorldButton;

    public event Action<bool> onCloseActionTriggered;

    private void Awake()
    {
        closeAction.OnTriggered += OnCloseActionTriggered;
        closeButton.onPointerDown += () => Close(true);
    }

    private void OnDestroy() { closeAction.OnTriggered -= OnCloseActionTriggered; }

    private void OnCloseActionTriggered(DCLAction_Trigger action) { Close(false); }

    private void Close(bool closedByButtonPress) { onCloseActionTriggered?.Invoke(closedByButtonPress); }
}