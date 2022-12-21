using System;

namespace DCL.LoadingScreen
{
    public interface ILoadingScreenView
    {
        void UpdateLoadingMessage();

        void Dispose();
    }
}
