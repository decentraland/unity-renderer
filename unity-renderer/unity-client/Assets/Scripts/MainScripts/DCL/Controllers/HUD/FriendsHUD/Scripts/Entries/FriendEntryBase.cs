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
        public Texture2D avatarImage;
        public bool blocked;

        public event System.Action<Texture2D> OnTextureUpdateEvent;
        public void OnSpriteUpdate(Texture2D texture)
        {
            avatarImage = texture;
            OnTextureUpdateEvent?.Invoke(texture);
        }
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

    protected virtual void OnDisable()
    {
        OnPointerExit(null);
    }

    protected void OnDestroy()
    {
        model.OnTextureUpdateEvent -= OnAvatarImageChange;
    }

    public virtual void Populate(Model model)
    {
        this.model = model;

        if (playerNameText.text != model.userName)
            playerNameText.text = model.userName;

        if (model.avatarImage == null)
        {
            model.OnTextureUpdateEvent -= OnAvatarImageChange;
            model.OnTextureUpdateEvent += OnAvatarImageChange;
        }

        if (model.avatarImage != playerImage.texture)
            OnAvatarImageChange(model.avatarImage);

        playerBlockedImage.enabled = model.blocked;
    }

    private void OnAvatarImageChange(Texture2D texture)
    {
        playerImage.texture = texture;
    }
}
