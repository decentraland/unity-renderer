using DCL;
using DCL.Social.Friends;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendEntryBase : BaseComponentView
{
    private const string OPEN_PASSPORT_SOURCE = "FriendsHUD";
    public FriendEntryModel Model { get; private set; } = new FriendEntryModel();

    public Image playerBlockedImage;

    [SerializeField] private RectTransform menuPositionReference;
    [SerializeField] protected internal TextMeshProUGUI playerNameText;
    [SerializeField] protected internal RawImage playerImage;
    [SerializeField] protected internal Button menuButton;
    [SerializeField] protected internal AudioEvent audioEventHover;
    [SerializeField] protected internal GameObject onlineStatusContainer;
    [SerializeField] protected internal GameObject offlineStatusContainer;
    [SerializeField] protected internal Button passportButton;

    private BaseVariable<(string playerId, string source)> currentPlayerInfoCardId;
    private bool avatarFetchingEnabled;

    public event Action<FriendEntryBase> OnMenuToggle;

    public override void Awake()
    {
        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(() => OnMenuToggle?.Invoke(this));
        passportButton?.onClick.RemoveAllListeners();
        passportButton?.onClick.AddListener(ShowUserProfile);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (audioEventHover != null)
            audioEventHover.Play(true);
    }

    public void Dock(UserContextMenu contextMenuPanel)
    {
        var panelTransform = (RectTransform) contextMenuPanel.transform;
        panelTransform.pivot = menuPositionReference.pivot;
        panelTransform.position = menuPositionReference.position;
    }

    public override void OnDisable()
    {
        DisableAvatarSnapshotFetching();
    }

    protected void OnDestroy()
    {
        DisableAvatarSnapshotFetching();
    }

    public virtual void EnableAvatarSnapshotFetching()
    {
        if (avatarFetchingEnabled) return;
        avatarFetchingEnabled = true;
        // TODO: replace image loading for ImageComponentView implementation
        Model?.avatarSnapshotObserver?.AddListener(OnAvatarImageChange);
    }

    public virtual void DisableAvatarSnapshotFetching()
    {
        if (!avatarFetchingEnabled) return;
        avatarFetchingEnabled = false;
        // TODO: replace image loading for ImageComponentView implementation
        Model?.avatarSnapshotObserver?.RemoveListener(OnAvatarImageChange);
    }

    public override void RefreshControl()
    {
        if (playerNameText.text != Model.userName)
            playerNameText.text = Model.userName;

        playerBlockedImage.enabled = Model.blocked;

        Model?.avatarSnapshotObserver?.RemoveListener(OnAvatarImageChange);

        if (isActiveAndEnabled && avatarFetchingEnabled)
            // TODO: replace image loading for ImageComponentView implementation
            Model.avatarSnapshotObserver?.AddListener(OnAvatarImageChange);

        if (onlineStatusContainer != null)
            onlineStatusContainer.SetActive(Model.status == PresenceStatus.ONLINE && !Model.blocked);
        if (offlineStatusContainer != null)
            offlineStatusContainer.SetActive(Model.status != PresenceStatus.ONLINE && !Model.blocked);
    }

    public virtual void Populate(FriendEntryModel model)
    {
        Model = model;
        RefreshControl();
    }

    public virtual bool IsVisible(RectTransform container)
    {
        if (!gameObject.activeSelf) return false;
        return ((RectTransform) transform).CountCornersVisibleFrom(container) > 0;
    }

    private void OnAvatarImageChange(Texture2D texture) { playerImage.texture = texture; }

    private void ShowUserProfile()
    {
        if (currentPlayerInfoCardId == null)
            currentPlayerInfoCardId = DataStore.i.HUDs.currentPlayerId;

        currentPlayerInfoCardId.Set((Model.userId, OPEN_PASSPORT_SOURCE));
    }
}
