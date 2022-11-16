using System;
using DCL.Chat.HUD;
using DCL.Interface;

public interface IChatHUDComponentView
{
    event Action<ChatMessage> OnSendMessage;
    event Action<string> OnMessageUpdated;
    event Action OnShowMenu;
    event Action OnInputFieldSelected;
    event Action OnInputFieldDeselected;
    event Action OnPreviousChatInHistory;
    event Action OnNextChatInHistory;
    
    int EntryCount { get; }

    void OnMessageCancelHover();
    void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false);
    void Dispose();
    void RemoveOldestEntry();
    void ClearAllEntries();
    void ResetInputField(bool loseFocus = false);
    void FocusInputField();
    void UnfocusInputField();
    void SetInputFieldText(string text);
}