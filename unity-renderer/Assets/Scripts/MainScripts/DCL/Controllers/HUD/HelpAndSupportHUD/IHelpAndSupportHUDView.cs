using System;

namespace DCL.HelpAndSupportHUD
{
    public interface IHelpAndSupportHUDView : IDisposable
    {
        void Initialize();

        void SetVisibility(bool visibility);

        event Action OnDiscordButtonPressed;
        event Action OnFaqButtonPressed;
        event Action OnSupportButtonPressed;
    }
}
