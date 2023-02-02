using DCL;
using DCL.Interface;
using JetBrains.Annotations;

namespace SignupHUD
{
    public class SignupHUDController : IHUD
    {
        internal readonly ISignupHUDView view;
        private readonly NewUserExperienceAnalytics newUserExperienceAnalytics;
        private readonly DataStore_LoadingScreen loadingScreenDataStore;

        internal string name;
        internal string email;
        internal BaseVariable<bool> signupVisible => DataStore.i.HUDs.signupVisible;
        internal IHUD avatarEditorHUD;

        [UsedImplicitly]
        public SignupHUDController() { }

        public SignupHUDController(IAnalytics analytics, ISignupHUDView view, DataStore_LoadingScreen loadingScreenDataStore)
        {
            newUserExperienceAnalytics = new NewUserExperienceAnalytics(analytics);
            this.view = view;
            this.loadingScreenDataStore = loadingScreenDataStore;
            loadingScreenDataStore.decoupledLoadingHUD.visible.OnChange += OnLoadingScreenAppear;
        }

        public void Initialize(IHUD avatarEditorHUD)
        {
            if (view == null)
                return;

            this.avatarEditorHUD = avatarEditorHUD;

            signupVisible.OnChange += OnSignupVisibleChanged;
            signupVisible.Set(false);

            view.OnNameScreenNext += OnNameScreenNext;
            view.OnEditAvatar += OnEditAvatar;
            view.OnTermsOfServiceAgreed += OnTermsOfServiceAgreed;
            view.OnTermsOfServiceBack += OnTermsOfServiceBack;

            CommonScriptableObjects.isLoadingHUDOpen.OnChange += OnLoadingScreenAppear;
        }
        private void OnLoadingScreenAppear(bool current, bool previous)
        {
            if(signupVisible.Get() && current)
                signupVisible.Set(false);
        }

        private void OnSignupVisibleChanged(bool current, bool previous) { SetVisibility(current); }

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
            avatarEditorHUD?.SetVisibility(true);
        }

        internal void OnTermsOfServiceAgreed()
        {
            WebInterface.SendPassport(name, email);
            DataStore.i.common.isSignUpFlow.Set(false);
            newUserExperienceAnalytics?.SendTermsOfServiceAcceptedNux();
        }

        internal void OnTermsOfServiceBack() { StartSignupProcess(); }

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
