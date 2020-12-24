using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Context related to all the systems involved in the execution of decentraland scenes.
    /// </summary>
    public class WorldRuntimeContext : System.IDisposable
    {
        public readonly WorldState state;
        public readonly SceneController sceneController;
        public readonly PointerEventsController pointerEventsController;
        public readonly SceneBoundsChecker sceneBoundsChecker;
        public readonly WorldBlockersController blockersController;

        public WorldRuntimeContext(WorldState state,
            SceneController sceneController,
            PointerEventsController pointerEventsController,
            SceneBoundsChecker sceneBoundsChecker,
            WorldBlockersController blockersController)
        {
            this.state = state;
            this.sceneController = sceneController;
            this.pointerEventsController = pointerEventsController;
            this.sceneBoundsChecker = sceneBoundsChecker;
            this.blockersController = blockersController;
        }

        public void Dispose()
        {
            pointerEventsController.Cleanup();
            sceneBoundsChecker.Stop();
            blockersController.Dispose();
            sceneController.Dispose();
        }
    }
}