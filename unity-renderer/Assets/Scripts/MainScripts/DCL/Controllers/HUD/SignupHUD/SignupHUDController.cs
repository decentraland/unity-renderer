using DCL;
using DCL.Interface;

namespace SignupHUD
{
    public class SignupHUDController : IHUD
    {
        private ISignupHUDView view;

        private string name;
        private string email;
        private BaseVariable<bool> signupVisible => DataStore.i.HUDs.signupVisible;

        protected virtual ISignupHUDView CreateView() => SignupHUDView.CreateView();

        public void Initialize()
        {
            view = CreateView();
            if (view == null)
                return;

            signupVisible.OnChange += OnSignupVisibleChanged;
            signupVisible.Set(false);

            view.OnNameScreenNext += OnNameScreenNext;
            view.OnEditAvatar += OnEditAvatar;
            view.OnTermsOfServiceAgreed += OnTermsOfServiceAgreed;
            view.OnTermsOfServiceBack += OnTermsOfServiceBack;

        }
        private void OnSignupVisibleChanged(bool current, bool previous) { SetVisibility(current); }

        public void StartSignupProcess()
        {
            name = null;
            email = null;
            signupVisible.Set(true);
            view?.ShowNameScreen();
        }

        private void OnNameScreenNext(string newName, string newEmail)
        {
            name = newName;
            email = newEmail;
            view?.ShowTermsOfServiceScreen();
        }

        private void OnEditAvatar()
        {
            signupVisible.Set(false);
            //TODO: Open Avatar Editor
        }

        private void OnTermsOfServiceAgreed()
        {
            WebInterface.SendPassport(name, email);
            signupVisible.Set(false);
        }

        private void OnTermsOfServiceBack() { StartSignupProcess(); }

        public void SetVisibility(bool visible) { view?.SetVisibility(visible); }

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