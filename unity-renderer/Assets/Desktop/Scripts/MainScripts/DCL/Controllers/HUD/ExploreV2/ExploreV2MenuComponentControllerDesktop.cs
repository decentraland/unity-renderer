using DCLServices.PlacesAPIService;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentControllerDesktop : ExploreV2MenuComponentController
{
    protected override IExploreV2MenuComponentView CreateView() =>
        ExploreV2MenuComponentViewDesktop.Create();

    public ExploreV2MenuComponentControllerDesktop(IPlacesAPIService placesAPIService, IPlacesAnalytics placesAnalytics) : base(placesAPIService, placesAnalytics) {}
}
