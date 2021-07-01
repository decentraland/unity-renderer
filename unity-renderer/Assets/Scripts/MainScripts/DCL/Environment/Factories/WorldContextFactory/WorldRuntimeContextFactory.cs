using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    public static class WorldRuntimeContextFactory
    {
        public static WorldRuntimeContext CreateDefault()
        {
            return CreateDefault(null);
        }
        public static WorldRuntimeContext CreateDefault(IPoolableComponentFactory poolableComponentFactory)
        {
            return new WorldRuntimeContext(
                state: new WorldState(),
                sceneController: new SceneController(),
                pointerEventsController: new PointerEventsController(),
                sceneBoundsChecker: new SceneBoundsChecker(),
                blockersController: new WorldBlockersController(),
                componentFactory: new RuntimeComponentFactory(poolableComponentFactory));
        }
    }
}