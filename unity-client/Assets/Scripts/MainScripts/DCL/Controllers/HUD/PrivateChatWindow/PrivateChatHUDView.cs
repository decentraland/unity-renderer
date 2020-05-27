using DCL.Helpers;
using UnityEngine;

public class PrivateChatHUDView : ChatHUDView
{
    string ENTRY_PATH_SENT = "ChatEntrySent";
    string ENTRY_PATH_RECEIVED = "ChatEntryReceived";

    public override void AddEntry(ChatEntry.Model chatEntryModel, bool setScrollPositionToBottom = false)
    {
        var chatEntryGO = Instantiate(Resources.Load(chatEntryModel.subType == ChatEntry.Model.SubType.PRIVATE_TO ? ENTRY_PATH_SENT : ENTRY_PATH_RECEIVED) as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();

        chatEntry.SetFadeout(false);
        chatEntry.Populate(chatEntryModel);

        chatEntry.OnTriggerHover += OnMessageTriggerHover;
        chatEntry.OnCancelHover += OnMessageCancelHover;

        entries.Add(chatEntry);
        Utils.ForceUpdateLayout(transform as RectTransform, delayed: false);

        if (setScrollPositionToBottom)
            scrollRect.verticalNormalizedPosition = 0;
    }

    protected override void OnMessageTriggerHover(ChatEntry chatEntry)
    {
        (messageHoverPanel.transform as RectTransform).pivot = new Vector2(chatEntry.model.subType == ChatEntry.Model.SubType.PRIVATE_TO ? 1 : 0, 0.5f);

        base.OnMessageTriggerHover(chatEntry);
    }
}
