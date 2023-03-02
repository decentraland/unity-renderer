using DCL.Helpers;
using System;

namespace DCL.Chat.HUD
{
    public class PromoteChannelsToastComponentController : IDisposable
    {
        internal const string PLAYER_PREFS_PROMOTE_CHANNELS_TOAS_DISMISSED_KEY = "PromoteChannelsToastDismissed";

        private readonly IPromoteChannelsToastComponentView view;
        private readonly IPlayerPrefs playerPrefs;
        private readonly DataStore dataStore;
        private readonly RendererState rendererState;

        private BaseVariable<bool> isPromoteToastVisible => dataStore.channels.isPromoteToastVisible;
        private BaseVariable<bool> isTutorialRunning => dataStore.common.isTutorialRunning;

        public PromoteChannelsToastComponentController(
            IPromoteChannelsToastComponentView view,
            IPlayerPrefs playerPrefs,
            DataStore dataStore,
            RendererState rendererState)
        {
            this.view = view;
            this.view.OnClose += Dismiss;
            this.view.Hide();

            this.playerPrefs = playerPrefs;

            this.dataStore = dataStore;

            this.rendererState = rendererState;
            this.rendererState.OnChange += RendererState_OnChange;
        }

        private void RendererState_OnChange(bool current, bool previous)
        {
            if (!current)
                return;

            rendererState.OnChange -= RendererState_OnChange;
            isPromoteToastVisible.OnChange += OnToastVisbile;

            if (!playerPrefs.GetBool(PLAYER_PREFS_PROMOTE_CHANNELS_TOAS_DISMISSED_KEY, false))
            {
                if (!dataStore.common.isSignUpFlow.Get())
                    isPromoteToastVisible.Set(true, notifyEvent: true);
                else
                    isTutorialRunning.OnChange += IsTutorialRunning_OnChange;
            }
            else
                isPromoteToastVisible.Set(false, notifyEvent: true);
        }

        private void IsTutorialRunning_OnChange(bool current, bool previous)
        {
            if (current)
                return;

            isTutorialRunning.OnChange -= IsTutorialRunning_OnChange;
            isPromoteToastVisible.Set(true, notifyEvent: true);
        }

        private void Dismiss()
        {
            isPromoteToastVisible.Set(false);
            playerPrefs.Set(PLAYER_PREFS_PROMOTE_CHANNELS_TOAS_DISMISSED_KEY, true);
            playerPrefs.Save();
        }

        private void OnToastVisbile(bool isVisible, bool _)
        {
            if (isVisible)
                view.Show();
            else
            {
                isPromoteToastVisible.OnChange -= OnToastVisbile;
                view.Hide();
            }
        }

        public void Dispose()
        {
            if (view != null)
            {
                view.OnClose -= Dismiss;
                view.Dispose();
            }

            rendererState.OnChange -= RendererState_OnChange;
            isPromoteToastVisible.OnChange -= OnToastVisbile;
            isTutorialRunning.OnChange -= IsTutorialRunning_OnChange;
        }
    }
}