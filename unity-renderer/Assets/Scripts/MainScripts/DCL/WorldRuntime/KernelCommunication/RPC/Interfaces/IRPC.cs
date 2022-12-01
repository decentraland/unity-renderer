using Cysharp.Threading.Tasks;

namespace DCL
{
    public interface IRPC : IService
    {
        public UniTask EnsureRpc();

        public ClientEmotesKernelService Emotes();

        public ClientAnalyticsKernelService Analytics();
    }
}
