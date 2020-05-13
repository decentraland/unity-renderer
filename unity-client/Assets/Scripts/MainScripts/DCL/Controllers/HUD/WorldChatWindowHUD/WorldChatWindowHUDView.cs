using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldChatWindowHUDView : MonoBehaviour, IPointerClickHandler
{
    const string VIEW_PATH = "World Chat Window";

    public Button worldFilterButton;
    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;
    public WorldChatWindowHUDController controller;

    Regex whisperRegex = new Regex(@"(?i)^\/(whisper|w) (\S*) ");
    Match whisperRegexMatch;

    public static WorldChatWindowHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<WorldChatWindowHUDView>();
        view.Initialize();
        return view;
    }

    void Awake()
    {
        chatHudView.inputField.onSubmit.AddListener(OnTextInputSubmit);
        chatHudView.inputField.onValueChanged.AddListener(OnTextInputValueChanged);
    }

    private void Initialize()
    {
        this.closeButton.onClick.AddListener(Toggle);
    }

    public bool isInPreview { get; private set; }

    public void DeactivatePreview()
    {
        group.alpha = 1;
        isInPreview = false;
        chatHudView.SetFadeoutMode(false);
    }

    public void ActivatePreview()
    {
        group.alpha = 0;
        isInPreview = true;
        chatHudView.SetFadeoutMode(true);
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            DeactivatePreview();
            chatHudView.ForceUpdateLayout();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DeactivatePreview();
    }

    public void OnTextInputValueChanged(string text)
    {
        if (!string.IsNullOrEmpty(controller.lastPrivateMessageReceivedSender) && text == "/r ")
        {
            chatHudView.inputField.text = $"/w {controller.lastPrivateMessageReceivedSender} ";
            chatHudView.inputField.caretPosition = chatHudView.inputField.text.Length - 1;
        }
    }

    public void OnTextInputSubmit(string text)
    {
        text = GetLastWhisperCommand(text);

        if (!string.IsNullOrEmpty(text))
        {
            StartCoroutine(WaitAndUpdateInputText(text));
        }
    }

    IEnumerator WaitAndUpdateInputText(string newText)
    {
        yield return null;

        chatHudView.inputField.text = newText;
        chatHudView.inputField.caretPosition = newText.Length - 1;
    }

    public string GetLastWhisperCommand(string inputString)
    {
        whisperRegexMatch = whisperRegex.Match(inputString);
        if (whisperRegexMatch.Success)
        {
            return whisperRegexMatch.Value;
        }

        return string.Empty;
    }
}
