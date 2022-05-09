using DCL;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChannelChatWindowView : MonoBehaviour, IPointerClickHandler, IChannelChatWindowView
{
    const string VIEW_PATH = "World Chat Window";

    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;

    public event Action OnDeactivatePreview;
    public event Action OnActivatePreview;
    public event Action OnClose;
    public event Action OnBack;

    private string lastInputText = string.Empty;

    public bool IsActive => gameObject.activeInHierarchy;
    public bool IsPreview { get; private set; }
    public IChatHUDComponentView ChatHUD => chatHudView;
    public RectTransform Transform => (RectTransform) transform;
    public string Channel => "#nearby";

    public static ChannelChatWindowView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ChannelChatWindowView>();
        return view;
    }

    private void Awake()
    {
        chatHudView.inputField.onValueChanged.AddListener(OnTextInputValueChanged);
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
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
        if (!this) return;
        if (gameObject)
            Destroy(gameObject);
    }

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);
    
    public void Setup(string channelId, string name, string description)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DeactivatePreview();
    }

    private void OnTextInputValueChanged(string text)
    {
        if (IsPreview)
            chatHudView.inputField.text = lastInputText;
        else
            lastInputText = chatHudView.inputField.text;
    }

    private IEnumerator WaitAndUpdateInputText(string newText)
    {
        yield return null;

        chatHudView.inputField.text = newText;
        chatHudView.inputField.caretPosition = newText.Length;
    }
}