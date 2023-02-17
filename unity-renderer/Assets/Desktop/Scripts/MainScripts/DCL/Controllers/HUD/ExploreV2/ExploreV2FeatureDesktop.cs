/// <summary>
/// Plugin feature that initialize the ExploreV2 Desktop feature.
/// </summary>
public class ExploreV2FeatureDesktop : IPlugin
{
    public IExploreV2MenuComponentController exploreV2MenuComponentController;

    public ExploreV2FeatureDesktop()
    {
        exploreV2MenuComponentController = CreateController();
        exploreV2MenuComponentController.Initialize();
    }

    internal virtual IExploreV2MenuComponentController CreateController() =>
        new ExploreV2MenuComponentControllerDesktop();

    public void Dispose()
    {
        exploreV2MenuComponentController.Dispose();
    }
}
