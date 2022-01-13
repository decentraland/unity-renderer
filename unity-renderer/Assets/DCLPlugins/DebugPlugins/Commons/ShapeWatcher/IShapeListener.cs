using System;
using DCL.Models;

namespace DCLPlugins.PreviewModePlugin.Commons
{
    public interface IShapeListener : IDisposable
    {
        void OnShapeUpdated(IDCLEntity entity);
    }
}