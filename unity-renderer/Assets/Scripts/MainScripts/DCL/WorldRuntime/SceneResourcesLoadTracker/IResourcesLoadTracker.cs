using System;

namespace DCL.WorldRuntime
{
    internal interface IResourcesLoadTracker : IDisposable
    {
        int pendingResourcesCount { get; }
        float loadingProgress { get; }
        int totalResourcesCount { get; }
        float totalSizeInMB { get;  }
        float sizeDownloadedInMB { get;  }

        event Action OnResourcesLoaded;
        event Action OnStatusUpdate;
        void PrintWaitingResourcesDebugInfo();
        string GetStateString();
        bool CheckPendingResources();
    }
}
