using DCL;
using DCL.Map;

public class TeleportHUDPlugin : IPlugin
{
    private readonly TeleportPromptHUDController teleportPromptHUDController;

    public TeleportHUDPlugin()
    {
        teleportPromptHUDController = new TeleportPromptHUDController(DataStore.i,
            WebInterfaceMinimapApiBridge.i,
            DataStore.i.rpc.context.restrictedActions,
            Environment.i.world.teleportController);
    }

    public void Dispose()
    {
        teleportPromptHUDController.Dispose();
    }
}
