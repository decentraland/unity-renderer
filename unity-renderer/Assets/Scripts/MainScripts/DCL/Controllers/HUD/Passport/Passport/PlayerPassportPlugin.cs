using DCL;

public class PlayerPassportPlugin : IPlugin
{

    public PlayerPassportPlugin()
    {
        DataStore.i.HUDs.enableNewPassport.Set(true);
    }

    public void Dispose()
    {

    }
}
