/// <summary>
/// Plugin feature that initialize the ExploreV2 feature.
/// </summary>
public class ExploreV2Feature : PluginFeature
{
    public ExploreV2MenuComponentController exploreV2MenuComponentController;

    public override void Initialize()
    {
        base.Initialize();

        exploreV2MenuComponentController = new ExploreV2MenuComponentController();
        exploreV2MenuComponentController.Initialize();
    }
}