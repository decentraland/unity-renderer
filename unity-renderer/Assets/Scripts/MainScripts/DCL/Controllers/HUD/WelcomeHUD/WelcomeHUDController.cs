using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

public class WelcomeHUDController : IHUD
{
    internal WelcomeHUDView view;

    public System.Action OnConfirmed;
    public System.Action OnDismissed;

    bool hasWallet;
    public void Initialize(bool hasWallet)
    {
        this.hasWallet = hasWallet;
        view = WelcomeHUDView.CreateView(hasWallet);
        view.Initialize(OnConfirmPressed, OnClosePressed);

        Utils.UnlockCursor();
    }

    internal void Close()
    {
        SetVisibility(false);
        Utils.LockCursor();
    }

    void OnConfirmPressed()
    {
        Close();

        if (hasWallet)
        {
            OnConfirmed?.Invoke();
            WebInterface.ReportMotdClicked();
        }
        else
        {
            OnDismissed?.Invoke();
        }
    }

    void OnClosePressed()
    {
        Close();
        OnDismissed?.Invoke();
    }

    public void Dispose()
    {
        if (view != null)
            Object.Destroy(view.gameObject);
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
        if (visible)
        {
            Utils.UnlockCursor();
        }
        else
        {
            Utils.LockCursor();
        }
    }
}
