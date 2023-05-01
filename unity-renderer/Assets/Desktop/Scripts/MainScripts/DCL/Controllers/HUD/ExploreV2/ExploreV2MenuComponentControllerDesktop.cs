using Cysharp.Threading.Tasks;
using DCL.Providers;
using Environment = DCL.Environment;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentControllerDesktop : ExploreV2MenuComponentController
{

    protected override async UniTask<IExploreV2MenuComponentView> CreateView() =>
        await Environment.i.serviceLocator
                         .Get<IAddressableResourceProvider>()
                         .Instantiate<IExploreV2MenuComponentView>("ExploreV2MenuDesktop");

}
