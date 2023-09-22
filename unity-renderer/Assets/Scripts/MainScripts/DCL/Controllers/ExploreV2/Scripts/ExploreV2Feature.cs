using DCL;
using DCLServices.PlacesAPIService;
using DCLServices.WorldsAPIService;

/// <summary>
/// Plugin feature that initialize the ExploreV2 feature.
/// </summary>
public class ExploreV2Feature : IPlugin
{
    public IExploreV2MenuComponentController exploreV2MenuComponentController;

    public ExploreV2Feature ()
    {
        exploreV2MenuComponentController = CreateController();
        exploreV2MenuComponentController.Initialize();
    }

    internal virtual IExploreV2MenuComponentController CreateController() => new ExploreV2MenuComponentController(Environment.i.serviceLocator.Get<IPlacesAPIService>(),Environment.i.serviceLocator.Get<IWorldsAPIService>(), new PlacesAnalytics());

    public void Dispose()
    {
        exploreV2MenuComponentController.Dispose();
    }
}
