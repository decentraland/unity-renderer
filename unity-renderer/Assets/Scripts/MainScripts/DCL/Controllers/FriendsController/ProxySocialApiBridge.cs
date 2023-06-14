using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public class ProxySocialApiBridge : ISocialApiBridge
    {
        private readonly RPCSocialApiBridge socialApiBridge;
        private readonly DataStore dataStore;

        private CancellationTokenSource lifeCycleCancellationToken = new ();
        private UniTaskCompletionSource featureFlagsInitializedTask;

        private FeatureFlag featureFlags => dataStore.featureFlags.flags.Get();

        private bool useSocialApiBridge => featureFlags.IsFeatureEnabled("use-social-client");

        public event Action<FriendRequest> OnIncomingFriendRequestAdded
        {
            add => socialApiBridge.OnIncomingFriendRequestAdded += value;
            remove => socialApiBridge.OnIncomingFriendRequestAdded -= value;
        }

        public event Action<FriendRequest> OnOutgoingFriendRequestAdded
        {
            add => socialApiBridge.OnOutgoingFriendRequestAdded += value;
            remove => socialApiBridge.OnOutgoingFriendRequestAdded -= value;
        }

        public event Action<string> OnFriendRequestAccepted
        {
            add => socialApiBridge.OnFriendRequestAccepted += value;
            remove => socialApiBridge.OnFriendRequestAccepted -= value;
        }

        public event Action<string> OnFriendRequestRejected
        {
            add => socialApiBridge.OnFriendRequestRejected += value;
            remove => socialApiBridge.OnFriendRequestRejected -= value;
        }

        public event Action<string> OnFriendRequestCanceled
        {
            add => socialApiBridge.OnFriendRequestCanceled += value;
            remove => socialApiBridge.OnFriendRequestCanceled -= value;
        }

        public event Action<string> OnDeletedByFriend
        {
            add => socialApiBridge.OnDeletedByFriend += value;
            remove => socialApiBridge.OnDeletedByFriend -= value;
        }

        public ProxySocialApiBridge(RPCSocialApiBridge socialApiBridge,
            DataStore dataStore)
        {
            this.socialApiBridge = socialApiBridge;
            this.dataStore = dataStore;
        }

        public void Dispose()
        {
            lifeCycleCancellationToken.SafeCancelAndDispose();

            if (useSocialApiBridge)
                socialApiBridge.Dispose();
        }

        public void Initialize()
        {
            lifeCycleCancellationToken = lifeCycleCancellationToken.SafeRestart();

            void InitializeIfEnabled()
            {
                if (useSocialApiBridge)
                    socialApiBridge.Initialize();
            }

            if (featureFlags.IsInitialized)
                InitializeIfEnabled();
            else
                WaitForFeatureFlagsToBeInitialized(lifeCycleCancellationToken.Token)
                   .ContinueWith(InitializeIfEnabled)
                   .Forget();
        }

        public UniTask<AllFriendsInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default)
        {
            if (useSocialApiBridge)
                return socialApiBridge.GetInitializationInformationAsync(cancellationToken);

            return UniTask.Never<AllFriendsInitializationMessage>(cancellationToken);
        }

        public UniTask RejectFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            if (useSocialApiBridge)
                return socialApiBridge.RejectFriendshipAsync(friendId, cancellationToken);

            return UniTask.Never(cancellationToken);
        }

        public UniTask<FriendRequest> RequestFriendshipAsync(string friendId, string messageBody, CancellationToken cancellationToken = default)
        {
            if (useSocialApiBridge)
                return socialApiBridge.RequestFriendshipAsync(friendId, messageBody, cancellationToken);

            return UniTask.Never<FriendRequest>(cancellationToken);
        }

        public UniTask CancelFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            if (useSocialApiBridge)
                return socialApiBridge.CancelFriendshipAsync(friendId, cancellationToken);

            return UniTask.Never(cancellationToken);
        }

        public UniTask AcceptFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            if (useSocialApiBridge)
                return socialApiBridge.AcceptFriendshipAsync(friendId, cancellationToken);

            return UniTask.Never(cancellationToken);
        }

        public UniTask DeleteFriendshipAsync(string friendId, CancellationToken cancellationToken = default)
        {
            if (useSocialApiBridge)
                return socialApiBridge.DeleteFriendshipAsync(friendId, cancellationToken);

            return UniTask.Never(cancellationToken);
        }

        private async UniTask WaitForFeatureFlagsToBeInitialized(CancellationToken cancellationToken)
        {
            if (featureFlagsInitializedTask == null)
            {
                featureFlagsInitializedTask = new UniTaskCompletionSource();

                void CompleteTaskAndUnsubscribe(FeatureFlag current, FeatureFlag previous)
                {
                    dataStore.featureFlags.flags.OnChange -= CompleteTaskAndUnsubscribe;
                    featureFlagsInitializedTask.TrySetResult();
                }

                dataStore.featureFlags.flags.OnChange += CompleteTaskAndUnsubscribe;
            }

            await featureFlagsInitializedTask.Task.AttachExternalCancellation(cancellationToken);
            await UniTask.NextFrame(cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);
        }
    }
}
