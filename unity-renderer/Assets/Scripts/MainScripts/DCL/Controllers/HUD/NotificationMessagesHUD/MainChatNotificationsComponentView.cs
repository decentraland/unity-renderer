using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;

public class MainChatNotificationsComponentView : BaseComponentView
{
    [SerializeField] RectTransform chatEntriesContainer;
    [SerializeField] GameObject chatNotification;

    public static MainChatNotificationsComponentView Create()
    {
        return Instantiate(Resources.Load<MainChatNotificationsComponentView>("SocialBarV1/ChatNotificationHUD"));
    }

    public void AddNewChatNotification(ChatMessage message)
    {
        ChatNotificationMessageComponentView chatNotificationComponentView = Instantiate(chatNotification).GetComponent<ChatNotificationMessageComponentView>();
        chatNotificationComponentView.transform.SetParent(chatEntriesContainer, false);
        chatNotificationComponentView.SetMessage(message.body);
        chatNotificationComponentView.SetNotificationHeader(message.sender);
        //chatNotificationComponentView.SetTimestamp(message.timestamp);
    }

    public override void RefreshControl()
    {

    }

}
