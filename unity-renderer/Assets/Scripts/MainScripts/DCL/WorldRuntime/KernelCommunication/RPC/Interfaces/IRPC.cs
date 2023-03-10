using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using Decentraland.Social.Friendships;

namespace DCL
{
    public interface IRPC : IService
    {
        public UniTask EnsureRpc();

        public ClientEmotesKernelService Emotes();

        public ClientFriendRequestKernelService FriendRequests();

        public ClientFriendsKernelService Friends();
        public ClientSignRequestKernelService SignRequestKernelService();

        public ClientFriendshipsService Social();
    }
}
