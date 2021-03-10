using DCL.Interface;
using System;

public class TermsOfServiceHUDController : IHUD
{
    [Serializable]
    public class Model
    {
        public string sceneId;
        public string sceneName;
        public bool adultContent;
        public bool gamblingContent;
        public string tosURL;
        public string privacyPolicyURL;
        public string emailContactURL;
    }

    internal TermsOfServiceHUDView view;
    internal Model model;

    public TermsOfServiceHUDController()
    {
        view = TermsOfServiceHUDView.CreateView();
        view.Initialize(SendAgreed, SendDeclined, OpenToS, OpenPrivacyPolicy, OpenContactEmail);
    }


    public void ShowTermsOfService(Model model)
    {
        this.model = model;

        if (this.model == null)
        {
            view.SetVisible(false);
            return;
        }

        view.SetData(model.sceneName, model.adultContent, model.gamblingContent, !string.IsNullOrEmpty(model.tosURL), !string.IsNullOrEmpty(model.privacyPolicyURL), !string.IsNullOrEmpty(model.emailContactURL));
        view.SetVisible(true);
    }

    private void SendAgreed(bool dontShowAgain)
    {
        WebInterface.SendTermsOfServiceResponse(model.sceneId, true, dontShowAgain);
        view.SetVisible(false);
    }

    private void SendDeclined(bool dontShowAgain)
    {
        WebInterface.SendTermsOfServiceResponse(model.sceneId, false, dontShowAgain);
        view.SetVisible(false);
    }

    private void OpenToS()
    {
        if (!string.IsNullOrEmpty(model.tosURL))
            WebInterface.OpenURL(model.tosURL);
    }

    private void OpenPrivacyPolicy()
    {
        if (!string.IsNullOrEmpty(model.privacyPolicyURL))
            WebInterface.OpenURL(model.privacyPolicyURL);
    }

    private void OpenContactEmail()
    {
        if (!string.IsNullOrEmpty(model.emailContactURL))
            WebInterface.OpenURL($"mailto:{model.emailContactURL}");
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }

    public void Dispose()
    {
        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }
}
