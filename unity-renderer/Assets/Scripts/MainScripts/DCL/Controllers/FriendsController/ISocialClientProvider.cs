using Cysharp.Threading.Tasks;
using Decentraland.Social.Friendships;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public interface ISocialClientProvider
    {
        public event Action OnTransportError;

        UniTask<IClientFriendshipsService> Provide(CancellationToken cancellationToken);
    }
}
