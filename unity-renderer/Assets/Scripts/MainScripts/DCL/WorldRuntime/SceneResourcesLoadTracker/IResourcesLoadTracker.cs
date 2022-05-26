using System;

namespace DCL.WorldRuntime
{
    internal interface IResourcesLoadTracker : IDisposable
    {
        int pendingResourcesCount { get; }
        float loadingProgress { get; }
        event Action OnResourcesLoaded;
        event Action OnStatusUpdate;
        void PrintWaitingResourcesDebugInfo();
        string GetStateString();
        bool CheckPendingResources();
    }
}