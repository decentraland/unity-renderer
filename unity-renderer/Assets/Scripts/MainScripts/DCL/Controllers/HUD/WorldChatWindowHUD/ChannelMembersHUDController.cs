using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL.Chat.HUD
{
    public class ChannelMembersHUDController : IDisposable
    {
        private const int LOAD_TIMEOUT = 2;
        internal const int LOAD_PAGE_SIZE = 30;

        private readonly IChatController chatController;
        private IChannelMembersComponentView view;
        private DateTime loadStartedTimestamp = DateTime.MinValue;
        private CancellationTokenSource loadingCancellationToken = new CancellationTokenSource();
        private string currentChannelId;

        public IChannelMembersComponentView View => view;

        public ChannelMembersHUDController(IChannelMembersComponentView view, IChatController chatController)
        {
            this.view = view;
            this.chatController = chatController;
        }

        public void SetChannelId(string channelId) => currentChannelId = channelId;

        public void Dispose()
        {
            ClearListeners();
            view.Dispose();
            loadingCancellationToken.Cancel();
            loadingCancellationToken.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                ClearListeners();

                view.ClearSearchInput();

                view.OnSearchUpdated += SearchMembers;
                view.OnRequestMoreMembers += LoadMoreMembers;

                view.Show();
                view.ClearAllEntries();
                view.ShowLoading();

                loadStartedTimestamp = DateTime.Now;
                chatController.GetChannelMembers(currentChannelId, LOAD_PAGE_SIZE, 0);

                loadingCancellationToken.Cancel();
                loadingCancellationToken = new CancellationTokenSource();
                WaitTimeoutThenHideLoading(loadingCancellationToken.Token).Forget();
            }
            else
            {
                ClearListeners();
                view.Hide();
            }
        }

        private void SearchMembers(string searchText)
        {
            loadStartedTimestamp = DateTime.Now;
            view.ClearAllEntries();
            view.HideLoadingMore();
            view.ShowLoading();

            if (string.IsNullOrEmpty(searchText))
                chatController.GetChannelMembers(currentChannelId, LOAD_PAGE_SIZE, 0);
            else
                chatController.GetChannelMembers(currentChannelId, LOAD_PAGE_SIZE, 0, searchText);

            loadingCancellationToken.Cancel();
            loadingCancellationToken = new CancellationTokenSource();
            WaitTimeoutThenHideLoading(loadingCancellationToken.Token).Forget();
        }

        private void LoadMoreMembers()
        {
            if (IsLoading()) return;
            loadStartedTimestamp = DateTime.Now;
            view.HideLoadingMore();
            chatController.GetChannelMembers(currentChannelId, LOAD_PAGE_SIZE, view.EntryCount);
        }

        private bool IsLoading() => (DateTime.Now - loadStartedTimestamp).TotalSeconds < LOAD_TIMEOUT;

        private void ClearListeners()
        {
            view.OnSearchUpdated -= SearchMembers;
            view.OnRequestMoreMembers -= LoadMoreMembers;
        }

        private async UniTask WaitTimeoutThenHideLoading(CancellationToken cancellationToken)
        {
            await UniTask.Delay(LOAD_TIMEOUT * 1000, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            view.HideLoading();
        }
    }
}