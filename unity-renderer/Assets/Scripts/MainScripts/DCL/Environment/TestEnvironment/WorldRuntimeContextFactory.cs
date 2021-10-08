using DCL.Controllers;
using NSubstitute;

namespace DCL.Tests
{
    public static class WorldRuntimeContextFactory
    {
        public static WorldRuntimeContext CreateMocked() { return CreateWithCustomMocks(); }

        public static WorldRuntimeContext CreateWithGenericMocks(params object[] mocks)
        {
            IWorldState state = Substitute.For<IWorldState>();
            ISceneController sceneController = Substitute.For<ISceneController>();
            IPointerEventsController pointerEventsController = Substitute.For<IPointerEventsController>();
            ISceneBoundsChecker sceneBoundsChecker = Substitute.For<ISceneBoundsChecker>();
            IWorldBlockersController worldBlockersController = Substitute.For<IWorldBlockersController>();
            IRuntimeComponentFactory componentFactory = Substitute.For<IRuntimeComponentFactory>();

            foreach (var mock in mocks)
            {
                switch ( mock )
                {
                    case IWorldState customMock:
                        state = customMock;
                        break;
                    case ISceneController customMock:
                        sceneController = customMock;
                        break;
                    case IPointerEventsController customMock:
                        pointerEventsController = customMock;
                        break;
                    case ISceneBoundsChecker customMock:
                        sceneBoundsChecker = customMock;
                        break;
                    case IWorldBlockersController customMock:
                        worldBlockersController = customMock;
                        break;
                    case IRuntimeComponentFactory customMock:
                        componentFactory = customMock;
                        break;
                }
            }

            return new WorldRuntimeContext(state, sceneController, pointerEventsController, sceneBoundsChecker,
                worldBlockersController, componentFactory);
        }

        public static WorldRuntimeContext CreateWithCustomMocks(
            IWorldState state = null,
            ISceneController sceneController = null,
            IPointerEventsController pointerEventsController = null,
            ISceneBoundsChecker sceneBoundsChecker = null,
            IWorldBlockersController worldBlockersController = null,
            IRuntimeComponentFactory componentFactory = null)
        {
            return new WorldRuntimeContext(
                state: state ?? Substitute.For<IWorldState>(),
                sceneController: sceneController ?? Substitute.For<ISceneController>(),
                pointerEventsController: pointerEventsController ?? Substitute.For<IPointerEventsController>(),
                sceneBoundsChecker: sceneBoundsChecker ?? Substitute.For<ISceneBoundsChecker>(),
                blockersController: worldBlockersController ?? Substitute.For<IWorldBlockersController>(),
                componentFactory: componentFactory ?? Substitute.For<IRuntimeComponentFactory>());
        }
    }
}