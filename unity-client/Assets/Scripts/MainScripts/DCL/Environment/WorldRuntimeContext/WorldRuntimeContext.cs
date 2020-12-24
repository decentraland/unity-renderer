using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Context related to all the systems involved in the execution of decentraland scenes.
    /// </summary>
    public class WorldRuntimeContext : System.IDisposable
    {
        public readonly IWorldState state;
        public readonly ISceneController sceneController;
        public readonly IPointerEventsController pointerEventsController;
        public readonly ISceneBoundsChecker sceneBoundsChecker;
        public readonly IWorldBlockersController blockersController;

        public WorldRuntimeContext(IWorldState state,
            ISceneController sceneController,
            IPointerEventsController pointerEventsController,
            ISceneBoundsChecker sceneBoundsChecker,
            IWorldBlockersController blockersController)
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