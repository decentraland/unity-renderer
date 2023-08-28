using DCL;

public interface ITeleportController : IService
{
    void Teleport(int x, int y);
    void JumpInWorld(string worldName);
    void JumpIn(int coordsX, int coordsY, string serverName, string layerName = null);
    void GoToCrowd();
    void GoToMagic();
}
