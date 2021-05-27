using System;

namespace SignupHUD
{
    public class SignupHUDController : IHUD
    {
        private ISignupHUDView view;

        private string name;
        private string email;

        protected virtual ISignupHUDView CreateView() => SignupHUDView.CreateView();

        public void Initialize()
        {
            view = CreateView();
            if (view != null)
            {
                view.OnNameScreenNext += OnNameScreenNext;
                view.OnTermsOfServiceAgreed += TermsOfServiceAgreed;
                view.OnTermsOfServiceBack += TermsOfServiceBack;
            }
        }

        public void StartSignupProcess()
        {
            name = null;
            email = null;
            view?.SetVisibility(true);
            view?.ShowNameScreen();
        }

        private void OnNameScreenNext(string newName, string newEmail)
        {
            name = newName;
            email = newEmail;
            view?.ShowTermsOfServiceScreen();
        }

        private void TermsOfServiceAgreed()
        {
            //Send data to kernel
            view?.SetVisibility(false);
        }

        private void TermsOfServiceBack() { StartSignupProcess(); }

        public void SetVisibility(bool visible) { throw new System.NotImplementedException(); }

        public void Dispose()
        {
            if (view == null)
                return;
            view.Dispose();
            view.OnNameScreenNext -= OnNameScreenNext;
            view.OnTermsOfServiceAgreed -= TermsOfServiceAgreed;
        }
    }

    public interface ISignupHUDView : IDisposable
    {
        delegate void NameScreenDone(string newName, string newEmail);
        event NameScreenDone OnNameScreenNext;
        event Action OnTermsOfServiceAgreed;
        event Action OnTermsOfServiceBack;

        void SetVisibility(bool visible);
        void ShowNameScreen();
        void ShowTermsOfServiceScreen();
    }

}