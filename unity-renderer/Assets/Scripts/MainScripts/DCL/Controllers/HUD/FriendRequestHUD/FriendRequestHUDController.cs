using Cysharp.Threading.Tasks;
using DCL.Tasks;
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
            hideCancellationToken.SafeCancelAndDispose();
            hideCancellationToken = null;
        }

        public async UniTask HideWithDelay(int delayMs = AUTOMATIC_CLOSE_DELAY, CancellationToken cancellationToken = default)
        {
            hideCancellationToken = hideCancellationToken.SafeRestartLinked(cancellationToken);

            await UniTask.Delay(delayMs, cancellationToken: hideCancellationToken.Token);

            view.Close();
        }

        public void Hide()
        {
            hideCancellationToken.SafeCancelAndDispose();
            view.Close();
        }
    }
}
