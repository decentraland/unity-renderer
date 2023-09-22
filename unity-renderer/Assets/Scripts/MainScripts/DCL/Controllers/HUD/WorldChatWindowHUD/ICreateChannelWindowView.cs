using System;
using UnityEngine;

namespace DCL.Social.Chat
{
    public interface ICreateChannelWindowView
    {
        event Action<string> OnChannelNameUpdated;
        event Action OnCreateSubmit;
        event Action OnClose;
        event Action OnJoinChannel;
        RectTransform Transform { get; }
        void Show();
        void Hide();
        void ClearError();
        void DisableCreateButton();
        void EnableCreateButton();
        void ClearInputText();
        void Dispose();
        void FocusInputField();
        void ShowChannelExistsError(bool showJoinChannelOption);
        void ShowWrongFormatError();
        void ShowTooShortError();
        void ShowChannelsExceededError();
        void ShowUnknownError();
    }
}
