﻿using System;
using DCL.Interface;

public interface IChatHUDComponentView
{
    event Action<ChatMessage> OnSendMessage;
    event Action<string> OnMessageUpdated;
    event Action OnShowMenu;
    event Action OnInputFieldSelected;
    
    int EntryCount { get; }

    IChatEntryFactory ChatEntryFactory { get; set; }

    void OnMessageCancelHover();
    void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false);
    void Dispose();
    void RemoveFirstEntry();
    void ClearAllEntries();
    void ResetInputField(bool loseFocus = false);
    void FocusInputField();
    void SetInputFieldText(string text);
}