using System;

namespace MainScripts.DCL.Controllers.HUD.ToSPopupHUD
{
    public class ToSPopupController : IDisposable
    {
        private readonly IToSPopupView view;
        private readonly BaseVariable<bool> tosPopupVisible;
        private readonly IToSPopupHandler handler;

        public  ToSPopupController(IToSPopupView view, BaseVariable<bool> tosPopupVisible,IToSPopupHandler handler)
        {
            this.view = view;
            this.view.OnAccept += HandleAccept;
            this.view.OnCancel += HandleCancel;
            this.view.OnTermsOfServiceLinkPressed += HandleViewToS;
            this.tosPopupVisible = tosPopupVisible;
            this.tosPopupVisible.OnChange += OnToSPopupVisible;
            this.handler = handler;
            OnToSPopupVisible(this.tosPopupVisible.Get(), false);
        }

        internal void OnToSPopupVisible(bool current, bool previous)
        {
            if (current)
                view.Show();
            else
                view.Hide();
        }

        internal void HandleCancel()
        {
            handler.Cancel();
        }

        internal void HandleAccept()
        {
            handler.Accept();
        }

        private void HandleViewToS()
        {
            handler.ViewToS();
        }

        public void Dispose()
        {
            view.OnAccept -= HandleAccept;
            view.OnAccept -= HandleCancel;
            view.OnAccept -= HandleViewToS;
            view.Dispose();
            tosPopupVisible.OnChange -= OnToSPopupVisible;
        }
    }
}
