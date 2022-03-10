public class GotoPanelFeature : IPlugin
{
    public GotoPanelHUDController gotoPanelHUDController;

    public GotoPanelFeature()
    {
        gotoPanelHUDController = CreateController();
    }

    internal virtual GotoPanelHUDController CreateController() => new GotoPanelHUDController();

    public void Dispose()
    {
        gotoPanelHUDController.Dispose();
    }
}
