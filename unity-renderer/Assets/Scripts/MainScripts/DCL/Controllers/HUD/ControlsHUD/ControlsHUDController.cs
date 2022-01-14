using DCL;
using UnityEngine;
using DCL.Helpers;

public class ControlsHUDController : IHUD
{
    internal ControlsHUDView view;

    private bool prevMouseLockState = false;

    public event System.Action OnControlsOpened;
    public event System.Action OnControlsClosed;

    public BaseVariable<bool> controlsVisible => DataStore.i.HUDs.controlsVisible;

    public ControlsHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ControlsHUD")).GetComponent<ControlsHUDView>();
        view.name = "_ControlsHUD";
        view.gameObject.SetActive(false);

        controlsVisible.OnChange += OnControlsVisibleChanged;
        OnControlsVisibleChanged(controlsVisible.Get(), false);

        view.onCloseActionTriggered += Hide;

        if (!DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
        {
            KernelConfig.i.EnsureConfigInitialized().Then(config => OnKernelConfigChanged(config, null));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }
    }

    public void SetVisibility(bool visible) { controlsVisible.Set(visible); }

    private void OnControlsVisibleChanged(bool current, bool previous) { SetVisibility_Internal(current); }

    private void SetVisibility_Internal(bool visible)
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
            prevMouseLockState = Utils.IsCursorLocked;
            Utils.UnlockCursor();
            view.gameObject.SetActive(true);
            view.showHideAnimator.Show();
            OnControlsOpened?.Invoke();
            AudioScriptableObjects.fadeIn.Play(true);
        }
    }

    public void Dispose()
    {
        controlsVisible.OnChange -= OnControlsVisibleChanged;
        if (view)
        {
            Object.Destroy(view.gameObject);
        }

        if (!DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
        }
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
        controlsVisible.Set(false);
    }

    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
    {
        view?.voiceChatButton.SetActive(current.comms.voiceChatEnabled);
        view?.builderInWorldButton.SetActive(current.features.enableBuilderInWorld);
    }
}