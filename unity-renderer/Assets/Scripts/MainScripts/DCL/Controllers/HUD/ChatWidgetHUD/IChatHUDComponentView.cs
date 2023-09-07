using DCL.Social.Chat;
using DCL.Interface;
using System;
using System.Collections.Generic;

namespace DCL.Social.Chat
{
    public interface IChatHUDComponentView
    {
        event Action<ChatMessage> OnSendMessage;
        event Action<string, int> OnMessageUpdated;
        event Action<string> OnOpenedContextMenu;
        event Action OnShowMenu;
        event Action OnInputFieldSelected;
        event Action OnInputFieldDeselected;
        event Action OnPreviousChatInHistory;
        event Action OnNextChatInHistory;
        event Action<string> OnMentionSuggestionSelected;
        event Action<ChatEntryModel> OnCopyMessageRequested;

        int EntryCount { get; }
        IComparer<ChatEntryModel> SortingStrategy { set; }
        bool UseLegacySorting { set; }

        void OnMessageCancelHover();

        void SetEntry(ChatEntryModel model, bool setScrollPositionToBottom = false);

        void Dispose();

        void RemoveOldestEntry();

        void ClearAllEntries();

        void ResetInputField(bool loseFocus = false);

        void FocusInputField();

        void UnfocusInputField();

        void SetInputFieldText(string text);

        void ShowMentionSuggestions();

        void SetMentionSuggestions(List<ChatMentionSuggestionModel> suggestions);

        void HideMentionSuggestions();

        void AddMentionToInputField(int fromIndex, int length, string userId, string userName);

        void AddTextIntoInputField(string text);
    }
}
