using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Chat.HUD
{
    public class SearchChannelsWindowController : IHUD
    {
        private const int LOAD_TIMEOUT = 3;
        private const int LOAD_PAGE_SIZE = 30;

        private readonly IChatController chatController;
        private ISearchChannelsWindowView view;
        private DateTime loadStartedTimestamp = DateTime.MinValue;
        private CancellationTokenSource loadingCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource loadingMoreCancellationToken = new CancellationTokenSource();

        public ISearchChannelsWindowView View => view;

        public event Action OnClosed;
        public event Action OnBack;

        public SearchChannelsWindowController(IChatController chatController)
        {
            this.chatController = chatController;
        }

        public void Initialize(ISearchChannelsWindowView view = null)
        {
            view ??= SearchChannelsWindowComponentView.Create();
            this.view = view;
        }

        public void Dispose()
        {
            ClearListeners();
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

                ClearListeners();
                view.OnSearchUpdated += SearchChannels;
                view.OnRequestMoreChannels += LoadMoreChannels;
                view.OnBack += HandleViewBacked;
                view.OnClose += HandleViewClosed;
                view.OnJoinChannel += HandleJoinChannel;
                chatController.OnChannelUpdated += ShowChannel;
                
                chatController.GetChannels(LOAD_PAGE_SIZE, 0);
                
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

        private void SearchChannels(string searchText)
        {
            loadStartedTimestamp = DateTime.Now;
            view.ClearAllEntries();
            view.ShowLoading();
            
            if (string.IsNullOrEmpty(searchText))
                chatController.GetChannels(LOAD_PAGE_SIZE, 0);
            else
                chatController.GetChannels(LOAD_PAGE_SIZE, 0, searchText);
            
            loadingCancellationToken.Cancel();
            loadingCancellationToken = new CancellationTokenSource();
            WaitTimeoutThenHideLoading(loadingCancellationToken.Token).Forget();
        }

        private void ShowChannel(Channel channel)
        {
            if (!view.IsActive) return;
            view.HideLoading();
            view.HideLoadingMore();
            view.Set(channel);
        }

        private void LoadMoreChannels()
        {
            if (IsLoading()) return;
            loadStartedTimestamp = DateTime.Now;
            view.ShowLoadingMore();
            chatController.GetChannels(LOAD_PAGE_SIZE, view.EntryCount);
            loadingMoreCancellationToken.Cancel();
            loadingMoreCancellationToken = new CancellationTokenSource();
            WaitTimeoutThenHideLoadingMore(loadingMoreCancellationToken.Token).Forget();
        }

        private bool IsLoading() => (DateTime.Now - loadStartedTimestamp).TotalSeconds < LOAD_TIMEOUT;

        private void HandleViewClosed() => OnClosed?.Invoke();

        private void HandleViewBacked() => OnBack?.Invoke();
        
        private void ClearListeners()
        {
            chatController.OnChannelUpdated -= ShowChannel;
            view.OnSearchUpdated -= SearchChannels;
            view.OnRequestMoreChannels -= LoadMoreChannels;
            chatController.OnChannelUpdated -= ShowChannel;
            view.OnJoinChannel -= HandleJoinChannel;
        }

        private async UniTask WaitTimeoutThenHideLoading(CancellationToken cancellationToken)
        {
            await UniTask.Delay(LOAD_TIMEOUT * 1000, cancellationToken: cancellationToken);
            view.HideLoading();
        }
        
        private async UniTask WaitTimeoutThenHideLoadingMore(CancellationToken cancellationToken)
        {
            await UniTask.Delay(LOAD_TIMEOUT * 1000, cancellationToken: cancellationToken);
            view.HideLoadingMore();
        }
        
        private void HandleJoinChannel(string channelId)
        {
            chatController.JoinOrCreateChannel(channelId);
        }
    }
}