namespace DCL.Controllers
{
    public interface IOutOfSceneBoundariesHandler
    {
        void UpdateOutOfBoundariesState(bool isInsideBoundaries);
    }
}