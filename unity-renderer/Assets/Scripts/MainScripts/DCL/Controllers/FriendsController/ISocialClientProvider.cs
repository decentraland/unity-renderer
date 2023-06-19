using Cysharp.Threading.Tasks;
using Decentraland.Social.Friendships;
using System.Threading;

namespace DCL.Social.Friends
{
    public interface ISocialClientProvider
    {
        UniTask<IClientFriendshipsService> Provide(CancellationToken cancellationToken);
    }
}
