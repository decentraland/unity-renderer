using Cysharp.Threading.Tasks;
using System.Threading;

namespace DCL
{
    public interface IHUDController : IService
    {
        UniTask ConfigureHUDElement(HUDElementID hudElementId, HUDConfiguration configuration, CancellationToken cancellationToken = default, string extraPayload = null);
        IHUD GetHUDElement(HUDElementID id);
        void Cleanup();
    }
}
