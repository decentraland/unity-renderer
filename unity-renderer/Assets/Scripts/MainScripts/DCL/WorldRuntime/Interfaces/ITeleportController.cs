using DCL;

public interface ITeleportController : IService
{

    public void Teleport(int x, int y);

    void JumpIn(int coordsX, int coordsY, string serverName, string layerName);
}
