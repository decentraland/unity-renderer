using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHeadButton : TaskbarButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] internal ShowHideAnimator labelContainer;
    [SerializeField] internal TMPro.TextMeshProUGUI label;
    [SerializeField] internal Image portrait;
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

        if (profile.faceSnapshot != null)
            portrait.sprite = profile.faceSnapshot;
        else
            profile.OnFaceSnapshotReadyEvent += Profile_OnFaceSnapshotReadyEvent;

        SetOnlineStatus(false);
    }

    private void Profile_OnFaceSnapshotReadyEvent(Sprite portraitSprite)
    {
        profile.OnFaceSnapshotReadyEvent -= Profile_OnFaceSnapshotReadyEvent;

        if (portraitSprite != null && this.portrait.sprite != portraitSprite)
            this.portrait.sprite = portraitSprite;
    }

    private void OnDestroy()
    {
        if (profile != null)
            profile.OnFaceSnapshotReadyEvent -= Profile_OnFaceSnapshotReadyEvent;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        labelContainer.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        labelContainer.Hide();
    }

    public void SetOnlineStatus(bool isOnline)
    {
        onlineStatusIndicator.color = isOnline ? onlineColor : offlineColor;
    }
}
