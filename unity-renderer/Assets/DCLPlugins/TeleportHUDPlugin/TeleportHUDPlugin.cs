using DCL;
using DCL.Map;

public class TeleportHUDPlugin : IPlugin
{
    public TeleportHUDPlugin()
    {
        new TeleportPromptHUDController(DataStore.i, WebInterfaceMinimapApiBridge.i); 
    }

    public void Dispose() { }
}
