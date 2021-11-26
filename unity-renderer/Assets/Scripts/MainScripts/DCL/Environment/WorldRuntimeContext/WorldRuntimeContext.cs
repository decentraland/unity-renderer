﻿using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Context related to all the systems involved in the execution of decentraland scenes.
    /// </summary>
    public class WorldRuntimeContext
    {
        private ServiceLocator serviceLocator;
        public IWorldState state => serviceLocator.Get<IWorldState>();
        public ISceneController sceneController => serviceLocator.Get<ISceneController>();
        public IPointerEventsController pointerEventsController => serviceLocator.Get<IPointerEventsController>();
        public ISceneBoundsChecker sceneBoundsChecker => serviceLocator.Get<ISceneBoundsChecker>();
        public IWorldBlockersController blockersController => serviceLocator.Get<IWorldBlockersController>();
        public IRuntimeComponentFactory componentFactory => serviceLocator.Get<IRuntimeComponentFactory>();

        public WorldRuntimeContext (ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }
    }
}