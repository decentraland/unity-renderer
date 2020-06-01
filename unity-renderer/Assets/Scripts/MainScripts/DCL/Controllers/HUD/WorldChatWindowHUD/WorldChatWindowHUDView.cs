using System.Collections;
using DCL.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class WorldChatWindowHUDView : MonoBehaviour, IPointerClickHandler
{
    const string VIEW_PATH = "World Chat Window";

    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;
    public WorldChatWindowHUDController controller;
    public bool isInPreview { get; private set; }

    public event UnityAction OnDeactivatePreview;
    public event UnityAction OnClose;
    public UnityAction<ChatMessage> OnSendMessage;

    ChatMessage lastWhisperMessageSent;
    string lastInputText = string.Empty;

    public static WorldChatWindowHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<WorldChatWindowHUDView>();
        view.Initialize();
        return view;
    }

    void Awake()
    {
        chatHudView.OnSendMessage += ChatHUDView_OnSendMessage;
        chatHudView.inputField.onValueChanged.AddListener(OnTextInputValueChanged);
    }

    private void Initialize()
    {
        this.closeButton.onClick.AddListener(OnCloseButtonPressed);
    }

    public void OnCloseButtonPressed()
    {
        controller.SetVisibility(false);
        OnClose?.Invoke();
    }

    public void DeactivatePreview()
    {
        chatHudView.scrollRect.enabled = true;
        group.alpha = 1;
        isInPreview = false;
        chatHudView.SetFadeoutMode(false);
        OnDeactivatePreview?.Invoke();
    }

    public void ActivatePreview()
    {
        chatHudView.scrollRect.enabled = false;
        group.alpha = 0;
        isInPreview = true;
        chatHudView.SetFadeoutMode(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DeactivatePreview();
    }

    public void OnTextInputValueChanged(string text)
    {
        if (isInPreview)
            chatHudView.inputField.text = lastInputText;
        else
            lastInputText = chatHudView.inputField.text;
        

        if (!string.IsNullOrEmpty(controller.lastPrivateMessageReceivedSender) && text == "/r ")
        {
            chatHudView.inputField.text = $"/w {controller.lastPrivateMessageReceivedSender} ";
            chatHudView.inputField.caretPosition = chatHudView.inputField.text.Length;
        }
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

    IEnumerator WaitAndUpdateInputText(string newText)
    {
        yield return null;

        chatHudView.inputField.text = newText;
        chatHudView.inputField.caretPosition = newText.Length;
    }
}