using System;
using UnityEngine;

namespace DCL.LoadingScreen
{
    public interface ILoadingScreenTimeoutView : IDisposable
    {
        event Action OnExitButtonClicked;
        event Action OnJumpHomeButtonClicked;

        void ShowSceneTimeout();

        void HideSceneTimeout();
    }
}
