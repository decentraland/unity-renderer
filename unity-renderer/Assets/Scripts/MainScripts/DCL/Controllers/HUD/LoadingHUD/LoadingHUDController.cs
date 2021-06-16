using DCL;

namespace LoadingHUD
{
    public class LoadingHUDController : IHUD
    {
        internal ILoadingHUDView view;
        internal BaseVariable<bool> visible => DataStore.i.HUDs.loadingHUD.visible;
        internal BaseVariable<string> message => DataStore.i.HUDs.loadingHUD.message;
        internal BaseVariable<float> percentage => DataStore.i.HUDs.loadingHUD.percentage;
        internal BaseVariable<bool> showWalletPrompt => DataStore.i.HUDs.loadingHUD.showWalletPrompt;

        internal virtual ILoadingHUDView CreateView() => LoadingHUDView.CreateView();

        public void Initialize()
        {
            view = CreateView();

            ClearEvents();
            SetViewVisible(visible.Get());
            visible.OnChange += OnVisibleHUDChanged;
            message.OnChange += OnMessageChanged;
            percentage.OnChange += OnPercentageChanged;
            showWalletPrompt.OnChange += OnShowWalletPromptChanged;
        }

        private void OnVisibleHUDChanged(bool current, bool previous) { SetViewVisible(current); }
        private void OnMessageChanged(string current, string previous) { view?.SetMessage(current); }
        private void OnPercentageChanged(float current, float previous) { view?.SetPercentage(current / 100f); }
        private void OnShowWalletPromptChanged(bool current, bool previous) { view?.SetWalletPrompt(current); }

        public void SetVisibility(bool visible) { this.visible.Set(visible); }

        public void Dispose() { ClearEvents(); }

        internal void ClearEvents()
        {
            visible.OnChange -= OnVisibleHUDChanged;
            message.OnChange -= OnMessageChanged;
            percentage.OnChange -= OnPercentageChanged;
            showWalletPrompt.OnChange -= OnShowWalletPromptChanged;
        }

        internal void SetViewVisible(bool isVisible) { view?.SetVisible(isVisible); }
    }
}