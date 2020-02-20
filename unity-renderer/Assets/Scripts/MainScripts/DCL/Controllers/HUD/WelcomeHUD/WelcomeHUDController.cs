using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

public class WelcomeHUDController : IHUD, System.IDisposable
{
    [System.Serializable]
    public class Model : HUDConfiguration
    {
        public bool hasWallet; //TODO(Brian): Use WEB3 param in userProfile
    }

    internal WelcomeHUDView view;
    internal Model model;

    public System.Action OnConfirmed;
    public System.Action OnDismissed;

    public void Initialize(Model model)
    {
        this.model = model;

        view = WelcomeHUDView.CreateView(model.hasWallet);
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
        if (model != null)
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
