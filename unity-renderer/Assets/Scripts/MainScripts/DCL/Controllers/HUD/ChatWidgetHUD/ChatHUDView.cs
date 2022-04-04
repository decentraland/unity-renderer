using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHUDView : MonoBehaviour, IChatHUDComponentView
{
    private const string VIEW_PATH = "Chat Widget";
    private const string ENTRY_PATH = "Chat Entry";
    private const int MAX_CONTINUOUS_MESSAGES = 6;
    private const int MIN_MILLISECONDS_BETWEEN_MESSAGES = 1500;
    private const int TEMPORARILY_MUTE_MINUTES = 10;

    public bool detectWhisper = true;
    public TMP_InputField inputField;
    public RectTransform chatEntriesContainer;

    public ScrollRect scrollRect;
    public GameObject messageHoverPanel;
    public GameObject messageHoverGotoPanel;
    public TextMeshProUGUI messageHoverText;
    public TextMeshProUGUI messageHoverGotoText;
    public UserContextMenu contextMenu;
    public UserContextConfirmationDialog confirmationDialog;

    [NonSerialized] protected List<ChatEntry> entries = new List<ChatEntry>();
    [NonSerialized] protected List<DateSeparatorEntry> dateSeparators = new List<DateSeparatorEntry>();

    private readonly ChatMessage currentMessage = new ChatMessage();
    private readonly List<ChatEntry.Model> lastMessages = new List<ChatEntry.Model>();
    private readonly Dictionary<string, ulong> temporarilyMutedSenders = new Dictionary<string, ulong>();

    private readonly Dictionary<Action, UnityAction<string>> inputFieldListeners =
        new Dictionary<Action, UnityAction<string>>();

    private bool enableFadeoutMode;

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

    public static ChatHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ChatHUDView>();
        return view;
    }

    private void Awake()
    {
        inputField.onSubmit.AddListener(OnInputFieldSubmit);
        inputField.onSelect.AddListener(OnInputFieldSelect);
        inputField.onDeselect.AddListener(OnInputFieldDeselect);
        inputField.onValueChanged.AddListener(str => OnMessageUpdated?.Invoke(str));
    }

    private void OnInputFieldSubmit(string message)
    {
        currentMessage.body = message;
        currentMessage.sender = UserProfile.GetOwnUserProfile().userId;
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

    public void ResetInputField(bool loseFocus = false)
    {
        inputField.text = string.Empty;
        inputField.caretColor = Color.white;
        if (loseFocus)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void OnEnable()
    {
        Utils.ForceUpdateLayout(transform as RectTransform);
    }

    public void FocusInputField()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void SetInputFieldText(string text)
    {
        inputField.text = text;
        inputField.MoveTextEnd(false);
    }

    bool EntryIsVisible(ChatEntry entry)
    {
        int visibleCorners =
            (entry.transform as RectTransform).CountCornersVisibleFrom(scrollRect.viewport.transform as RectTransform);
        return visibleCorners > 0;
    }

    public void SetFadeoutMode(bool enabled)
    {
        enableFadeoutMode = enabled;

        for (int i = 0; i < entries.Count; i++)
        {
            ChatEntry entry = entries[i];

            if (enabled)
            {
                entry.SetFadeout(EntryIsVisible(entry));
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

    public virtual void AddEntry(ChatEntry.Model chatEntryModel, bool setScrollPositionToBottom = false)
    {
        if (IsSpamming(chatEntryModel.senderName))
            return;

        var chatEntryGO = Instantiate(Resources.Load(ENTRY_PATH) as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();

        if (enableFadeoutMode && EntryIsVisible(chatEntry))
            chatEntry.SetFadeout(true);
        else
            chatEntry.SetFadeout(false);

        chatEntry.Populate(chatEntryModel);

        if (chatEntryModel.messageType == ChatMessage.Type.PUBLIC ||
            chatEntryModel.messageType == ChatMessage.Type.PRIVATE)
            chatEntry.OnPressRightButton += OnOpenContextMenu;

        chatEntry.OnTriggerHover += OnMessageTriggerHover;
        chatEntry.OnTriggerHoverGoto += OnMessageCoordinatesTriggerHover;
        chatEntry.OnCancelHover += OnMessageCancelHover;
        chatEntry.OnCancelGotoHover += OnMessageCancelGotoHover;

        entries.Add(chatEntry);

        SortEntries();

        Utils.ForceUpdateLayout(chatEntry.transform as RectTransform, delayed: false);

        if (setScrollPositionToBottom && scrollRect.verticalNormalizedPosition > 0)
            scrollRect.verticalNormalizedPosition = 0;

        if (string.IsNullOrEmpty(chatEntryModel.senderId))
            return;

        if (lastMessages.Count == 0)
        {
            lastMessages.Add(chatEntryModel);
        }
        else if (lastMessages[lastMessages.Count - 1].senderName == chatEntryModel.senderName)
        {
            if (MessagesSentTooFast(lastMessages[lastMessages.Count - 1].timestamp, chatEntryModel.timestamp))
            {
                lastMessages.Add(chatEntryModel);

                if (lastMessages.Count == MAX_CONTINUOUS_MESSAGES)
                {
                    temporarilyMutedSenders.Add(chatEntryModel.senderName, chatEntryModel.timestamp);
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

    public void Dispose() => Destroy(gameObject);

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

    bool MessagesSentTooFast(ulong oldMessageTimeStamp, ulong newMessageTimeStamp)
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

    private void OnOpenContextMenu(ChatEntry chatEntry)
    {
        contextMenu.transform.position = chatEntry.contextMenuPositionReference.position;
        contextMenu.transform.parent = this.transform;
        contextMenu.Show(chatEntry.model.senderId);
    }

    protected virtual void OnMessageTriggerHover(ChatEntry chatEntry)
    {
        if (contextMenu == null || contextMenu.isVisible)
            return;

        messageHoverText.text = chatEntry.messageLocalDateTime;
        messageHoverPanel.transform.position = chatEntry.hoverPanelPositionReference.position;
        messageHoverPanel.SetActive(true);
    }

    protected virtual void OnMessageCoordinatesTriggerHover(ChatEntry chatEntry, ParcelCoordinates parcelCoordinates)
    {
        messageHoverGotoText.text = $"{parcelCoordinates.ToString()} INFO";
        messageHoverGotoPanel.transform.position = new Vector3(Input.mousePosition.x,
            chatEntry.hoverPanelPositionReference.transform.position.y,
            chatEntry.hoverPanelPositionReference.transform.position.z);
        messageHoverGotoPanel.SetActive(true);
    }

    public void OnMessageCancelHover()
    {
        messageHoverPanel.SetActive(false);
        messageHoverText.text = string.Empty;
    }

    public void OnMessageCancelGotoHover()
    {
        messageHoverGotoPanel.SetActive(false);
        messageHoverGotoText.text = string.Empty;
    }

    public void SortEntries()
    {
        entries = entries.OrderBy(x => x.model.timestamp).ToList();

        int count = entries.Count;
        for (int i = 0; i < count; i++)
        {
            if (entries[i].transform.GetSiblingIndex() != i)
            {
                entries[i].transform.SetSiblingIndex(i);
                Utils.ForceUpdateLayout(entries[i].transform as RectTransform, delayed: false);
            }
        }
    }

    public void ClearAllEntries()
    {
        foreach (var entry in entries)
        {
            Destroy(entry.gameObject);
        }

        entries.Clear();

        foreach (DateSeparatorEntry separator in dateSeparators)
        {
            Destroy(separator.gameObject);
        }

        dateSeparators.Clear();
    }

    public void SetGotoPanelStatus(bool isActive)
    {
        DataStore.i.HUDs.gotoPanelVisible.Set(isActive);
    }
}