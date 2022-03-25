using System;
using DCL.Interface;

public interface IChatHUDComponentView
{
    event Action<ChatMessage> OnSendMessage;
    event Action<string> OnPressPrivateMessage;
    event Action OnShowMenu;
    event Action OnInputFieldSelected;
    
    int EntryCount { get; }

    void OnMessageCancelHover();
    void AddEntry(ChatEntry.Model chatEntryModel, bool setScrollPositionToBottom = false);
    void Dispose();
    void RemoveFirstEntry();
    void Hide();
    void ClearAllEntries();
    void ResetInputField();
    void FocusInputField();
}