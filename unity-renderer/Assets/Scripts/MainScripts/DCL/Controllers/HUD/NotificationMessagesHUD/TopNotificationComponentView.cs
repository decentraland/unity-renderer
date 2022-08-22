using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class TopNotificationComponentView : BaseComponentView, ITopNotificationsComponentView
{
    [SerializeField] private ChatNotificationMessageComponentView chatNotificationComponentView;

    public event Action<string> OnClickedNotification;

    public static TopNotificationComponentView Create()
    {
        return Instantiate(Resources.Load<TopNotificationComponentView>("SocialBarV1/TopNotificationHUD"));
    }

    public Transform GetPanelTransform()
    {
        return gameObject.transform;
    }

    public void AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null)
    {
        if (message.messageType == ChatMessage.Type.PRIVATE)
        {
            PopulatePrivateNotification(message, username, profilePicture);
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            PopulatePublicNotification(message, username);
        }
    }

    private void PopulatePrivateNotification(ChatMessage message, string username = null, string profilePicture = null)
    {
        chatNotificationComponentView.SetIsPrivate(true);
        chatNotificationComponentView.SetMessage(message.body);
        chatNotificationComponentView.SetNotificationHeader("Private message");
        chatNotificationComponentView.SetNotificationSender(username);
        chatNotificationComponentView.SetNotificationTargetId(message.sender);
        if (profilePicture != null)
            chatNotificationComponentView.SetImage(profilePicture);
    }

    private void PopulatePublicNotification(ChatMessage message, string username = null)
    {
        chatNotificationComponentView.SetIsPrivate(false);
        chatNotificationComponentView.SetMessage(message.body);

        var channelId = string.IsNullOrEmpty(message.recipient) ? "nearby" : message.recipient;
        var channelName = string.IsNullOrEmpty(message.recipient) ? "~nearby" : $"#{message.recipient}";

        chatNotificationComponentView.SetNotificationTargetId(channelId);
        chatNotificationComponentView.SetNotificationHeader(channelName);
        chatNotificationComponentView.SetNotificationSender(username);
    }

    public override void Show(bool instant = false)
    {
        gameObject.SetActive(true);
    }

    public override void Hide(bool instant = false)
    {
        gameObject.SetActive(false);
    }

    public void ShowNotification()
    {
        chatNotificationComponentView.Show();
    }

    public void HideNotification()
    {
        chatNotificationComponentView.Hide();
    }

    private void ClickedOnNotification(string targetId)
    {
        OnClickedNotification?.Invoke(targetId);
    }

    public override void RefreshControl()
    {

    }
}
