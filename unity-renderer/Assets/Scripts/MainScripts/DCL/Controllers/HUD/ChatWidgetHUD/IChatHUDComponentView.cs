﻿using DCL.Chat.HUD;
using DCL.Interface;
using System;
using System.Collections.Generic;

public interface IChatHUDComponentView
{
    event Action<ChatMessage> OnSendMessage;
    event Action<string, int> OnMessageUpdated;
    event Action OnOpenedContextMenu;
    event Action OnShowMenu;
    event Action OnInputFieldSelected;
    event Action OnInputFieldDeselected;
    event Action OnPreviousChatInHistory;
    event Action OnNextChatInHistory;
    event Action<string> OnMentionSuggestionSelected;

    int EntryCount { get; }
    IComparer<ChatEntryModel> SortingStrategy { get; set; }

    void OnMessageCancelHover();
    void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false);
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
