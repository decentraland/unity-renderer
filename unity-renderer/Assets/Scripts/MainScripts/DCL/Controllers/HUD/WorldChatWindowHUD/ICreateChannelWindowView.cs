using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public interface ICreateChannelWindowView
    {
        event Action<string> OnChannelNameUpdated;
        event Action OnCreateSubmit;
        event Action OnClose;
        event Action OnOpenChannel;
        RectTransform Transform { get; }
        void Show();
        void Hide();
        void ShowError(string message);
        void ShowChannelExistsError(bool showJoinChannelOption);
        void ClearError();
        void DisableCreateButton();
        void EnableCreateButton();
        void ClearInputText();
        void Dispose();
        void FocusInputField();
    }
}