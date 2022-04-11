public class GotoPanelPlugin : IPlugin
{
    public GotoPanel.GotoPanelHUDController gotoPanelHUDController;

    public GotoPanelPlugin()
    {
        gotoPanelHUDController = CreateController();
        gotoPanelHUDController.Initialize();
    }

    internal virtual GotoPanel.GotoPanelHUDController CreateController() => new GotoPanel.GotoPanelHUDController();

    public void Dispose()
    {
        gotoPanelHUDController.Dispose();
    }
}
