using System;

namespace DCL.WorldRuntime
{
    internal interface IComponentsLoadTracker : IDisposable
    {
        int pendingResourcesCount { get; }
        float loadingProgress { get; }
        event Action OnComponentsLoaded;
        event Action OnStatusUpdate;
        void PrintWaitingResourcesDebugInfo();
        string GetStateString();
        bool CheckPendingResources();
    }
}