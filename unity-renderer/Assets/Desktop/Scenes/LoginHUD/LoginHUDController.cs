using System;
using System.Collections;
using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Login
{
    public class LoginHUDController : IDisposable
    {
        internal ILoginHUDView view;

        internal virtual ILoginHUDView CreateView() =>
            LoginHUDView.CreateView();

        public void Initialize()
        {
            view = CreateView();

            if (view == null)
                return;

            view.OnPlay += OnPlay;
            view.OnPlayAsGuest += OnPlayAsGuest;
        }

        private void OnPlay()
        {
            WebInterface.SendAuthentication(WebInterface.RendererAuthenticationType.Guest);
            ChangeMainScene();
        }

        private void OnPlayAsGuest()
        {
            WebInterface.SendAuthentication(WebInterface.RendererAuthenticationType.Guest);
            ChangeMainScene();
        }

        private void ChangeMainScene()
        {
            view.SetLoading(true);
            SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
        }

        public void Dispose()
        {
            if (view == null)
                return;

            view.OnPlay -= OnPlay;
            view.OnPlayAsGuest -= OnPlayAsGuest;
            view.Dispose();
        }
    }
}
