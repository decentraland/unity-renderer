using System;
using DCL.Models;

namespace DCLPlugins.DebugPlugins.Commons
{
    public interface ISceneListener : IDisposable
    {
        void OnEntityAdded(IDCLEntity entity);
        void OnEntityRemoved(IDCLEntity entity);
    }
}