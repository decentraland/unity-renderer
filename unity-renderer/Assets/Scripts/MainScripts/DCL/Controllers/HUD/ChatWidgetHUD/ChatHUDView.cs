using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebGLIMEInput = WebGLSupport.WebGLInput;

namespace DCL.Social.Chat
{
    public class ChatHUDView : BaseComponentView, IChatHUDComponentView
    {
        [SerializeField] internal TMP_InputField inputField;
        [SerializeField] internal RectTransform chatEntriesContainer;
        [SerializeField] internal ScrollRect scrollRect;
        [SerializeField] internal GameObject messageHoverPanel;
        [SerializeField] internal GameObject messageHoverGotoPanel;
        [SerializeField] internal TextMeshProUGUI messageHoverText;
        [SerializeField] internal TextMeshProUGUI messageHoverGotoText;
        [SerializeField] internal UserContextMenu contextMenu;
        [SerializeField] internal UserContextConfirmationDialog confirmationDialog;
        [SerializeField] internal DefaultChatEntryFactory defaultChatEntryFactory;
        [SerializeField] internal PoolChatEntryFactory poolChatEntryFactory;
        [SerializeField] internal InputAction_Trigger nextChatInHistoryInput;
        [SerializeField] internal InputAction_Trigger previousChatInHistoryInput;
        [SerializeField] internal InputAction_Trigger nextMentionSuggestionInput;
        [SerializeField] internal InputAction_Trigger previousMentionSuggestionInput;
        [SerializeField] internal InputAction_Trigger closeMentionSuggestionsInput;
        [SerializeField] internal ChatMentionSuggestionComponentView chatMentionSuggestions;
        [SerializeField] internal WebGLIMEInput webGlImeInput;
        [SerializeField] private Model model;

        private readonly Dictionary<string, ChatEntry> entries = new ();
        private readonly ChatMessage currentMessage = new ();
        private readonly Dictionary<Action, UnityAction<string>> inputFieldSelectedListeners = new ();
        private readonly Dictionary<Action, UnityAction<string>> inputFieldUnselectedListeners = new ();

        private int updateLayoutDelayedFrames;
        private bool isSortingDirty;

        public event Action<string, int> OnMessageUpdated;
        public event Action OnOpenedContextMenu;

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
        public event Action<string> OnMentionSuggestionSelected;
        public event Action<ChatEntryModel> OnCopyMessageRequested;
        public event Action<ChatMessage> OnSendMessage;

        public int EntryCount => entries.Count;
        public IChatEntryFactory ChatEntryFactory { get; set; }
        public IComparer<ChatEntryModel> SortingStrategy { get; set; }
        public bool UseLegacySorting { private get; set; }

        public override void Awake()
        {
            base.Awake();
            inputField.onSubmit.AddListener(OnInputFieldSubmit);
            inputField.onSelect.AddListener(OnInputFieldSelect);
            inputField.onDeselect.AddListener(OnInputFieldDeselect);
            inputField.onValueChanged.AddListener(str => OnMessageUpdated?.Invoke(str, inputField.stringPosition));
            chatMentionSuggestions.OnEntrySubmit += model => OnMentionSuggestionSelected?.Invoke(model.userId);
            ChatEntryFactory ??= (IChatEntryFactory)poolChatEntryFactory ?? defaultChatEntryFactory;
            model.enableFadeoutMode = true;
            contextMenu.SetPassportOpenSource(true);

#if (UNITY_WEBGL && !UNITY_EDITOR)
            // WebGLInput plugin breaks many features:
            // @mentions navigation with ARROW keys
            // @mentions suggestions not hiding when pressing SPACE after @
            // chat windows toggling with ENTER key
            // chat history navigation with ARROW keys
            // probably more...
            // Key input events are not triggered anymore after focusing the input field
            // One of the main reasons is because the plugin replaces the TMP_InputField for a native web input,
            // overriding input handing, caret position handling, most of the input behaviour..
            // To mitigate the issues for most of the non-asian users,
            // we destroy the component because we assume that users dont need IME input
            // Users with asian languages will keep experiencing the issues
            SystemLanguage systemLanguage = Application.systemLanguage;

            if (systemLanguage is not (SystemLanguage.Chinese
                or SystemLanguage.ChineseSimplified
                or SystemLanguage.ChineseTraditional
                or SystemLanguage.Korean
                or SystemLanguage.Japanese))
                Destroy(webGlImeInput);
#endif
        }

        public override void OnEnable()
        {
            base.OnEnable();
            UpdateLayout();
            nextChatInHistoryInput.OnTriggered += HandleNextChatInHistoryInput;
            previousChatInHistoryInput.OnTriggered += HandlePreviousChatInHistoryInput;
            nextMentionSuggestionInput.OnTriggered += HandleNextMentionSuggestionInput;
            previousMentionSuggestionInput.OnTriggered += HandlePreviousMentionSuggestionInput;
            closeMentionSuggestionsInput.OnTriggered += HandleCloseMentionSuggestionsInput;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            nextChatInHistoryInput.OnTriggered -= HandleNextChatInHistoryInput;
            previousChatInHistoryInput.OnTriggered -= HandlePreviousChatInHistoryInput;
            nextMentionSuggestionInput.OnTriggered -= HandleNextMentionSuggestionInput;
            previousMentionSuggestionInput.OnTriggered -= HandlePreviousMentionSuggestionInput;
            closeMentionSuggestionsInput.OnTriggered -= HandleCloseMentionSuggestionsInput;
        }

        public void Update()
        {
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
                SetEntry(entry);
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
            inputField.SetTextWithoutNotify(text);
            inputField.MoveTextEnd(false);
            OnMessageUpdated?.Invoke(inputField.text, inputField.stringPosition);
        }

