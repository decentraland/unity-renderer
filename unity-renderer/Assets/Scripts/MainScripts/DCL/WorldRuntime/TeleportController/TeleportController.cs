using DCL;
using DCL.Interface;

public class TeleportController : ITeleportController
{

    public void Teleport(int x, int y)
    {
        DataStore.i.HUDs.loadingHUD.invokedTeleport.Set(true);
        WebInterface.GoTo(x, y);
    }
    public void JumpIn(int coordsX, int coordsY, string serverName, string layerName)
    {
        DataStore.i.HUDs.loadingHUD.invokedTeleport.Set(true);
        WebInterface.JumpIn(coordsX, coordsY, serverName, layerName);
    }

    public void Dispose() { throw new System.NotImplementedException(); }
    public void Initialize() { throw new System.NotImplementedException(); }
}
