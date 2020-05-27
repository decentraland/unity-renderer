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
        public Sprite avatarImage;
        public bool blocked;

        public event System.Action<Sprite> OnSpriteUpdateEvent;
        public void OnSpriteUpdate(Sprite sprite)
        {
            OnSpriteUpdateEvent?.Invoke(sprite);
        }
    }

    public Model model { get; private set; } = new Model();
    [System.NonSerialized] public string userId;

    public Image playerBlockedImage;
    public Transform menuPositionReference;

    [SerializeField] protected internal TextMeshProUGUI playerNameText;
    [SerializeField] protected internal Image playerImage;
    [SerializeField] protected internal Button menuButton;
    [SerializeField] protected internal Image backgroundImage;
    [SerializeField] protected internal Sprite hoveredBackgroundSprite;
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
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.sprite = unhoveredBackgroundSprite;
        menuButton.gameObject.SetActive(false);
    }

    protected virtual void OnDisable()
    {
        OnPointerExit(null);

        model.OnSpriteUpdateEvent -= OnAvatarImageChange;
    }

    public virtual void Populate(Model model)
    {
        this.model = model;

        if (playerNameText.text != model.userName)
            playerNameText.text = model.userName;

        if (model.avatarImage == null)
        {
            model.OnSpriteUpdateEvent -= OnAvatarImageChange;
            model.OnSpriteUpdateEvent += OnAvatarImageChange;
        }

        if (model.avatarImage != playerImage.sprite)
            playerImage.sprite = model.avatarImage;

        playerBlockedImage.enabled = model.blocked;
    }

    private void OnAvatarImageChange(Sprite sprite)
    {
        playerImage.sprite = sprite;
        model.avatarImage = sprite;
    }
}
