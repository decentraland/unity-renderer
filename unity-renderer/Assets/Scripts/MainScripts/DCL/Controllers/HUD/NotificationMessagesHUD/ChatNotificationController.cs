using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DCL.Interface;
using DCL;

public class ChatNotificationController : IHUD
{
    private DataStore dataStore;
    private IChatController chatController;
    private MainChatNotificationsComponentView mainChatNotificationView;
    private IUserProfileBridge userProfileBridge;
    private BaseVariable<Transform> notificationPanelTransform => dataStore.HUDs.notificationPanelTransform;
    private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
    public CancellationTokenSource fadeOutCT = new CancellationTokenSource();

    private UserProfile ownUserProfile;

    public ChatNotificationController(DataStore dataStore, MainChatNotificationsComponentView mainChatNotificationView, IChatController chatController, IUserProfileBridge userProfileBridge)
    {
        this.dataStore = dataStore;
        this.chatController = chatController;
        this.userProfileBridge = userProfileBridge;
        this.mainChatNotificationView = mainChatNotificationView;
        mainChatNotificationView.OnResetFade += ResetFadeout;
        ownUserProfile = userProfileBridge.GetOwn();
        chatController.OnAddMessage += HandleMessageAdded;
        notificationPanelTransform.Set(mainChatNotificationView.gameObject.transform);
        visibleTaskbarPanels.OnChange += VisiblePanelsChanged;
    }

    private void VisiblePanelsChanged(HashSet<string> newList, HashSet<string> oldList)
    {
        SetVisibility(newList.Count == 0);
    }

    private void HandleMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE && message.messageType != ChatMessage.Type.PUBLIC) return;
        if (message.sender == ownUserProfile.userId) return;

        if (message.sender != null)
        {
            var profile = ExtractRecipient(message);
            if (profile == null) return;
            mainChatNotificationView.AddNewChatNotification(message, profile.userName, profile.face256SnapshotURL);
        }
        else
        {
            mainChatNotificationView.AddNewChatNotification(message);
        }
    }

    public void ResetFadeout(bool fadeOutAfterDelay = false)
    {
        mainChatNotificationView.ShowNotifications();
        fadeOutCT.Cancel();
        fadeOutCT = new CancellationTokenSource();

        if(fadeOutAfterDelay)
            WaitThenFadeOutNotifications(fadeOutCT.Token).Forget();
    }

    private async UniTaskVoid WaitThenFadeOutNotifications(CancellationToken cancellationToken)
    {
        await UniTask.Delay(8000, cancellationToken: cancellationToken);
        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        mainChatNotificationView.HideNotifications();
    }

    private UserProfile ExtractRecipient(ChatMessage message) =>
        userProfileBridge.Get(message.sender != ownUserProfile.userId ? message.sender : message.recipient);

    public void SetVisibility(bool visible)
    {
        ResetFadeout(visible);
        if (visible)
        {
            mainChatNotificationView.Show();
            mainChatNotificationView.ShowNotifications();
        }
        else
        {
            mainChatNotificationView.Hide();
        }
    }

    public void Dispose()
    {
        chatController.OnAddMessage -= HandleMessageAdded;
        visibleTaskbarPanels.OnChange -= VisiblePanelsChanged;
    }
}
