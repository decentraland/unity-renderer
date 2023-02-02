using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;

namespace DCL
{
    public interface IRPC : IService
    {
        public UniTask EnsureRpc();

        public ClientEmotesKernelService Emotes();

        public ClientFriendRequestKernelService FriendRequests();

        public ClientAnalyticsKernelService Analytics();

        public ClientFriendsKernelService Friends();
    }
}
