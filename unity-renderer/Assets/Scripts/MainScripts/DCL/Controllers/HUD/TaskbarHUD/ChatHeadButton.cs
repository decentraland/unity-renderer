using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHeadButton : TaskbarButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] internal ShowHideAnimator labelContainer;
    [SerializeField] internal TMPro.TextMeshProUGUI label;
    [SerializeField] internal RawImage portrait;
    [SerializeField] internal UnreadNotificationBadge unreadNotificationBadge;
    [SerializeField] internal Image onlineStatusIndicator;
    [SerializeField] internal Color onlineColor = Color.green;
    [SerializeField] internal Color offlineColor = Color.gray;

    internal ulong lastTimestamp;
    internal UserProfile profile;

    public void Initialize(UserProfile profile)
    {
        base.Initialize();
        this.profile = profile;
        unreadNotificationBadge.Initialize(ChatController.i, profile.userId);

        if (profile.userName.Length > 10)
            label.text = profile.userName.Substring(0, 10) + "...";
        else
            label.text = profile.userName;

        profile.snapshotObserver.AddListener(Profile_OnFaceSnapshotReadyEvent);
        SetOnlineStatus(false);
    }

    private void Profile_OnFaceSnapshotReadyEvent(Texture2D portraitTexture)
    {
        this.portrait.texture = portraitTexture;
    }

    private void OnDestroy()
    {
        if (profile != null)
            profile.snapshotObserver.RemoveListener(Profile_OnFaceSnapshotReadyEvent);
    }

    public void OnPointerEnter(PointerEventData eventData) { labelContainer.Show(); }

    public void OnPointerExit(PointerEventData eventData) { labelContainer.Hide(); }

    public void SetOnlineStatus(bool isOnline) { onlineStatusIndicator.color = isOnline ? onlineColor : offlineColor; }
}