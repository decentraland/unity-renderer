using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

internal class ExploreHUDView : MonoBehaviour
{
    public event UnityAction OnCloseButtonPressed;

    [SerializeField] internal HighlightScenesController highlightScenesController;
    [SerializeField] internal ShowHideAnimator popup;
    [SerializeField] internal Button_OnPointerDown closeButton;
    [SerializeField] internal GotoMagicButton gotoMagicButton;
    [SerializeField] internal Button_OnPointerDown togglePopupButton;
    [SerializeField] internal Color[] friendColors = null;
    [SerializeField] internal InputAction_Trigger closeAction;

    private InputAction_Trigger.Triggered closeActionDelegate;

    private void Awake()
    {
        closeActionDelegate = (x) => RaiseOnClose();
        closeButton.onPointerDown += RaiseOnClose;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            if (!IsActive())
            {
                popup.gameObject.SetActive(true);
            }
            popup.Show();
        }
        else
        {
            popup.Hide();
        }
    }

    public bool IsVisible()
    {
        return popup.isVisible;
    }

    public bool IsActive()
    {
        return popup.gameObject.activeSelf;
    }

    public void RefreshData()
    {
        highlightScenesController.RefreshIfNeeded();
    }

    public void Initialize(FriendTrackerController friendsController)
    {
        highlightScenesController.Initialize(friendsController);
    }

    private void RaiseOnClose()
    {
        OnCloseButtonPressed?.Invoke();
    }

    private void OnEnable()
    {
        closeAction.OnTriggered += closeActionDelegate;
    }

    private void OnDisable()
    {
        closeAction.OnTriggered -= closeActionDelegate;
    }
}
