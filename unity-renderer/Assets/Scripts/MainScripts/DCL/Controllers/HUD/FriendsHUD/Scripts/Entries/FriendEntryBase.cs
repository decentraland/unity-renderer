using System;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendEntryBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    [NonSerialized] public string userId;

    public Image playerBlockedImage;
    public Transform menuPositionReference;

    [SerializeField] protected internal TextMeshProUGUI playerNameText;
    [SerializeField] protected internal RawImage playerImage;
    [SerializeField] protected internal Button menuButton;
    [SerializeField] protected internal Image backgroundImage;
    [SerializeField] protected internal Sprite hoveredBackgroundSprite;
    [SerializeField] protected internal AudioEvent audioEventHover;
    [SerializeField] protected internal GameObject onlineStatusContainer;
    [SerializeField] protected internal GameObject offlineStatusContainer;
    [SerializeField] protected internal Button passportButton;
    [SerializeField] private Selectable.Transition transition = Selectable.Transition.SpriteSwap;
    [SerializeField] private ColorBlock transitionColors;
    
    protected internal Sprite unhoveredBackgroundSprite;
    private StringVariable currentPlayerInfoCardId;
    private Color originalBackgroundColor;

    public event Action<FriendEntryBase> OnMenuToggle;

    public virtual void Awake()
    {
        originalBackgroundColor = backgroundImage.color;
        unhoveredBackgroundSprite = backgroundImage.sprite;
        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(() => OnMenuToggle?.Invoke(this));
        passportButton?.onClick.RemoveAllListeners();
        passportButton?.onClick.AddListener(ShowUserProfile);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (transition == Selectable.Transition.SpriteSwap)
            backgroundImage.sprite = hoveredBackgroundSprite;
        else if (transition == Selectable.Transition.ColorTint)
            backgroundImage.color = originalBackgroundColor * transitionColors.highlightedColor;

        if (audioEventHover != null)
            audioEventHover.Play(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (transition == Selectable.Transition.SpriteSwap)
            backgroundImage.sprite = unhoveredBackgroundSprite;
        else if (transition == Selectable.Transition.ColorTint)
            backgroundImage.color = originalBackgroundColor * transitionColors.normalColor;
    }

    private void OnEnable()
    {
        // TODO: replace image loading for ImageComponentView implementation
        model.avatarSnapshotObserver?.AddListener(OnAvatarImageChange);
        
        if (transition == Selectable.Transition.ColorTint)
            backgroundImage.color = originalBackgroundColor * transitionColors.normalColor;
    }

    protected virtual void OnDisable()
    {
        model.avatarSnapshotObserver?.RemoveListener(OnAvatarImageChange);
        OnPointerExit(null);
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

    private void ShowUserProfile()
    {
        if (currentPlayerInfoCardId == null)
            currentPlayerInfoCardId = Resources.Load<StringVariable>("CurrentPlayerInfoCardId");
        currentPlayerInfoCardId.Set(model.userId);
    }
}