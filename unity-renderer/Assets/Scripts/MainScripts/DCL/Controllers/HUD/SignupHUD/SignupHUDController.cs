using System;
using UnityEngine;

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
                view.OnNameScreenDone += OnNameScreenDone;
                view.OnTermsOfServiceScreenDone += OnTermsOfServiceScreenDone;
            }
        }

        public void StartSignupProcess()
        {
            name = null;
            email = null;
            view?.ShowNameScreen();
        }

        private void OnNameScreenDone(string newName, string newemail)
        {
            name = newName;
            email = newemail;
            view?.ShowTermsOfServiceScreen();
        }

        private void OnTermsOfServiceScreenDone()
        {
            //Send data to kernel
            view?.HideEverything();
        }

        public void SetVisibility(bool visible) { throw new System.NotImplementedException(); }
        public void Dispose()
        {
            if (view == null)
                return;
            view.Dispose();
            view.OnNameScreenDone += OnNameScreenDone;
            view.OnTermsOfServiceScreenDone += OnTermsOfServiceScreenDone;
        }
    }

    public interface ISignupHUDView : IDisposable
    {
        delegate void NameScreenDone(string newName, string newEmail);
        event NameScreenDone OnNameScreenDone;
        event Action OnTermsOfServiceScreenDone;

        void HideEverything();
        void ShowNameScreen();
        void ShowTermsOfServiceScreen();
    }

    public class SignupHUDView : MonoBehaviour, ISignupHUDView
    {
        public event ISignupHUDView.NameScreenDone OnNameScreenDone;
        public event Action OnTermsOfServiceScreenDone;

        public static SignupHUDView CreateView() { return null; }

        public void HideEverything() { throw new NotImplementedException(); }
        public void ShowNameScreen() { throw new NotImplementedException(); }
        public void ShowTermsOfServiceScreen() { throw new NotImplementedException(); }
        public void Dispose()
        {
            if (this != null)
                Destroy(gameObject);
        }
    }
}