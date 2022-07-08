using System;
using DCL.Chat.Channels;

namespace DCL.Chat.HUD
{
    public class SearchChannelsWindowController : IHUD
    {
        private const int LOAD_TIMEOUT = 3;
        private const int LOAD_PAGE_SIZE = 30;

        private readonly IChatController chatController;
        private ISearchChannelsWindowView view;
        private DateTime loadStartedTimestamp = DateTime.MinValue;

        public ISearchChannelsWindowView View => view;

        public SearchChannelsWindowController(IChatController chatController)
        {
            this.chatController = chatController;
        }

        public void Initialize(ISearchChannelsWindowView view = null)
        {
            view ??= SearchChannelsWindowComponentView.Create();
            view.OnSearchUpdated += SearchChannels;
            view.OnRequestMoreChannels += LoadMoreChannels;
            chatController.OnChannelUpdated += ShowChannel;

            this.view = view;
        }

        public void Dispose()
        {
            chatController.OnChannelUpdated -= ShowChannel;
            view.OnSearchUpdated -= SearchChannels;
            view.OnRequestMoreChannels -= LoadMoreChannels;
            view.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                view.Show();
                if (IsLoading()) return;
                loadStartedTimestamp = DateTime.Now;
                view.ClearSearchInput();
                view.ClearAllEntries();
                view.ShowLoading();
                chatController.GetChannels(LOAD_PAGE_SIZE, 0);
            }
            else
            {
                view.Hide();
            }
        }

        private void SearchChannels(string searchText)
        {
            loadStartedTimestamp = DateTime.Now;
            view.ClearAllEntries();
            view.ShowLoading();
            
            if (string.IsNullOrEmpty(searchText))
                chatController.GetChannels(LOAD_PAGE_SIZE, 0);
            else
                chatController.GetChannels(LOAD_PAGE_SIZE, 0, searchText);
        }

        private void ShowChannel(Channel channel)
        {
            if (!view.IsActive) return;
            view.HideLoading();
            view.Set(channel);
        }

        private void LoadMoreChannels()
        {
            if (IsLoading()) return;
            loadStartedTimestamp = DateTime.Now;
            view.ShowLoading();
            chatController.GetChannels(LOAD_PAGE_SIZE, view.EntryCount);
        }

        private bool IsLoading() => (DateTime.Now - loadStartedTimestamp).TotalSeconds < LOAD_TIMEOUT;
    }
}