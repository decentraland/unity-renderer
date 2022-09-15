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
    private const int NEW_NOTIFICATION_DELAY = 5000;

    public event Action<string> OnClickedNotification;

    [SerializeField] private ChatNotificationMessageComponentView chatNotificationComponentView;
    [SerializeField] private Sprite notificationsImage;

    public event Action<bool> OnResetFade;

    //This structure is temporary for the first integration of the top notification, it will change when further defined
    private float normalContentXPos = 111;
    private float offsetContentXPos;
    private float normalHeaderXPos = 70;
    private float offsetHeaderXPos;

    private CancellationTokenSource animationCancellationToken = new CancellationTokenSource();
    private CancellationTokenSource waitCT = new CancellationTokenSource();
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
        chatNotificationComponentView.shouldAnimateFocus = false;
        chatNotificationComponentView.SetIsPrivate(true);
    }

    public Transform GetPanelTransform()
    {
        return gameObject.transform;
    }

    public bool isShowingNotification = false;
    int stackedNotifications = 0;

    public void AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null)
    {
        stackedNotifications++;
        if(isShowingNotification && stackedNotifications <= 2)
        {
            waitCT.Cancel();
            waitCT = new CancellationTokenSource();
            WaitBeforeShowingNewNotification(message, waitCT.Token, username, profilePicture).Forget();
            return;
        }

        isShowingNotification = true;
        animationCancellationToken.Cancel();
        animationCancellationToken = new CancellationTokenSource();
        chatNotificationComponentView.gameObject.SetActive(true);
        if(stackedNotifications > 2){
            PopulateMultipleNotification();
            AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
            OnResetFade?.Invoke(true);
            return;
        }
        stackedNotifications--;
        if (message.messageType == ChatMessage.Type.PRIVATE)
        {
            PopulatePrivateNotification(message, username, profilePicture);
            chatNotificationComponentView.SetPositionOffset(normalHeaderXPos, normalContentXPos);
            AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
            OnResetFade?.Invoke(true);
            ShowNotificationCooldown();
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            PopulatePublicNotification(message, username);
            chatNotificationComponentView.SetPositionOffset(offsetHeaderXPos, offsetContentXPos);
            AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
            OnResetFade?.Invoke(true);
            ShowNotificationCooldown();
        }
    }

    private async UniTaskVoid ShowNotificationCooldown()
    {
        await UniTask.Delay(NEW_NOTIFICATION_DELAY);
        stackedNotifications--;
        isShowingNotification = false;
    }

    private async UniTaskVoid WaitBeforeShowingNewNotification(ChatMessage message, CancellationToken cancellationToken, string username = null, string profilePicture = null)
    {
        while (isShowingNotification)
            await UniTask.NextFrame(cancellationToken).AttachExternalCancellation(cancellationToken);

        AddNewChatNotification(message, username, profilePicture);
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
        chatNotificationComponentView.SetNotificationSender($"{username}:");
        chatNotificationComponentView.SetNotificationTargetId(message.sender);
    }

    private void PopulatePublicNotification(ChatMessage message, string username = null)
    {
        chatNotificationComponentView.SetIsPrivate(false);
        chatNotificationComponentView.SetMessage(message.body);

        var channelId = string.IsNullOrEmpty(message.recipient) ? "nearby" : message.recipient;
        var channelName = string.IsNullOrEmpty(message.recipient) ? "~nearby" : $"#{message.recipient}";

        chatNotificationComponentView.SetNotificationTargetId(channelId);
        chatNotificationComponentView.SetNotificationHeader(channelName);
        chatNotificationComponentView.SetNotificationSender($"{username}:");
    }

    private void PopulateMultipleNotification()
    {
        chatNotificationComponentView.SetMessage("");
        chatNotificationComponentView.SetNotificationTargetId("conversationList");
        chatNotificationComponentView.SetNotificationHeader("CHAT NOTIFICATIONS");
        chatNotificationComponentView.SetNotificationSender($"{stackedNotifications} messages");
        chatNotificationComponentView.SetIsMultipleNotifications();
    }

    public override void Show(bool instant = false)
    {
        if (gameObject.activeInHierarchy)
            return;

        chatNotificationComponentView.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public override void Hide(bool instant = false)
    {
        if (!gameObject.activeInHierarchy)
            return;

        isShowingNotification = false;
        stackedNotifications = 0;
        chatNotificationComponentView.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ShowNotification()
    {
        chatNotificationComponentView.Show();
    }

    public void HideNotification()
    {
        isShowingNotification = false;
        stackedNotifications = 0;
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
