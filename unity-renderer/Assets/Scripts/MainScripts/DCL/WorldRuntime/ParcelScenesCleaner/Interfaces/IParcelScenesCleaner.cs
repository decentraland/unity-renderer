using System;
using DCL.Controllers;
using DCL.Models;

namespace DCL
{
    public interface IParcelScenesCleaner : IDisposable
    {
        void Initialize();
        void MarkForCleanup(IDCLEntity entity);
        void MarkRootEntityForCleanup(IParcelScene scene, IDCLEntity entity);
        void MarkDisposableComponentForCleanup(IParcelScene scene, string componentId);
        void ForceCleanup();
    }
}