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
    private const float X_OFFSET = 32f;

    [SerializeField] private ChatNotificationMessageComponentView chatNotificationComponentView;

    //This structure is temporary for the first integration of the top notification, it will change when further defined
    private float normalContentXPos = 111;
    private float offsetContentXPos;
    private float normalHeaderXPos = 70;
    private float offsetHeaderXPos;

    public event Action<string> OnClickedNotification;

    public static TopNotificationComponentView Create()
    {
        return Instantiate(Resources.Load<TopNotificationComponentView>("SocialBarV1/TopNotificationHUD"));
    }

    public void Start()
    {
        chatNotificationComponentView.OnClickedNotification += ClickedOnNotification;
        offsetContentXPos = normalContentXPos - X_OFFSET;
        offsetHeaderXPos = normalHeaderXPos - X_OFFSET;
        chatNotificationComponentView.SetPositionOffset(normalHeaderXPos, normalContentXPos);
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
            chatNotificationComponentView.SetPositionOffset(normalHeaderXPos, normalContentXPos);
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            PopulatePublicNotification(message, username);
            chatNotificationComponentView.SetPositionOffset(offsetHeaderXPos, offsetContentXPos);
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

    public override void Dispose()
    {
        base.Dispose();

        chatNotificationComponentView.OnClickedNotification -= ClickedOnNotification;
    }

    public override void RefreshControl()
    {

    }
}
