using DCL;
using DCLServices.PlacesAPIService;

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

    internal virtual IExploreV2MenuComponentController CreateController() => new ExploreV2MenuComponentController(Environment.i.serviceLocator.Get<IPlacesAPIService>());

    public void Dispose()
    {
        exploreV2MenuComponentController.Dispose();
    }
}
