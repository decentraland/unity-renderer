using System;
using DCL.Models;

namespace DCLPlugins.DebugPlugins.Commons
{
    public interface IShapeListener : IDisposable
    {
        void OnShapeUpdated(IDCLEntity entity);
        void OnShapeCleaned(IDCLEntity entity);
    }
}