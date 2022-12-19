using DCL;
using static Decentraland.Bff.AboutResponse.Types;

namespace WorldsFeaturesAnalytics
{
    public interface IWorldsAnalytics : IService
    {
        void OnEnteredRealm(AboutConfiguration current, AboutConfiguration _);
    }
}
