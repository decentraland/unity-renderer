using Cysharp.Threading.Tasks;

namespace DCL
{
    public interface IHUDController : IService
    {
        UniTask ConfigureHUDElement(HUDElementID hudElementId, HUDConfiguration configuration, string extraPayload = null);
        IHUD GetHUDElement(HUDElementID id);
        void Cleanup();
    }
}
