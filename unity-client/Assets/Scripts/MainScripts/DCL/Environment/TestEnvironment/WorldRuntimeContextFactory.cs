using DCL.Controllers;
using NSubstitute;

namespace DCL.Tests
{
    public static class WorldRuntimeContextFactory
    {
        public static WorldRuntimeContext CreateMocked()
        {
            return CreateWithCustomMocks();
        }

        public static WorldRuntimeContext CreateWithCustomMocks(
            IWorldState state = null,
            ISceneController sceneController = null,
            IPointerEventsController pointerEventsController = null,
            ISceneBoundsChecker sceneBoundsChecker = null,
            IWorldBlockersController worldBlockersController = null)
        {
            return new WorldRuntimeContext(
                state: state ?? Substitute.For<IWorldState>(),
                sceneController: sceneController ?? Substitute.For<ISceneController>(),
                pointerEventsController: pointerEventsController ?? Substitute.For<IPointerEventsController>(),
                sceneBoundsChecker: sceneBoundsChecker ?? Substitute.For<ISceneBoundsChecker>(),
                blockersController: worldBlockersController ?? Substitute.For<IWorldBlockersController>());
        }
    }
}