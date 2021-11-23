/// <summary>
/// Plugin feature that initialize the ExploreV2 feature.
/// </summary>
public class ExploreV2Feature : IPlugin
{
    public IExploreV2MenuComponentController exploreV2MenuComponentController;

    public void Initialize()
    {
        exploreV2MenuComponentController = CreateController();
        exploreV2MenuComponentController.Initialize();
    }

    public void OnGUI()
    {
    }

    public void Update()
    {
    }

    public void LateUpdate()
    {
    }

    internal virtual IExploreV2MenuComponentController CreateController() => new ExploreV2MenuComponentController();

    public void Dispose()
    {
    }
}