using DCL.Helpers;
using DCL.Configuration;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FriendsTabViewBase : MonoBehaviour, IPointerDownHandler
{
    [System.Serializable]
    public class EntryList
    {
        public string toggleTextPrefix;
        public GameObject toggleButton;
        public TextMeshProUGUI toggleText;
        public Transform container;
        private Dictionary<string, FriendEntryBase> entries = new Dictionary<string, FriendEntryBase>();

        public int Count()
        {
            return entries.Count;
        }

        public void Add(string userId, FriendEntryBase entry)
        {
            if (entries.ContainsKey(userId))
                return;

            entries.Add(userId, entry);

            entry.transform.SetParent(container, false);
            entry.transform.localScale = Vector3.one;

            UpdateToggle();
        }

        public FriendEntryBase Remove(string userId)
        {
            if (!entries.ContainsKey(userId))
                return null;

            var entry = entries[userId];

            entries.Remove(userId);

            UpdateToggle();

            return entry;
        }

        void UpdateToggle()
        {
            toggleText.text = $"{toggleTextPrefix} ({Count()})";
            toggleButton.SetActive(Count() != 0);
        }
    }

    [SerializeField] protected GameObject entryPrefab;
    [SerializeField] protected GameObject emptyListImage;

    protected RectTransform rectTransform;
    protected FriendsHUDView owner;

    public FriendsHUD_ContextMenu contextMenuPanel;
    public FriendsHUD_DialogBox confirmationDialog;

    protected Dictionary<string, FriendEntryBase> entries = new Dictionary<string, FriendEntryBase>();

    internal List<FriendEntryBase> GetAllEntries()
    {
        return entries.Values.ToList();
    }

    internal FriendEntryBase GetEntry(string userId)
    {
        if (!entries.ContainsKey(userId))
            return null;

        return entries[userId];
    }

    protected virtual void OnEnable()
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        rectTransform.ForceUpdateLayout();
    }

    protected virtual void OnDisable()
    {
        confirmationDialog.Hide();
        contextMenuPanel.Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject == null || eventData.pointerPressRaycast.gameObject.layer != PhysicsLayers.friendsHUDPlayerMenu)
            contextMenuPanel.Hide();
    }

    public virtual void Initialize(FriendsHUDView owner)
    {
        this.owner = owner;

        rectTransform = transform as RectTransform;

        contextMenuPanel.OnBlock += OnPressBlockButton;
        contextMenuPanel.OnDelete += OnPressDeleteButton;
        contextMenuPanel.OnPassport += OnPressPassportButton;
        contextMenuPanel.OnReport += OnPressReportButton;
    }

    protected virtual void OnPressReportButton(FriendEntryBase obj)
    {
    }

    protected virtual void OnPressPassportButton(FriendEntryBase obj)
    {
    }

    protected virtual void OnPressDeleteButton(FriendEntryBase obj)
    {
    }

    protected virtual void OnPressBlockButton(FriendEntryBase entry)
    {
        entry.model.blocked = !entry.model.blocked;
        entry.Populate(entry.model);
    }

    public virtual void CreateOrUpdateEntry(string userId, FriendEntryBase.Model model)
    {
        CreateEntry(userId);
        UpdateEntry(userId, model);
    }

    public virtual bool CreateEntry(string userId)
    {
        if (entries.ContainsKey(userId)) return false;

        if (emptyListImage.activeSelf)
            emptyListImage.SetActive(false);

        var entry = Instantiate(entryPrefab).GetComponent<FriendEntryBase>();
        entries.Add(userId, entry);

        entry.OnMenuToggle += (x) => { contextMenuPanel.Toggle(entry); };

        return true;
    }

    public virtual bool UpdateEntry(string userId, FriendEntryBase.Model model)
    {
        if (!entries.ContainsKey(userId)) return false;

        var entry = entries[userId];

        entry.Populate(model);
        entry.userId = userId;

        rectTransform.ForceUpdateLayout();
        return true;
    }

    public virtual bool RemoveEntry(string userId)
    {
        if (!entries.ContainsKey(userId)) return false;

        var entry = entries[userId];

        UnityEngine.Object.Destroy(entry.gameObject);
        entries.Remove(userId);

        if (entries.Count == 0)
            emptyListImage.SetActive(true);

        UpdateToggleTexts();

        rectTransform.ForceUpdateLayout();
        return true;
    }

    protected virtual void UpdateToggleTexts()
    {
        if (entries.Count == 0)
        {
            emptyListImage.SetActive(true);
        }
        else
        {
            emptyListImage.SetActive(false);
        }
    }
}
