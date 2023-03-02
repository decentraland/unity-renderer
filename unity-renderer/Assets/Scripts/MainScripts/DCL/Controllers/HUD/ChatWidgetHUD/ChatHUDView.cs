using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Chat.HUD;
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

    [SerializeField] protected TMP_InputField inputField;
    [SerializeField] protected RectTransform chatEntriesContainer;
    [SerializeField] protected ScrollRect scrollRect;
    [SerializeField] protected GameObject messageHoverPanel;
    [SerializeField] protected GameObject messageHoverGotoPanel;
    [SerializeField] protected TextMeshProUGUI messageHoverText;
    [SerializeField] protected TextMeshProUGUI messageHoverGotoText;
    [SerializeField] protected UserContextMenu contextMenu;
    [SerializeField] protected UserContextConfirmationDialog confirmationDialog;
    [SerializeField] protected DefaultChatEntryFactory defaultChatEntryFactory;
    [SerializeField] protected PoolChatEntryFactory poolChatEntryFactory;
    [SerializeField] protected InputAction_Trigger nextChatInHistoryInput;
    [SerializeField] protected InputAction_Trigger previousChatInHistoryInput;
    [SerializeField] private Model model;

    private readonly Dictionary<string, ChatEntry> entries = new ();
    private readonly ChatMessage currentMessage = new ();

    private readonly Dictionary<Action, UnityAction<string>> inputFieldSelectedListeners = new ();

    private readonly Dictionary<Action, UnityAction<string>> inputFieldUnselectedListeners = new ();

    private int updateLayoutDelayedFrames;
    private bool isSortingDirty;

    protected bool IsFadeoutModeEnabled => model.enableFadeoutMode;

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
            void Action(string s) =>
                value.Invoke();

            inputFieldSelectedListeners[value] = Action;
            inputField.onSelect.AddListener(Action);
        }

        remove
        {
            if (!inputFieldSelectedListeners.ContainsKey(value))
                return;

            inputField.onSelect.RemoveListener(inputFieldSelectedListeners[value]);
            inputFieldSelectedListeners.Remove(value);
        }
    }

    public event Action OnInputFieldDeselected
    {
        add
        {
            void Action(string s) =>
                value.Invoke();

            inputFieldUnselectedListeners[value] = Action;
            inputField.onDeselect.AddListener(Action);
        }

        remove
        {
            if (!inputFieldUnselectedListeners.ContainsKey(value))
                return;

            inputField.onDeselect.RemoveListener(inputFieldUnselectedListeners[value]);
            inputFieldUnselectedListeners.Remove(value);
        }
    }

    public event Action OnPreviousChatInHistory;
    public event Action OnNextChatInHistory;
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
        ChatEntryFactory ??= (IChatEntryFactory)poolChatEntryFactory ?? defaultChatEntryFactory;
        model.enableFadeoutMode = true;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UpdateLayout();
        nextChatInHistoryInput.OnTriggered += HandleNextChatInHistoryInput;
        previousChatInHistoryInput.OnTriggered += HandlePreviousChatInHistoryInput;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        nextChatInHistoryInput.OnTriggered -= HandleNextChatInHistoryInput;
        previousChatInHistoryInput.OnTriggered -= HandlePreviousChatInHistoryInput;
    }

    public override void Update()
    {
        base.Update();

        if (updateLayoutDelayedFrames > 0)
        {
            updateLayoutDelayedFrames--;

            if (updateLayoutDelayedFrames <= 0)
                chatEntriesContainer.ForceUpdateLayout(delayed: false);
        }

        if (isSortingDirty)
            SortEntriesImmediate();

        isSortingDirty = false;
    }

    public void ResetInputField(bool loseFocus = false)
    {
        inputField.text = string.Empty;
        inputField.caretColor = Color.white;

        if (loseFocus)
            UnfocusInputField();
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

    public void UnfocusInputField() =>
        EventSystem.current?.SetSelectedGameObject(null);

    public void SetInputFieldText(string text)
    {
        model.inputFieldText = text;
        inputField.text = text;
        inputField.MoveTextEnd(false);
    }

    private void SetFadeoutMode(bool enabled)
    {
        model.enableFadeoutMode = enabled;

        foreach (var entry in entries.Values)
            entry.SetFadeout(enabled && IsEntryVisible(entry));

        if (enabled)
            confirmationDialog.Hide();
    }

    public virtual void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false)
    {
        if (entries.ContainsKey(model.messageId))
        {
            var chatEntry = entries[model.messageId];
            chatEntry.SetFadeout(this.model.enableFadeoutMode);
            chatEntry.Populate(model);

            SetEntry(model.messageId, chatEntry, setScrollPositionToBottom);
        }
        else
        {
            var chatEntry = ChatEntryFactory.Create(model);
            chatEntry.SetFadeout(this.model.enableFadeoutMode);
            chatEntry.Populate(model);

            if (model.subType.Equals(ChatEntryModel.SubType.RECEIVED))
                chatEntry.OnUserNameClicked += OnOpenContextMenu;

            chatEntry.OnTriggerHover += OnMessageTriggerHover;
            chatEntry.OnTriggerHoverGoto += OnMessageCoordinatesTriggerHover;
            chatEntry.OnCancelHover += OnMessageCancelHover;
            chatEntry.OnCancelGotoHover += OnMessageCancelGotoHover;

            SetEntry(model.messageId, chatEntry, setScrollPositionToBottom);
        }
    }

    public virtual void SetEntry(string messageId, ChatEntry chatEntry, bool setScrollPositionToBottom = false)
    {
        chatEntry.transform.SetParent(chatEntriesContainer, false);
        entries[messageId] = chatEntry;

        SortEntries();
        UpdateLayout();

        if (setScrollPositionToBottom && scrollRect.verticalNormalizedPosition > 0)
            scrollRect.verticalNormalizedPosition = 0;
    }

    public void RemoveOldestEntry()
    {
        if (entries.Count <= 0) return;
        var firstEntry = GetFirstEntry();
        if (!firstEntry) return;
        entries.Remove(firstEntry.Model.messageId);
        ChatEntryFactory.Destroy(firstEntry);
        UpdateLayout();
    }

    public override void Hide(bool instant = false)
    {
        base.Hide(instant);

        if (contextMenu == null)
            return;

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
        foreach (var entry in entries.Values)
            ChatEntryFactory.Destroy(entry);

        entries.Clear();
        UpdateLayout();
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

        // we have to wait one frame to disengage the flow triggered by OnSendMessage
        // otherwise it crashes the application (WebGL only) due a TextMeshPro bug
        StartCoroutine(WaitThenTriggerSendMessage());
    }

    private IEnumerator WaitThenTriggerSendMessage()
    {
        yield return null;
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

    private void OnOpenContextMenu(ChatEntry chatEntry)
    {
        chatEntry.DockContextMenu((RectTransform)contextMenu.transform);
        contextMenu.transform.parent = transform.parent;
        contextMenu.transform.SetAsLastSibling();
        contextMenu.Show(chatEntry.Model.senderId);
    }

    private void OnMessageTriggerHover(ChatEntry chatEntry)
    {
        if (contextMenu == null || contextMenu.isVisible)
            return;

        messageHoverText.text = chatEntry.DateString;
        chatEntry.DockHoverPanel((RectTransform)messageHoverPanel.transform);
        messageHoverPanel.SetActive(true);
    }

    private void OnMessageCoordinatesTriggerHover(ChatEntry chatEntry, ParcelCoordinates parcelCoordinates)
    {
        messageHoverGotoText.text = $"{parcelCoordinates} INFO";
        chatEntry.DockHoverPanel((RectTransform)messageHoverGotoPanel.transform);
        messageHoverGotoPanel.SetActive(true);
    }

    private void OnMessageCancelGotoHover()
    {
        messageHoverGotoPanel.SetActive(false);
        messageHoverGotoText.text = string.Empty;
    }

    private void SortEntries() =>
        isSortingDirty = true;

    private void SortEntriesImmediate()
    {
        if (this.entries.Count <= 0) return;

        var entries = this.entries.Values.OrderBy(x => x.Model.timestamp).ToList();

        for (var i = 0; i < entries.Count; i++)
        {
            if (entries[i].transform.GetSiblingIndex() != i)
                entries[i].transform.SetSiblingIndex(i);
        }
    }

    private void HandleNextChatInHistoryInput(DCLAction_Trigger action) =>
        OnNextChatInHistory?.Invoke();

    private void HandlePreviousChatInHistoryInput(DCLAction_Trigger action) =>
        OnPreviousChatInHistory?.Invoke();

    private void UpdateLayout()
    {
        // we have to wait several frames before updating the layout
        // every message entry waits for 3 frames before updating its own layout
        // we gotta force the layout updating after that
        // TODO: simplify this change to a bool when we update to a working TextMeshPro version
        updateLayoutDelayedFrames = 4;
    }

    private ChatEntry GetFirstEntry()
    {
        ChatEntry firstEntry = null;

        for (var i = 0; i < chatEntriesContainer.childCount; i++)
        {
            var firstChildTransform = chatEntriesContainer.GetChild(i);
            if (!firstChildTransform) continue;
            var entry = firstChildTransform.GetComponent<ChatEntry>();
            if (!entry) continue;
            firstEntry = entry;
            break;
        }

        return firstEntry;
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
