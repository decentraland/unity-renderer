using System;
using DCL;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendEntryBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public class Model
    {
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
    [System.NonSerialized] public string userId;

    public Image playerBlockedImage;
    public Transform menuPositionReference;

    [SerializeField] protected internal TextMeshProUGUI playerNameText;
    [SerializeField] protected internal RawImage playerImage;
    [SerializeField] protected internal Button menuButton;
    [SerializeField] protected internal Image backgroundImage;
    [SerializeField] protected internal Sprite hoveredBackgroundSprite;
    [SerializeField] protected internal AudioEvent audioEventHover;
    protected internal Sprite unhoveredBackgroundSprite;

    public event System.Action<FriendEntryBase> OnMenuToggle;

    public virtual void Awake()
    {
        unhoveredBackgroundSprite = backgroundImage.sprite;
        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(() => OnMenuToggle?.Invoke(this));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.sprite = hoveredBackgroundSprite;
        menuButton.gameObject.SetActive(true);

        if (audioEventHover != null)
            audioEventHover.Play(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.sprite = unhoveredBackgroundSprite;
        menuButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        model.avatarSnapshotObserver?.AddListener(OnAvatarImageChange);
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

        this.model = model;
    }

    private void OnAvatarImageChange(Texture2D texture) { playerImage.texture = texture; }
}