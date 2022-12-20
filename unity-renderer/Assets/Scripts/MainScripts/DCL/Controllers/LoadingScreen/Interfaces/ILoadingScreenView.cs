using System;

namespace DCL.LoadingScreen
{
    public interface ILoadingScreenView : IDisposable
    {
        void UpdateLoadingMessage();
    }
}
