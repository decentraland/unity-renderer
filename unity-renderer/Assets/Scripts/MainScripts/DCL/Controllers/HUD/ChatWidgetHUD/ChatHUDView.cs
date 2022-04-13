using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHUDView : BaseComponentView, IChatHUDComponentView
{
    private const string VIEW_PATH = "SocialBarV1/Chat";
    private const int MAX_CONTINUOUS_MESSAGES = 6;
    private const int MIN_MILLISECONDS_BETWEEN_MESSAGES = 1500;
    private const int TEMPORARILY_MUTE_MINUTES = 10;

    public TMP_InputField inputField;
    public RectTransform chatEntriesContainer;
    public ScrollRect scrollRect;
    public GameObject messageHoverPanel;
    public GameObject messageHoverGotoPanel;
    public TextMeshProUGUI messageHoverText;
    public TextMeshProUGUI messageHoverGotoText;
    public UserContextMenu contextMenu;
    public UserContextConfirmationDialog confirmationDialog;
    [SerializeField] private DefaultChatEntryFactory defaultChatEntryFactory;
    [SerializeField] private Model model;
    
    [NonSerialized] protected List<ChatEntry> entries = new List<ChatEntry>();

    private readonly ChatMessage currentMessage = new ChatMessage();
    private readonly List<ChatEntryModel> lastMessages = new List<ChatEntryModel>();
    private readonly Dictionary<string, ulong> temporarilyMutedSenders = new Dictionary<string, ulong>();
    private readonly Dictionary<Action, UnityAction<string>> inputFieldListeners =
        new Dictionary<Action, UnityAction<string>>();

    public event Action<string> OnMessageUpdated;

    public event Action OnShowMenu
    {
        add
        {
            if (contextMenu != null)
                contextMenu.OnShowMenu += value;
        }
        remove
        {
            if (contextMenu != null)
                contextMenu.OnShowMenu -= value;
        }
    }

    public event Action OnInputFieldSelected
    {
        add
        {
            if (value == null) return;
            void Action(string s) => value.Invoke();
            inputFieldListeners[value] = Action;
            inputField.onSelect.AddListener(Action);
        }
        remove
        {
            if (value == null) return;
            if (!inputFieldListeners.ContainsKey(value)) return;
            inputField.onSelect.RemoveListener(inputFieldListeners[value]);
            inputFieldListeners.Remove(value);
        }
    }

    public event Action<ChatMessage> OnSendMessage;

    public int EntryCount => entries.Count;
    public IChatEntryFactory ChatEntryFactory { get; set; }

    public static ChatHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ChatHUDView>();
        return view;
    }

    public override void Awake()
    {
        base.Awake();
        inputField.onSubmit.AddListener(OnInputFieldSubmit);
        inputField.onSelect.AddListener(OnInputFieldSelect);
        inputField.onDeselect.AddListener(OnInputFieldDeselect);
        inputField.onValueChanged.AddListener(str => OnMessageUpdated?.Invoke(str));
        ChatEntryFactory ??= defaultChatEntryFactory;
    }
    
    public override void OnEnable()
    {
        base.OnEnable();
        Utils.ForceUpdateLayout(transform as RectTransform);
    }

    public void ResetInputField(bool loseFocus = false)
    {
        inputField.text = string.Empty;
        inputField.caretColor = Color.white;
        if (loseFocus)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public override void RefreshControl()
    {
        if (model.isInputFieldFocused)
            FocusInputField();
        SetInputFieldText(model.inputFieldText);
        SetFadeoutMode(model.enableFadeoutMode);
        ClearAllEntries();
        foreach (var entry in model.entries)
            AddEntry(entry);
    }

    public void FocusInputField()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void SetInputFieldText(string text)
    {
        model.inputFieldText = text;
        inputField.text = text;
        inputField.MoveTextEnd(false);
    }

    public void SetFadeoutMode(bool enabled)
    {
        model.enableFadeoutMode = enabled;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            if (enabled)
            {
                entry.SetFadeout(IsEntryVisible(entry));
            }
            else
            {
                entry.SetFadeout(false);
            }
        }

        if (enabled)
        {
            confirmationDialog.Hide();
        }
    }

    public virtual void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false)
    {
        if (IsSpamming(model.senderName)) return;

        var chatEntry = ChatEntryFactory.Create(model);
        chatEntry.transform.SetParent(chatEntriesContainer, false);

        if (this.model.enableFadeoutMode && IsEntryVisible(chatEntry))
            chatEntry.SetFadeout(true);
        else
            chatEntry.SetFadeout(false);

        chatEntry.Populate(model);

        if (model.messageType == ChatMessage.Type.PUBLIC
            || model.messageType == ChatMessage.Type.PRIVATE)
            chatEntry.OnPressRightButton += OnOpenContextMenu;

        chatEntry.OnTriggerHover += OnMessageTriggerHover;
        chatEntry.OnTriggerHoverGoto += OnMessageCoordinatesTriggerHover;
        chatEntry.OnCancelHover += OnMessageCancelHover;
        chatEntry.OnCancelGotoHover += OnMessageCancelGotoHover;

        entries.Add(chatEntry);

        SortEntries();

        Utils.ForceUpdateLayout(chatEntriesContainer, delayed: false);

        if (setScrollPositionToBottom && scrollRect.verticalNormalizedPosition > 0)
            scrollRect.verticalNormalizedPosition = 0;

        if (string.IsNullOrEmpty(model.senderId)) return;

        UpdateSpam(model);
    }

    public void RemoveFirstEntry()
    {
        if (entries.Count <= 0) return;
        Destroy(entries[0].gameObject);
        entries.Remove(entries[0]);
    }

    public void Hide()
    {
        if (contextMenu == null) return;
        contextMenu.Hide();
        confirmationDialog.Hide();
    }

    public void OnMessageCancelHover()
    {
        messageHoverPanel.SetActive(false);
        messageHoverText.text = string.Empty;
    }

    public virtual void ClearAllEntries()
    {
        foreach (var entry in entries)
            Destroy(entry.gameObject);
        entries.Clear();
    }

    public void SetGotoPanelStatus(bool isActive)
    {
        DataStore.i.HUDs.gotoPanelVisible.Set(isActive);
    }

    private bool IsEntryVisible(ChatEntry entry)
    {
        int visibleCorners =
            (entry.transform as RectTransform).CountCornersVisibleFrom(scrollRect.viewport.transform as RectTransform);
        return visibleCorners > 0;
    }

    private void OnInputFieldSubmit(string message)
    {
        currentMessage.body = message;
        currentMessage.sender = string.Empty;
        currentMessage.messageType = ChatMessage.Type.NONE;
        currentMessage.recipient = string.Empty;

        // A TMP_InputField is automatically marked as 'wasCanceled' when the ESC key is pressed
        if (inputField.wasCanceled)
            currentMessage.body = string.Empty;

        OnSendMessage?.Invoke(currentMessage);
    }

    private void OnInputFieldSelect(string message)
    {
        AudioScriptableObjects.inputFieldFocus.Play(true);
    }

    private void OnInputFieldDeselect(string message)
    {
        AudioScriptableObjects.inputFieldUnfocus.Play(true);
    }

    private void UpdateSpam(ChatEntryModel model)
    {
        if (lastMessages.Count == 0)
        {
            lastMessages.Add(model);
        }
        else if (lastMessages[lastMessages.Count - 1].senderName == model.senderName)
        {
            if (MessagesSentTooFast(lastMessages[lastMessages.Count - 1].timestamp, model.timestamp))
            {
                lastMessages.Add(model);

                if (lastMessages.Count == MAX_CONTINUOUS_MESSAGES)
                {
                    temporarilyMutedSenders.Add(model.senderName, model.timestamp);
                    lastMessages.Clear();
                }
            }
            else
            {
                lastMessages.Clear();
            }
        }
        else
        {
            lastMessages.Clear();
        }
    }

    private bool MessagesSentTooFast(ulong oldMessageTimeStamp, ulong newMessageTimeStamp)
    {
        DateTime oldDateTime = CreateBaseDateTime().AddMilliseconds(oldMessageTimeStamp).ToLocalTime();
        DateTime newDateTime = CreateBaseDateTime().AddMilliseconds(newMessageTimeStamp).ToLocalTime();
        return (newDateTime - oldDateTime).TotalMilliseconds < MIN_MILLISECONDS_BETWEEN_MESSAGES;
    }

    private bool IsSpamming(string senderName)
    {
        if (string.IsNullOrEmpty(senderName))
            return false;

        bool isSpamming = false;

        if (temporarilyMutedSenders.ContainsKey(senderName))
        {
            DateTime muteTimestamp =
                CreateBaseDateTime().AddMilliseconds(temporarilyMutedSenders[senderName]).ToLocalTime();
            if ((DateTime.Now - muteTimestamp).Minutes < TEMPORARILY_MUTE_MINUTES)
                isSpamming = true;
            else
                temporarilyMutedSenders.Remove(senderName);
        }

        return isSpamming;
    }

    private DateTime CreateBaseDateTime()
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    }

    private void OnOpenContextMenu(DefaultChatEntry chatEntry)
    {
        contextMenu.transform.position = chatEntry.contextMenuPositionReference.position;
        contextMenu.transform.parent = transform;
        contextMenu.Show(chatEntry.Model.senderId);
    }

    protected virtual void OnMessageTriggerHover(DefaultChatEntry chatEntry)
    {
        if (contextMenu == null || contextMenu.isVisible)
            return;

        messageHoverText.text = chatEntry.messageLocalDateTime;
        messageHoverPanel.transform.position = chatEntry.hoverPanelPositionReference.position;
        messageHoverPanel.SetActive(true);
    }

    protected virtual void OnMessageCoordinatesTriggerHover(DefaultChatEntry chatEntry, ParcelCoordinates parcelCoordinates)
    {
        messageHoverGotoText.text = $"{parcelCoordinates} INFO";
        var hoverGoToPanelPosition = chatEntry.hoverPanelPositionReference.transform.position;
        messageHoverGotoPanel.transform.position = new Vector3(Input.mousePosition.x,
            hoverGoToPanelPosition.y,
            hoverGoToPanelPosition.z);
        messageHoverGotoPanel.SetActive(true);
    }

    private void OnMessageCancelGotoHover()
    {
        messageHoverGotoPanel.SetActive(false);
        messageHoverGotoText.text = string.Empty;
    }

    private void SortEntries()
    {
        entries = entries.OrderBy(x => x.Model.timestamp).ToList();

        int count = entries.Count;
        for (int i = 0; i < count; i++)
        {
            if (entries[i].transform.GetSiblingIndex() != i)
            {
                entries[i].transform.SetSiblingIndex(i);
            }
        }
    }

    [Serializable]
    private struct Model
    {
        public bool isInputFieldFocused;
        public string inputFieldText;
        public bool enableFadeoutMode;
        public ChatEntryModel[] entries;
    }
}