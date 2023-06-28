using Cysharp.Threading.Tasks;
using Decentraland.Social.Friendships;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public interface ISocialClientProvider
    {
        public event Func<UniTask> OnTransportError;

        UniTask<IClientFriendshipsService> Provide(CancellationToken cancellationToken);
    }
}
