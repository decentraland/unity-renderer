using DCL.Social.Chat;
using System;

namespace DCL.Social.Chat
{
    public class ChannelLimitReachedWindowController : IDisposable
    {
        private readonly IChannelLimitReachedWindowView view;
        private readonly DataStore dataStore;

        public ChannelLimitReachedWindowController(IChannelLimitReachedWindowView view,
            DataStore dataStore)
        {
            this.view = view;
            view.Hide();
            view.OnClose += Close;
            this.dataStore = dataStore;
            dataStore.channels.currentChannelLimitReached.OnChange += Show;
        }

        public void Dispose()
        {
            dataStore.channels.currentChannelLimitReached.OnChange -= Show;

            if (view != null)
            {
                view.OnClose -= Close;
                view.Dispose();
            }
        }

        private void Show(string currentChannelId, string previousChannelId)
        {
            if (string.IsNullOrEmpty(currentChannelId)) return;
            view.Show();
        }

        private void Close() => view.Hide();
    }
}
