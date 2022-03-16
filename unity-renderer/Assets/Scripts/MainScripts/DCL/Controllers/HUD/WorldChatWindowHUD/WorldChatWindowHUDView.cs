using System;
using System.Collections;
using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldChatWindowHUDView : MonoBehaviour, IPointerClickHandler, IWorldChatComponentView
{
    const string VIEW_PATH = "World Chat Window";

    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;

    public event Action OnDeactivatePreview;
    public event Action OnActivatePreview;
    public event Action OnClose;
    public event Action<string> OnMessageUpdated;
    public event Action<ChatMessage> OnSendMessage;

    private ChatMessage lastWhisperMessageSent;
    private string lastInputText = string.Empty;

    public bool IsActive => gameObject.activeInHierarchy;
    public bool IsPreview { get; private set; }
    public bool IsInputFieldFocused => chatHudView.inputField.isFocused;
    public IChatHUDComponentView ChatHUD => chatHudView;
    public RectTransform Transform => (RectTransform) transform;

    public static WorldChatWindowHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<WorldChatWindowHUDView>();
        return view;
    }

    private void Awake()
    {
        chatHudView.OnSendMessage += ChatHUDView_OnSendMessage;
        chatHudView.inputField.onValueChanged.AddListener(OnTextInputValueChanged);
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
    }

    public void SetInputField(string text)
    {
        chatHudView.inputField.text = text;
        chatHudView.inputField.MoveTextEnd(false);
    }

    public void DeactivatePreview()
    {
        chatHudView.scrollRect.enabled = true;
        group.alpha = 1;
        DataStore.i.HUDs.chatInputVisible.Set(true);
        IsPreview = false;
        chatHudView.SetFadeoutMode(false);
        chatHudView.SetGotoPanelStatus(false);
        OnDeactivatePreview?.Invoke();
    }

    public void ActivatePreview()
    {
        chatHudView.scrollRect.enabled = false;
        group.alpha = 0;
        DataStore.i.HUDs.chatInputVisible.Set(false);
        IsPreview = true;
        chatHudView.SetFadeoutMode(true);
        OnActivatePreview?.Invoke();
    }

    public void Dispose()
    {
        if (gameObject)
            Destroy(gameObject);
    }

    public void Hide() => gameObject.SetActive(false);

    public void ResetInputField(bool loseFocus = false)
    {
        chatHudView.ResetInputField();
        if (loseFocus)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void FocusInputField() => chatHudView.FocusInputField();

    public void Show() => gameObject.SetActive(true);

    public void OnPointerClick(PointerEventData eventData)
    {
        DeactivatePreview();
    }

    public void OnTextInputValueChanged(string text)
    {
        if (IsPreview)
            chatHudView.inputField.text = lastInputText;
        else
            lastInputText = chatHudView.inputField.text;

        OnMessageUpdated?.Invoke(text);
    }

    public void ChatHUDView_OnSendMessage(ChatMessage message)
    {
        if (message.messageType == ChatMessage.Type.PRIVATE && !string.IsNullOrEmpty(message.body))
            lastWhisperMessageSent = message;
        else
            lastWhisperMessageSent = null;

        if (lastWhisperMessageSent != null)
            StartCoroutine(WaitAndUpdateInputText($"/w {lastWhisperMessageSent.recipient} "));
        else
            StartCoroutine(WaitAndUpdateInputText(string.Empty));

        OnSendMessage?.Invoke(message);
    }

    private IEnumerator WaitAndUpdateInputText(string newText)
    {
        yield return null;

        chatHudView.inputField.text = newText;
        chatHudView.inputField.caretPosition = newText.Length;
    }
}