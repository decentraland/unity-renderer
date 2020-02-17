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

    const bool ENABLED = false; // TODO: Remove when MOTD has been confirmed for the launch

    internal WelcomeHUDView view;
    internal Model model;

    public void Initialize(Model model)
    {
        if (!ENABLED) return;

        this.model = model;

        view = WelcomeHUDView.CreateView(model.hasWallet);
        view.Initialize(OnConfirmPressed, Close);

        Utils.UnlockCursor();
    }

    internal void Close()
    {
        SetVisibility(false);
        Utils.LockCursor();
    }

    void OnConfirmPressed()
    {
        if (model != null)
            WebInterface.ReportMotdClicked();

        Close();
    }

    public void Dispose()
    {
        if (view != null)
            Object.Destroy(view.gameObject);
    }

    public void SetVisibility(bool visible)
    {
        if (!ENABLED) return;

        view.gameObject.SetActive(visible);
    }
}
