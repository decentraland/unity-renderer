using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public class FriendRequestHUDController
    {
        private const int AUTOMATIC_CLOSE_DELAY = 2000;

        private readonly IFriendRequestHUDView view;
        private CancellationTokenSource hideCancellationToken = new ();

        public FriendRequestHUDController(IFriendRequestHUDView view)
        {
            this.view = view;
        }

        public void Dispose()
        {
            hideCancellationToken.Cancel();
            hideCancellationToken.Dispose();
            hideCancellationToken = null;
        }

        public async UniTask HideWithDelay(int delayMs = AUTOMATIC_CLOSE_DELAY, CancellationToken cancellationToken = default)
        {
            try
            {
                hideCancellationToken.Cancel();
                hideCancellationToken.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // the view has already been hidden before, so ignore the exception
            }

            hideCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await UniTask.Delay(delayMs, cancellationToken: hideCancellationToken.Token);

            view.Close();
        }

        public void Hide()
        {
            hideCancellationToken.Cancel();
            hideCancellationToken.Dispose();
            view.Close();
        }
    }
}
