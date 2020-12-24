using DCL.Controllers;

namespace DCL
{
    public static class WorldRuntimeContextFactory
    {
        public static WorldRuntimeContext CreateDefault()
        {
            return new WorldRuntimeContext(
                state: new WorldState(),
                sceneController: new SceneController(),
                pointerEventsController: new PointerEventsController(),
                sceneBoundsChecker: new SceneBoundsChecker(),
                blockersController: new WorldBlockersController());
        }
    }
}