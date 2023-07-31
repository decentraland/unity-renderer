using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Social.Chat
{
    public class ChatInputContextualMenuView : BaseComponentView<ChatInputContextualMenuModel>,
        IPointerClickHandler, IChatInputContextMenuView
    {
        public static readonly BaseList<ChatInputContextualMenuView> instances = new ();

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private GameObject menuContainer;
        [SerializeField] private Button pasteButton;
        [SerializeField] private Button copyButton;
        [SerializeField] private bool selfDestroy;

        private string selectedText;
        private readonly List<RaycastResult> clickRaycastResultsBuffer = new ();

        public event Action<string> OnSelectionChanged;
        public event Action OnShowRequested;
        public event Action OnPasteRequested;
        public event Action<string> OnCopyRequested;

        public override void Awake()
        {
            base.Awake();

            inputField.onTextSelection.AddListener((str, i, i2) =>
            {
                int from = Mathf.Min(i, i2);
                int until = Mathf.Max(i, i2);
                selectedText = str?.Substring(from, until - from);
                OnSelectionChanged?.Invoke(str);
            });
            inputField.onEndTextSelection.AddListener((str, i, i2) =>
            {
                int from = Mathf.Min(i, i2);
                int until = Mathf.Max(i, i2);
                selectedText = str?.Substring(from, until - from);
                OnSelectionChanged?.Invoke(str);
            });
            pasteButton.onClick.AddListener(() => OnPasteRequested?.Invoke());
            copyButton.onClick.AddListener(() => OnCopyRequested?.Invoke(selectedText));

            instances.Add(this);
        }

        public override void Dispose()
        {
            instances.Remove(this);

            if (!selfDestroy) return;

            base.Dispose();
        }

        private void Update()
        {
            HideIfClickedOutside();
        }

        public override void RefreshControl()
        {
            pasteButton.interactable = model.IsPasteButtonEnabled;
            copyButton.interactable = model.IsCopyButtonEnabled;
        }

        public override void Show(bool instant = false)
        {
            menuContainer.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
            menuContainer.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData is not { button: PointerEventData.InputButton.Right }) return;
            if (menuContainer.activeSelf) return;

            menuContainer.transform.position = eventData.position + (Vector2.up * 20);
            OnShowRequested?.Invoke();
        }

        public void Paste(string text)
        {
            int position = 0;

            if (inputField.selectionStringAnchorPosition != -1
                && inputField.selectionStringFocusPosition != -1)
            {
                int from = Mathf.Min(inputField.selectionStringFocusPosition, inputField.selectionStringAnchorPosition);
                int until = Mathf.Max(inputField.selectionStringFocusPosition, inputField.selectionStringAnchorPosition);

                string result = inputField.text.Remove(from, until - from);

                result = result.Insert(from, text);

                inputField.text = result;
                position = from + text.Length;
            }
            else if (inputField.stringPosition != -1)
            {
                inputField.text = inputField.text.Insert(inputField.stringPosition, text);
                position = inputField.text.Length;
            }
            else
            {
                inputField.text += text;
                position = inputField.text.Length;
            }

            inputField.stringPosition = position;
            inputField.Select();
        }

        private void HideIfClickedOutside()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            clickRaycastResultsBuffer.Clear();
            EventSystem.current.RaycastAll(pointerEventData, clickRaycastResultsBuffer);

            if (clickRaycastResultsBuffer.All(result => result.gameObject != copyButton.gameObject
                                                        && result.gameObject != pasteButton.gameObject))
                Hide();
        }
    }
}
