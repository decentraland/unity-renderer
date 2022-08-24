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

    public event Action<string> OnClickedNotification;

    [SerializeField] private ChatNotificationMessageComponentView chatNotificationComponentView;

    //This structure is temporary for the first integration of the top notification, it will change when further defined
    private float normalContentXPos = 111;
    private float offsetContentXPos;
    private float normalHeaderXPos = 70;
    private float offsetHeaderXPos;

    private CancellationTokenSource animationCancellationToken = new CancellationTokenSource();
    private RectTransform notificationRect;

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
        notificationRect = chatNotificationComponentView.gameObject.GetComponent<RectTransform>();
    }

    public Transform GetPanelTransform()
    {
        return gameObject.transform;
    }

    public void AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null)
    {
        animationCancellationToken.Cancel();
        animationCancellationToken = new CancellationTokenSource();
        if (message.messageType == ChatMessage.Type.PRIVATE)
        {
            PopulatePrivateNotification(message, username, profilePicture);
            chatNotificationComponentView.SetPositionOffset(normalHeaderXPos, normalContentXPos);
            AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            PopulatePublicNotification(message, username);
            chatNotificationComponentView.SetPositionOffset(offsetHeaderXPos, offsetContentXPos);
            AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
        }
    }

    private async UniTaskVoid AnimateNewEntry(RectTransform notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Sequence mySequence = DOTween.Sequence().AppendInterval(0.2f).Append(notification.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        try
        {
            Vector2 endPosition = new Vector2(0, 0);
            Vector2 currentPosition = notification.anchoredPosition;
            notification.localScale = Vector3.zero;
            DOTween.To(() => currentPosition, x => currentPosition = x, endPosition, 0.8f).SetEase(Ease.OutCubic);
            while (notification.anchoredPosition.y < 0)
            {
                notification.anchoredPosition = currentPosition;
                await UniTask.NextFrame(cancellationToken).AttachExternalCancellation(cancellationToken); ;
            }
            mySequence.Play();
        }
        catch (OperationCanceledException ex)
        {
            if (!DOTween.IsTweening(notification))
                notification.DOScale(1, 0.3f).SetEase(Ease.OutBack);
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
