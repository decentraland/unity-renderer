using System;

namespace DCL
{
    public interface IDebugController : IDisposable
    {
        event Action OnDebugModeSet;
        void SetDebug();
        void HideFPSPanel();
        void ShowFPSPanel();
        void SetSceneDebugPanel();
        void SetEngineDebugPanel();
    }
}