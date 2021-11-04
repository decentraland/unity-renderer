/// <summary>
/// Plugin feature that initialize the ExploreV2 feature.
/// </summary>
public class ExploreV2Feature : PluginFeature
{
    public IExploreV2MenuComponentController exploreV2MenuComponentController;

    public override void Initialize()
    {
        base.Initialize();

        exploreV2MenuComponentController = CreateController();
        exploreV2MenuComponentController.Initialize();
    }

    internal virtual IExploreV2MenuComponentController CreateController() => new ExploreV2MenuComponentController();
}