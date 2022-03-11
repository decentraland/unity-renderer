using DCL;

    public class LoadingHUDController : IHUD
    {
        internal LoadingHUDView view;
        internal BaseVariable<bool> visible => DataStore.i.HUDs.loadingHUD.visible;
        internal BaseVariable<string> message => DataStore.i.HUDs.loadingHUD.message;
        internal BaseVariable<float> percentage => DataStore.i.HUDs.loadingHUD.percentage;
        internal BaseVariable<bool> showTips => DataStore.i.HUDs.loadingHUD.showTips;

        protected internal virtual LoadingHUDView CreateView() { return LoadingHUDView.CreateView(); }

        public void Initialize()
        {
            view = CreateView();

            ClearEvents();

            SetViewVisible(visible.Get());
            view?.SetMessage(message.Get());
            view?.SetPercentage(percentage.Get() / 100f);
            view?.SetTips(showTips.Get());

            // set initial states to prevent reconciliation errors
            visible.OnChange += OnVisibleHUDChanged;
            message.OnChange += OnMessageChanged;
            percentage.OnChange += OnPercentageChanged;
            showTips.OnChange += OnShowTipsChanged;
        }

        private void OnVisibleHUDChanged(bool current, bool previous) { SetViewVisible(current); }
        private void OnMessageChanged(string current, string previous) { view?.SetMessage(current); }
        private void OnPercentageChanged(float current, float previous) { view?.SetPercentage(current / 100f); }
        private void OnShowTipsChanged(bool current, bool previous) { view?.SetTips(current); }

        public void SetVisibility(bool visible) { this.visible.Set(visible); }

        public void Dispose()
        {
            ClearEvents();
            view?.Dispose();
        }

        internal void ClearEvents()
        {
            visible.OnChange -= OnVisibleHUDChanged;
            message.OnChange -= OnMessageChanged;
            percentage.OnChange -= OnPercentageChanged;
            showTips.OnChange -= OnShowTipsChanged;
        }

        internal void SetViewVisible(bool isVisible) { view?.SetVisible(isVisible); }
    }