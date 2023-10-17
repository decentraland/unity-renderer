using DCL;
using DCL.Browser;
using DCL.Interface;

namespace SignupHUD
{
    public class SignupHUDController : IHUD
    {
        private const string TOS_URL = "https://decentraland.org/terms/";
        private const string PRIVACY_POLICY_URL = "https://decentraland.org/privacy/";

        private readonly NewUserExperienceAnalytics newUserExperienceAnalytics;
        private readonly DataStore_LoadingScreen loadingScreenDataStore;
        private readonly DataStore_HUDs dataStoreHUDs;
        private readonly IBrowserBridge browserBridge;
        internal readonly ISignupHUDView view;

        internal string name;
        internal string email;
        private BaseVariable<bool> signupVisible => DataStore.i.HUDs.signupVisible;

        public SignupHUDController(IAnalytics analytics, ISignupHUDView view, DataStore_LoadingScreen loadingScreenDataStore,
            DataStore_HUDs dataStoreHUDs, IBrowserBridge browserBridge)
        {
            newUserExperienceAnalytics = new NewUserExperienceAnalytics(analytics);
            this.view = view;
            this.loadingScreenDataStore = loadingScreenDataStore;
            this.dataStoreHUDs = dataStoreHUDs;
            this.browserBridge = browserBridge;
            loadingScreenDataStore.decoupledLoadingHUD.visible.OnChange += OnLoadingScreenAppear;
        }

        public void Initialize()
        {
            if (view == null)
                return;

            signupVisible.OnChange += OnSignupVisibleChanged;

            view.OnNameScreenNext += OnNameScreenNext;
            view.OnEditAvatar += OnEditAvatar;
            view.OnTermsOfServiceAgreed += OnTermsOfServiceAgreed;
            view.OnTermsOfServiceBack += OnTermsOfServiceBack;
            view.OnLinkClicked += OnLinkClicked;

            CommonScriptableObjects.isLoadingHUDOpen.OnChange += OnLoadingScreenAppear;
            signupVisible.Set(signupVisible.Get(), true);
        }

        private void OnLoadingScreenAppear(bool current, bool previous)
        {
            if(signupVisible.Get() && current)
                signupVisible.Set(false);
        }

        private void OnSignupVisibleChanged(bool current, bool previous)
        {
            SetVisibility(current);
        }

        internal void StartSignupProcess()
        {
            name = null;
            email = null;
            view?.ShowNameScreen();
        }

        internal void OnNameScreenNext(string newName, string newEmail)
        {
            name = newName;
            email = newEmail;
            view?.ShowTermsOfServiceScreen();
        }

        internal void OnEditAvatar()
        {
            signupVisible.Set(false);
            dataStoreHUDs.avatarEditorVisible.Set(true, true);
        }

        internal void OnTermsOfServiceAgreed()
        {
            WebInterface.SendPassport(name, email);
            DataStore.i.common.isSignUpFlow.Set(false);
            newUserExperienceAnalytics?.SendTermsOfServiceAcceptedNux();
        }

        internal void OnTermsOfServiceBack() { StartSignupProcess(); }

        private void OnLinkClicked(string linkId)
        {
            switch (linkId)
            {
                case "tosUrl":
                    browserBridge.OpenUrl(TOS_URL);
                    break;
                case "privacyPolicyUrl":
                    browserBridge.OpenUrl(PRIVACY_POLICY_URL);
                    break;
            }
        }

        public void SetVisibility(bool visible)
        {
            view?.SetVisibility(visible);
            if (visible)
                StartSignupProcess();
        }

        public void Dispose()
        {
            signupVisible.OnChange -= OnSignupVisibleChanged;
            if (view == null)
                return;
            view.OnNameScreenNext -= OnNameScreenNext;
            view.OnEditAvatar -= OnEditAvatar;
            view.OnTermsOfServiceAgreed -= OnTermsOfServiceAgreed;
            view.OnTermsOfServiceBack -= OnTermsOfServiceBack;
            CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= OnLoadingScreenAppear;
            loadingScreenDataStore.decoupledLoadingHUD.visible.OnChange -= OnLoadingScreenAppear;
            view.Dispose();
        }
    }
}
