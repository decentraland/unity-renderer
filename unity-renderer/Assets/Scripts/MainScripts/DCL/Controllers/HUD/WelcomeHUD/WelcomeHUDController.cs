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

    public void Initialize(Model model)
    {
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
        Object.Destroy(view.gameObject);
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }
}
