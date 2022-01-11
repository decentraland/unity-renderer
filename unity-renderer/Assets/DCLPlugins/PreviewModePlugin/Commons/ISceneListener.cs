using System;
using DCL.Models;

namespace DCLPlugins.PreviewModePlugin.Commons
{
    public interface ISceneListener : IDisposable
    {
        void OnEntityAdded(IDCLEntity entity);
    }
}