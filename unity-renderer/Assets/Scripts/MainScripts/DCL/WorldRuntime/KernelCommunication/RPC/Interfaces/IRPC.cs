using Cysharp.Threading.Tasks;

namespace DCL
{
    public interface IRPC : IService
    {
        public ClientEmotesKernelService emotes { get; internal set; }

        public UniTask EnsureRpc();
    }
}
