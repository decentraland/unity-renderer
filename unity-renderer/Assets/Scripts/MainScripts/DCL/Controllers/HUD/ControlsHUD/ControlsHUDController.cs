using UnityEngine;
using DCL.Helpers;

public class ControlsHUDController : IHUD
{
    internal ControlsHUDView view;

    private bool prevMouseLockState = false;

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

            if (HUDAudioPlayer.i != null)
                HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.fadeOut);
        }
        else if (!IsVisible() && visible)
        {
            prevMouseLockState = Utils.isCursorLocked;
            Utils.UnlockCursor();
            view.gameObject.SetActive(true);
            view.showHideAnimator.Show();
            if (HUDAudioPlayer.i != null)
                HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.fadeIn);
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