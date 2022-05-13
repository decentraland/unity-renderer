using System;
using DCL.Interface;

public interface IChatHUDComponentView
{
    event Action<ChatMessage> OnSendMessage;
    event Action<string> OnMessageUpdated;
    event Action OnShowMenu;
    event Action OnInputFieldSelected;
    event Action OnInputFieldDeselected;
    
    int EntryCount { get; }

    IChatEntryFactory ChatEntryFactory { get; set; }
    bool IsInputFieldSelected { get; }

    void OnMessageCancelHover();
    void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false);
    void Dispose();
    void RemoveFirstEntry();
    void ClearAllEntries();
    void ResetInputField(bool loseFocus = false);
    void FocusInputField();
    void UnfocusInputField();
    void SetInputFieldText(string text);
}