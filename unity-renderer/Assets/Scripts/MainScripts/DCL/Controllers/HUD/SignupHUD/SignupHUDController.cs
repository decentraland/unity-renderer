using DCL;
using DCL.Interface;

namespace SignupHUD
{
    public class SignupHUDController : IHUD
    {
        internal ISignupHUDView view;

        internal string name;
        internal string email;
        internal BaseVariable<bool> signupVisible => DataStore.i.HUDs.signupVisible;
        internal IHUD avatarEditorHUD;
        private readonly NewUserExperienceAnalytics newUserExperienceAnalytics;

        internal virtual ISignupHUDView CreateView() => SignupHUDView.CreateView();

        public SignupHUDController()
        {
            
        }

        public SignupHUDController(IAnalytics analytics)
        {
            newUserExperienceAnalytics = new NewUserExperienceAnalytics(analytics);
        }

        public void Initialize(IHUD avatarEditorHUD)
        {
            view = CreateView();

            if (view == null)
                return;

            this.avatarEditorHUD = avatarEditorHUD;

            signupVisible.OnChange += OnSignupVisibleChanged;
            signupVisible.Set(false);

            view.OnNameScreenNext += OnNameScreenNext;
            view.OnEditAvatar += OnEditAvatar;
            view.OnTermsOfServiceAgreed += OnTermsOfServiceAgreed;
            view.OnTermsOfServiceBack += OnTermsOfServiceBack;
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
            signupVisible.Set(false);
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
            view.Dispose();
        }
    }
}