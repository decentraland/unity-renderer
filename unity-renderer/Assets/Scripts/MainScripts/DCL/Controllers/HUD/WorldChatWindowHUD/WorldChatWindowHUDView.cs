using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections;

public class WorldChatWindowHUDView : MonoBehaviour, IPointerClickHandler
{
    const string VIEW_PATH = "World Chat Window";
    static int ANIM_PROPERTY_SELECTED = Animator.StringToHash("Selected");

    public Button worldFilterButton;
    public Button pmFilterButton;
    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;
    public WorldChatWindowHUDController controller;

    Regex whisperRegex = new Regex(@"(?i)^\/(whisper|w) (\S*) ");
    Match whisperRegexMatch;

    TabMode tabMode = TabMode.WORLD;
    enum TabMode
    {
        WORLD,
        PRIVATE
    }

    public static WorldChatWindowHUDView Create(UnityAction onPrivateMessages, UnityAction onWorldMessages)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<WorldChatWindowHUDView>();
        view.Initialize(onPrivateMessages, onWorldMessages);
        return view;
    }

    void Awake()
    {
        chatHudView.inputField.onSubmit.AddListener(OnTextInputSubmit);
        chatHudView.inputField.onValueChanged.AddListener(OnTextInputValueChanged);
    }

    void OnEnable()
    {
        UpdateTabAnimators();
    }

    void UpdateTabAnimators()
    {
        switch (tabMode)
        {
            case TabMode.WORLD:
                pmFilterButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
                worldFilterButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
                break;
            case TabMode.PRIVATE:
                pmFilterButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
                worldFilterButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
                break;
        }
    }

    private void Initialize(UnityAction onPrivateMessages, UnityAction onWorldMessages)
    {
        this.closeButton.onClick.AddListener(Toggle);
        this.pmFilterButton.onClick.AddListener(() =>
           {
               onPrivateMessages.Invoke();
               tabMode = TabMode.PRIVATE;
               UpdateTabAnimators();
           });

        this.worldFilterButton.onClick.AddListener(() =>
           {
               onWorldMessages.Invoke();
               tabMode = TabMode.WORLD;
               UpdateTabAnimators();
           });
    }

    public bool isInPreview { get; private set; }

    public void DeactivatePreview()
    {
        group.alpha = 1;
        isInPreview = false;
    }

    public void ActivatePreview()
    {
        group.alpha = 0;
        isInPreview = true;
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
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