        public void ShowMentionSuggestions()
        {
            chatMentionSuggestions.Show();
        }

        public void SetMentionSuggestions(List<ChatMentionSuggestionModel> suggestions)
        {
            chatMentionSuggestions.Clear();
            chatMentionSuggestions.Set(suggestions);
            chatMentionSuggestions.SelectFirstEntry();
        }

        public void HideMentionSuggestions()
        {
            chatMentionSuggestions.Hide();
        }

        public void AddMentionToInputField(int fromIndex, int length, string userId, string userName)
        {
            string message = inputField.text;
            StringBuilder builder = new (message);

            SetInputFieldText(builder.Remove(fromIndex, length)
                                     .Insert(fromIndex, @$"@{userName} ")
                                     .ToString());
            FocusInputField();
        }

        public void AddTextIntoInputField(string text)
        {
            SetInputFieldText(string.IsNullOrEmpty(inputField.text) ? $"{text} " : $"{inputField.text.TrimEnd()} {text} ");
            FocusInputField();
        }

        public virtual void SetEntry(ChatEntryModel model, bool setScrollPositionToBottom = false)
        {
            if (entries.ContainsKey(model.messageId))
            {
                var chatEntry = entries[model.messageId];
                Populate(chatEntry, model);
                SetEntry(model.messageId, chatEntry, setScrollPositionToBottom);
            }
            else
            {
                var chatEntry = ChatEntryFactory.Create(model);
                Populate(chatEntry, model);
                chatEntry.ConfigureMentionLinkDetector(contextMenu);

                if (model.subType.Equals(ChatEntryModel.SubType.RECEIVED))
                    chatEntry.OnUserNameClicked += OnOpenContextMenu;

                chatEntry.OnTriggerHover += OnMessageTriggerHover;
                chatEntry.OnTriggerHoverGoto += OnMessageCoordinatesTriggerHover;
                chatEntry.OnCancelHover += OnMessageCancelHover;
                chatEntry.OnCancelGotoHover += OnMessageCancelGotoHover;
                chatEntry.OnCopyClicked += OnMessageCopy;

                SetEntry(model.messageId, chatEntry, setScrollPositionToBottom);
            }
        }

        public void SetEntry(string messageId, ChatEntry chatEntry, bool setScrollPositionToBottom = false)
        {
            Dock(chatEntry);
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

        protected void Populate(ChatEntry entry, ChatEntryModel model)
        {
            entry.Populate(model);
            entry.SetFadeout(this.model.enableFadeoutMode);
        }

        protected void Dock(ChatEntry entry)
        {
            entry.transform.SetParent(chatEntriesContainer, false);
        }

        private void SetFadeoutMode(bool enabled)
        {
            model.enableFadeoutMode = enabled;

            foreach (var entry in entries.Values)
                entry.SetFadeout(enabled && IsEntryVisible(entry));

            if (enabled)
                confirmationDialog.Hide();
        }

        private bool IsEntryVisible(ChatEntry entry)
        {
            int visibleCorners =
                (entry.transform as RectTransform).CountCornersVisibleFrom(scrollRect.viewport.transform as RectTransform);

            return visibleCorners > 0;
        }

        private void OnInputFieldSubmit(string message)
        {
            if (chatMentionSuggestions.IsVisible)
            {
                if (!inputField.wasCanceled)
                    chatMentionSuggestions.SubmitSelectedEntry();

                return;
            }

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
            OnOpenedContextMenu?.Invoke();
            chatEntry.DockContextMenu((RectTransform)contextMenu.transform);
            contextMenu.transform.parent = transform.parent;
            contextMenu.transform.SetAsLastSibling();
            contextMenu.Show(chatEntry.Model.senderId);
        }

        private void OnMessageTriggerHover(ChatEntry chatEntry)
        {
            if (contextMenu == null || contextMenu.isVisible)
                return;

            messageHoverText.text = chatEntry.HoverString;
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

            if (UseLegacySorting)
            {
                var entries = this.entries.Values.OrderBy(x => x.Model.timestamp).ToList();

                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i].transform.GetSiblingIndex() != i)
                        entries[i].transform.SetSiblingIndex(i);
                }

                return;
            }

            if (SortingStrategy == null) return;

            foreach (var entry in entries.Values.OrderBy(obj => obj.Model, SortingStrategy))
                entry.transform.SetAsLastSibling();
        }

        private void HandleNextChatInHistoryInput(DCLAction_Trigger action)
        {
            if (chatMentionSuggestions.IsVisible) return;
            OnNextChatInHistory?.Invoke();
        }

        private void HandlePreviousChatInHistoryInput(DCLAction_Trigger action)
        {
            if (chatMentionSuggestions.IsVisible) return;
            OnPreviousChatInHistory?.Invoke();
        }

        private void HandleNextMentionSuggestionInput(DCLAction_Trigger action)
        {
            if (!chatMentionSuggestions.IsVisible) return;
            chatMentionSuggestions.SelectNextEntry();
        }

        private void HandlePreviousMentionSuggestionInput(DCLAction_Trigger action)
        {
            if (!chatMentionSuggestions.IsVisible) return;
            chatMentionSuggestions.SelectPreviousEntry();
        }

        private void HandleCloseMentionSuggestionsInput(DCLAction_Trigger action)
        {
            if (!chatMentionSuggestions.IsVisible) return;
            chatMentionSuggestions.Hide();
        }

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

        private void OnMessageCopy(ChatEntry entry) =>
            OnCopyMessageRequested?.Invoke(entry.Model);

        [Serializable]
        private struct Model
        {
            public bool isInputFieldFocused;
            public string inputFieldText;
            public bool enableFadeoutMode;
            public ChatEntryModel[] entries;
        }
    }
}
