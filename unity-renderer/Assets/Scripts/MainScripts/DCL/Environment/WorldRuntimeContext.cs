using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Context related to all the systems involved in the execution of decentraland scenes.
    /// </summary>
    [System.Obsolete("This is kept for retrocompatibilty and will be removed in the future. Use Environment.i.serviceLocator instead.")]
    public class WorldRuntimeContext
    {
        private ServiceLocator serviceLocator;
        public IWorldState state => serviceLocator.Get<IWorldState>();
        public ISceneController sceneController => serviceLocator.Get<ISceneController>();
        public ISceneBoundsChecker sceneBoundsChecker => serviceLocator.Get<ISceneBoundsChecker>();
        public IWorldBlockersController blockersController => serviceLocator.Get<IWorldBlockersController>();
        public IRuntimeComponentFactory componentFactory => serviceLocator.Get<IRuntimeComponentFactory>();
        public ITeleportController teleportController => serviceLocator.Get<ITeleportController>();


        public WorldRuntimeContext (ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }
    }
}