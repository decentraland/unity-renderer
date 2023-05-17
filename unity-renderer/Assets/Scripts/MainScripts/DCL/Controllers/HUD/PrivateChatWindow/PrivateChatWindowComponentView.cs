using DCL.Chat.HUD;
using DCL.Social.Chat;
using DCL.Social.Friends;
using SocialBar.UserThumbnail;
using SocialFeaturesAnalytics;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PrivateChatWindowComponentView : BaseComponentView, IPrivateChatComponentView, IPointerDownHandler
{
    private const float REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD = 0.995f;

    [SerializeField] internal Button backButton;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal UserThumbnailComponentView userThumbnail;
    [SerializeField] internal TMP_Text userNameLabel;
    [SerializeField] internal PrivateChatHUDView chatView;
    [SerializeField] internal GameObject jumpInButtonContainer;
    [SerializeField] internal JumpInButton jumpInButton;
    [SerializeField] internal UserContextMenu userContextMenu;
    [SerializeField] internal RectTransform userContextMenuReferencePoint;
    [SerializeField] internal Button optionsButton;
    [SerializeField] internal GameObject messagesLoading;
    [SerializeField] internal ScrollRect scroll;
    [SerializeField] internal GameObject oldMessagesLoadingContainer;
    [SerializeField] private Model model;

    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private Coroutine alphaRoutine;
    private Vector2 originalSize;

    public event Action OnPressBack;
    public event Action OnMinimize;
    public event Action OnClose;
    public event Action<string> OnUnfriend
    {
        add => userContextMenu.OnUnfriend += value;
        remove => userContextMenu.OnUnfriend -= value;
    }

    public event Action OnRequireMoreMessages;
    public event Action<bool> OnFocused
    {
        add => onFocused += value;
        remove => onFocused -= value;
    }
    public event Action OnClickOverWindow;

    public IChatHUDComponentView ChatHUD => chatView;
    public bool IsActive => gameObject.activeInHierarchy;
    public RectTransform Transform => (RectTransform) transform;
    public bool IsFocused => isFocused;

    public override void Awake()
    {
        base.Awake();
        originalSize = ((RectTransform) transform).sizeDelta;
        backButton.onClick.AddListener(() => OnPressBack?.Invoke());
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        optionsButton.onClick.AddListener(ShowOptions);
        userContextMenu.OnBlock += HandleBlockFromContextMenu;
        scroll.onValueChanged.AddListener((scrollPos) =>
        {
            if (scrollPos.y > REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD)
                OnRequireMoreMessages?.Invoke();
        });
    }

    public void Initialize(IFriendsController friendsController, ISocialAnalytics socialAnalytics)
    {
        this.friendsController = friendsController;
        this.socialAnalytics = socialAnalytics;
    }

    public override void Dispose()
    {
        if (!this) return;
        if (!gameObject) return;

        if (userContextMenu != null)
        {
            userContextMenu.OnBlock -= HandleBlockFromContextMenu;
        }

        base.Dispose();
    }

    public void SetLoadingMessagesActive(bool isActive)
    {
        if (messagesLoading == null)
            return;

        messagesLoading.SetActive(isActive);
    }

    public void SetOldMessagesLoadingActive(bool isActive)
    {
        if (oldMessagesLoadingContainer == null)
            return;

        oldMessagesLoadingContainer.SetActive(isActive);
        oldMessagesLoadingContainer.transform.SetAsFirstSibling();
    }

    public override void RefreshControl()
    {
        userThumbnail.Configure(new UserThumbnailComponentModel
        {
            faceUrl = model.faceSnapshotUrl,
            isBlocked = model.isUserBlocked,
            isOnline = model.isUserOnline
        });
        userNameLabel.SetText(model.userName);
        jumpInButtonContainer.SetActive(model.isUserOnline);
    }

    public void Setup(UserProfile profile, bool isOnline, bool isBlocked)
    {
        model = new Model
        {
            userId = profile.userId,
            faceSnapshotUrl = profile.face256SnapshotURL,
            userName = profile.userName,
            isUserOnline = isOnline,
            isUserBlocked = isBlocked
        };
        RefreshControl();

        jumpInButton.Initialize(friendsController, profile.userId, socialAnalytics);
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void OnPointerDown(PointerEventData eventData) => OnClickOverWindow?.Invoke();

    private void ShowOptions()
    {
        var contextMenuTransform = (RectTransform) userContextMenu.transform;
        contextMenuTransform.pivot = userContextMenuReferencePoint.pivot;
        contextMenuTransform.position = userContextMenuReferencePoint.position;
        userContextMenu.Show(model.userId);
    }

    private void HandleBlockFromContextMenu(string userId, bool isBlocked)
    {
        if (userId != model.userId) return;
        model.isUserBlocked = isBlocked;
        RefreshControl();
    }

    [Serializable]
    private struct Model
    {
        public string userId;
        public string userName;
        public string faceSnapshotUrl;
        public bool isUserBlocked;
        public bool isUserOnline;
    }
}
