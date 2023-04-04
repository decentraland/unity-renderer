using DCL;
using DCL.Map;

public class TeleportHUDPlugin : IPlugin
{
    private readonly TeleportPromptHUDController teleportPromptHUDController;

    public TeleportHUDPlugin()
    {
        teleportPromptHUDController = new TeleportPromptHUDController(DataStore.i, WebInterfaceMinimapApiBridge.i);
    }

    public void Dispose()
    {
        teleportPromptHUDController.Dispose();
    }
}
