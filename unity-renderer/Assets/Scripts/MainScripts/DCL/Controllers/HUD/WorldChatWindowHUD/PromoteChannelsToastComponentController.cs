using DCL.Helpers;
using System;

namespace DCL.Chat.HUD
{
    public class PromoteChannelsToastComponentController : IDisposable
    {
        internal const string PLAYER_PREFS_PROMOTE_CHANNELS_TOAS_DISMISSED_KEY = "PromoteChannelsToastDismissed";

        private readonly IPromoteChannelsToastComponentView view;
        private readonly DataStore dataStore;

        private BaseVariable<bool> isPromoteToastVisible => dataStore.channels.isPromoteToastVisible;

        public PromoteChannelsToastComponentController(
            IPromoteChannelsToastComponentView view,
            DataStore dataStore)
        {
            this.view = view;
            this.view.OnClose += Dismiss;

            this.dataStore = dataStore;
            isPromoteToastVisible.OnChange += OnToastVisbile;

            if (!PlayerPrefsUtils.GetBool(PLAYER_PREFS_PROMOTE_CHANNELS_TOAS_DISMISSED_KEY, false))
                isPromoteToastVisible.Set(true, notifyEvent: true);
            else
                isPromoteToastVisible.Set(false, notifyEvent: true);
        }

        private void Dismiss()
        {
            isPromoteToastVisible.Set(false);
            PlayerPrefsUtils.SetBool(PLAYER_PREFS_PROMOTE_CHANNELS_TOAS_DISMISSED_KEY, true);
            PlayerPrefsUtils.Save();
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

            isPromoteToastVisible.OnChange -= OnToastVisbile;
        }
    }
}