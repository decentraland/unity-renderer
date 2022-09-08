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
    private const int FADEOUT_DELAY = 8000;

    private DataStore dataStore;
    private IChatController chatController;
    private IMainChatNotificationsComponentView mainChatNotificationView;
    private ITopNotificationsComponentView topNotificationView;
    private IUserProfileBridge userProfileBridge;
    private BaseVariable<Transform> notificationPanelTransform => dataStore.HUDs.notificationPanelTransform;
    private BaseVariable<Transform> topNotificationPanelTransform => dataStore.HUDs.topNotificationPanelTransform;
    private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
    private CancellationTokenSource fadeOutCT = new CancellationTokenSource();

    private UserProfile ownUserProfile;

    public ChatNotificationController(DataStore dataStore, IMainChatNotificationsComponentView mainChatNotificationView, ITopNotificationsComponentView topNotificationView, IChatController chatController, IUserProfileBridge userProfileBridge)
    {
        this.dataStore = dataStore;
        this.chatController = chatController;
        this.userProfileBridge = userProfileBridge;
        this.mainChatNotificationView = mainChatNotificationView;
        this.topNotificationView = topNotificationView;
        mainChatNotificationView.OnResetFade += ResetFadeOut;
        topNotificationView.OnResetFade += ResetFadeOut;
        mainChatNotificationView.OnPanelFocus += TogglePanelBackground;
        ownUserProfile = userProfileBridge.GetOwn();
        chatController.OnAddMessage += HandleMessageAdded;
        notificationPanelTransform.Set(mainChatNotificationView.GetPanelTransform());
        topNotificationPanelTransform.Set(topNotificationView.GetPanelTransform());
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
            if(topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                topNotificationView.AddNewChatNotification(message, profile.userName, profile.face256SnapshotURL);
        }
        else
        {
            mainChatNotificationView.AddNewChatNotification(message);
            if (topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
                topNotificationView.AddNewChatNotification(message);
        }
    }

    public void ResetFadeOut(bool fadeOutAfterDelay = false)
    {
        mainChatNotificationView.ShowNotifications();
        if(topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
            topNotificationView.ShowNotification();

        fadeOutCT.Cancel();
        fadeOutCT = new CancellationTokenSource();

        if(fadeOutAfterDelay)
            WaitThenFadeOutNotifications(fadeOutCT.Token).Forget();
    }

    public void TogglePanelBackground(bool isInFocus)
    {
        if (isInFocus)
            mainChatNotificationView.ShowPanel();
        else
            mainChatNotificationView.HidePanel();
    }

    private async UniTaskVoid WaitThenFadeOutNotifications(CancellationToken cancellationToken)
    {
        await UniTask.Delay(FADEOUT_DELAY, cancellationToken: cancellationToken);
        await UniTask.SwitchToMainThread(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        mainChatNotificationView.HideNotifications();

        if(topNotificationPanelTransform.Get().gameObject.activeInHierarchy)
            topNotificationView.HideNotification();
    }

    private UserProfile ExtractRecipient(ChatMessage message) =>
        userProfileBridge.Get(message.sender != ownUserProfile.userId ? message.sender : message.recipient);

    public void SetVisibility(bool visible)
    {
        ResetFadeOut(visible);
        if (visible)
        {
            mainChatNotificationView.Show();
            topNotificationView.Hide();
            mainChatNotificationView.ShowNotifications();
        }
        else
        {
            mainChatNotificationView.Hide();
            topNotificationView.Show();
        }
    }

    public void Dispose()
    {
        chatController.OnAddMessage -= HandleMessageAdded;
        visibleTaskbarPanels.OnChange -= VisiblePanelsChanged; 
        mainChatNotificationView.OnResetFade -= ResetFadeOut;
    }
}
