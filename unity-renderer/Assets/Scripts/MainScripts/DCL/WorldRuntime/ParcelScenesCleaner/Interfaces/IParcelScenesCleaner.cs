using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL
{
    public interface IParcelScenesCleaner : IService
    {
        void MarkForCleanup(IDCLEntity entity);
        void MarkRootEntityForCleanup(IParcelScene scene, IDCLEntity entity);
        void MarkDisposableComponentForCleanup(IParcelScene scene, string componentId);
        void CleanMarkedEntities();
        public IEnumerator CleanMarkedEntitiesAsync(bool immediate = false);
    }
}