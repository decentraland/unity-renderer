using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class ChatHUDView : MonoBehaviour
{
    static string VIEW_PATH = "Chat Widget";
    string ENTRY_PATH = "Chat Entry";

    public bool detectWhisper = true;
    public TMP_InputField inputField;
    public RectTransform chatEntriesContainer;

    public ScrollRect scrollRect;
    public ChatHUDController controller;
    public GameObject messageHoverPanel;
    public TextMeshProUGUI messageHoverText;
    public UserContextMenu contextMenu;
    public UserContextConfirmationDialog confirmationDialog;

    [NonSerialized] public List<ChatEntry> entries = new List<ChatEntry>();
    [NonSerialized] public List<DateSeparatorEntry> dateSeparators = new List<DateSeparatorEntry>();

    ChatMessage currentMessage = new ChatMessage();
    Regex whisperRegex = new Regex(@"(?i)^\/(whisper|w) (\S+)( *)(.*)");
    Match whisperRegexMatch;

    public event UnityAction<string> OnPressPrivateMessage;
    public event UnityAction<ChatMessage> OnSendMessage;

    public static ChatHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ChatHUDView>();
        return view;
    }

    public void Initialize(ChatHUDController controller, UnityAction<ChatMessage> OnSendMessage)
    {
        this.controller = controller;
        this.OnSendMessage += OnSendMessage;
        inputField.onSubmit.AddListener(OnInputFieldSubmit);
        inputField.onSelect.AddListener(OnInputFieldSelect);
        inputField.onDeselect.AddListener(OnInputFieldDeselect);
    }

    private void OnInputFieldSubmit(string message)
    {
        currentMessage.body = message;
        currentMessage.sender = UserProfile.GetOwnUserProfile().userId;
        currentMessage.messageType = ChatMessage.Type.NONE;
        currentMessage.recipient = string.Empty;

        if (detectWhisper && !string.IsNullOrWhiteSpace(message))
        {
            whisperRegexMatch = whisperRegex.Match(message);

            if (whisperRegexMatch.Success)
            {
                currentMessage.messageType = ChatMessage.Type.PRIVATE;
                currentMessage.recipient = whisperRegexMatch.Groups[2].Value;
                currentMessage.body = whisperRegexMatch.Groups[4].Value;
            }
        }

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

    public void ResetInputField()
    {
        inputField.text = string.Empty;
        inputField.caretColor = Color.white;
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

    bool enableFadeoutMode = false;

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
        var chatEntryGO = Instantiate(Resources.Load(ENTRY_PATH) as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();

        if (enableFadeoutMode && EntryIsVisible(chatEntry))
            chatEntry.SetFadeout(true);
        else
            chatEntry.SetFadeout(false);

        chatEntry.Populate(chatEntryModel);

        if (chatEntryModel.messageType == ChatMessage.Type.PRIVATE)
            chatEntry.OnPress += OnPressPrivateMessage;

        if (chatEntryModel.messageType == ChatMessage.Type.PUBLIC || chatEntryModel.messageType == ChatMessage.Type.PRIVATE)
            chatEntry.OnPressRightButton += OnOpenContextMenu;

        chatEntry.OnTriggerHover += OnMessageTriggerHover;
        chatEntry.OnCancelHover += OnMessageCancelHover;

        entries.Add(chatEntry);

        SortEntries();

        Utils.ForceUpdateLayout(transform as RectTransform, delayed: false);

        if (setScrollPositionToBottom)
            scrollRect.verticalNormalizedPosition = 0;
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

    public void OnMessageCancelHover()
    {
        messageHoverPanel.SetActive(false);
        messageHoverText.text = string.Empty;
    }

    public void SortEntries()
    {
        entries = entries.OrderBy(x => x.model.timestamp).ToList();

        int count = entries.Count;
        for (int i = 0; i < count; i++)
        {
            entries[i].transform.SetSiblingIndex(i);
        }
    }

    public void CleanAllEntries()
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
}