using System;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendEntryBase : MonoBehaviour, IPointerEnterHandler
{
    public class Model
    {
        public string userId;
        public PresenceStatus status;
        public string userName;
        public Vector2 coords;
        public string realm;
        public string realmServerName;
        public string realmLayerName;
        public ILazyTextureObserver avatarSnapshotObserver;
        public bool blocked;
    }

    public Model model { get; private set; } = new Model();

    public Image playerBlockedImage;
    
    [SerializeField] private RectTransform menuPositionReference;
    [SerializeField] protected internal TextMeshProUGUI playerNameText;
    [SerializeField] protected internal RawImage playerImage;
    [SerializeField] protected internal Button menuButton;
    [SerializeField] protected internal AudioEvent audioEventHover;
    [SerializeField] protected internal GameObject onlineStatusContainer;
    [SerializeField] protected internal GameObject offlineStatusContainer;
    [SerializeField] protected internal Button passportButton;
    
    private StringVariable currentPlayerInfoCardId;

    public event Action<FriendEntryBase> OnMenuToggle;

    public virtual void Awake()
    {
        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(() => OnMenuToggle?.Invoke(this));
        passportButton?.onClick.RemoveAllListeners();
        passportButton?.onClick.AddListener(ShowUserProfile);
    }

    public void OnPointerEnter(PointerEventData eventData)
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

    private void OnEnable()
    {
        // TODO: replace image loading for ImageComponentView implementation
        model.avatarSnapshotObserver?.AddListener(OnAvatarImageChange);
    }

    protected virtual void OnDisable()
    {
        model.avatarSnapshotObserver?.RemoveListener(OnAvatarImageChange);
    }

    protected void OnDestroy()
    {
        model.avatarSnapshotObserver?.RemoveListener(OnAvatarImageChange);
    }

    public virtual void Populate(Model model)
    {
        if (playerNameText.text != model.userName)
            playerNameText.text = model.userName;

        playerBlockedImage.enabled = model.blocked;

        if (this.model != null && isActiveAndEnabled)
        {
            this.model.avatarSnapshotObserver?.RemoveListener(OnAvatarImageChange);
            model.avatarSnapshotObserver?.AddListener(OnAvatarImageChange);
        }

        if (onlineStatusContainer != null)
            onlineStatusContainer.SetActive(model.status == PresenceStatus.ONLINE && !model.blocked);
        if (offlineStatusContainer != null)
            offlineStatusContainer.SetActive(model.status != PresenceStatus.ONLINE && !model.blocked);

        this.model = model;
    }

    private void OnAvatarImageChange(Texture2D texture) { playerImage.texture = texture; }

    protected void ShowUserProfile()
    {
        if (currentPlayerInfoCardId == null)
            currentPlayerInfoCardId = Resources.Load<StringVariable>("CurrentPlayerInfoCardId");
        currentPlayerInfoCardId.Set(model.userId);
    }
}