using UnityEngine;
using DCL.Helpers;

public class ControlsHUDController : IHUD
{
    internal ControlsHUDView view;

    private bool prevMouseLockState = false;

    public event System.Action OnControlsOpened;
    public event System.Action OnControlsClosed;

    public ControlsHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ControlsHUD")).GetComponent<ControlsHUDView>();
        view.name = "_ControlsHUD";
        view.gameObject.SetActive(false);

        view.onToggleActionTriggered += ToggleVisibility;
        view.onCloseActionTriggered += Hide;
    }

    public void SetVisibility(bool visible)
    {
        if (!view)
            return;

        if (IsVisible() && !visible)
        {
            if (prevMouseLockState)
            {
                Utils.LockCursor();
            }

            view.showHideAnimator.Hide();
            OnControlsClosed?.Invoke();

            AudioScriptableObjects.fadeOut.Play(true);
        }
        else if (!IsVisible() && visible)
        {
            prevMouseLockState = Utils.isCursorLocked;
            Utils.UnlockCursor();
            view.gameObject.SetActive(true);
            view.showHideAnimator.Show();
            OnControlsOpened?.Invoke();
            AudioScriptableObjects.fadeIn.Play(true);
        }
    }

    public void Dispose()
    {
        if (view)
        {
            Object.Destroy(view.gameObject);
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

    public void Hide(bool restorePointerLockStatus)
    {
        if (!restorePointerLockStatus)
            prevMouseLockState = false;
        SetVisibility(false);
    }
}