using System.Collections.Generic;
using DCL;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    private const string VIEW_PATH = "Taskbar";

    [Header("Taskbar Animation")] [SerializeField]
    internal ShowHideAnimator taskbarAnimator;

    [Header("Left Side Config")]
    [SerializeField] internal RectTransform leftWindowContainer;
    [SerializeField] internal RectTransform altSectionContainer;

    [SerializeField] internal ShowHideAnimator leftWindowContainerAnimator;
    [SerializeField] internal LayoutGroup leftWindowContainerLayout;
    [SerializeField] internal TaskbarButton chatButton;
    [SerializeField] internal TaskbarButton friendsButton;
    [SerializeField] internal TaskbarButton emotesButton;
    [SerializeField] internal TaskbarButton voiceChatButton;
    [SerializeField] internal GameObject experiencesContainer;
    [SerializeField] internal TaskbarButton experiencesButton;
    [SerializeField] internal RectTransform socialTooltipReference;

    private readonly Dictionary<TaskbarButtonType, TaskbarButton> buttonsByType =
        new Dictionary<TaskbarButtonType, TaskbarButton>();

    private TaskbarButton lastToggledOnButton;

    public event System.Action<bool> OnChatToggle;
    public event System.Action<bool> OnFriendsToggle;
    public event System.Action<bool> OnEmotesToggle;
    public event System.Action<bool> OnVoiceChatToggle;
    public event System.Action<bool> OnExperiencesToggle;

    private HUDCanvasCameraModeController hudCanvasCameraModeController;

    internal static TaskbarHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TaskbarHUDView>();
        view.Initialize();
        return view;
    }

    private void Awake() { hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera); }

    private void Initialize()
    {
        ShowBar(true, true);

        buttonsByType[TaskbarButtonType.Chat] = chatButton;
        buttonsByType[TaskbarButtonType.Emotes] = emotesButton;
        buttonsByType[TaskbarButtonType.Experiences] = experiencesButton;
        buttonsByType[TaskbarButtonType.Friends] = friendsButton;
        buttonsByType[TaskbarButtonType.VoiceChat] = voiceChatButton;

        chatButton.transform.parent.gameObject.SetActive(false);
        friendsButton.transform.parent.gameObject.SetActive(false);
        emotesButton.transform.parent.gameObject.SetActive(false);
        experiencesButton.transform.parent.gameObject.SetActive(false);
        voiceChatButton.transform.parent.gameObject.SetActive(false);

        chatButton.Initialize();
        friendsButton.Initialize();
        emotesButton.Initialize();
        experiencesButton.Initialize();
        voiceChatButton.Initialize();

        chatButton.OnToggleOn += ToggleOn;
        friendsButton.OnToggleOn += ToggleOn;
        emotesButton.OnToggleOn += ToggleOn;
        experiencesButton.OnToggleOn += ToggleOn;
        voiceChatButton.OnToggleOn += ToggleOn;
        chatButton.OnToggleOff += ToggleOff;
        friendsButton.OnToggleOff += ToggleOff;
        emotesButton.OnToggleOff += ToggleOff;
        experiencesButton.OnToggleOff += ToggleOff;
        voiceChatButton.OnToggleOff += ToggleOff;
    }

    private void OnDestroy()
    {
        hudCanvasCameraModeController?.Dispose();

        if (chatButton != null)
        {
            chatButton.OnToggleOn -= ToggleOn;
            chatButton.OnToggleOff -= ToggleOff;
        }

        if (friendsButton != null)
        {
            friendsButton.OnToggleOn -= ToggleOn;
            friendsButton.OnToggleOff -= ToggleOff;
        }

        if (emotesButton != null)
        {
            emotesButton.OnToggleOn -= ToggleOn;
            emotesButton.OnToggleOff -= ToggleOff;
        }

        if(voiceChatButton != null)
        {
            voiceChatButton.OnToggleOn -= ToggleOn;
            voiceChatButton.OnToggleOff -= ToggleOff;
        }

        if (experiencesButton != null)
        {
            experiencesButton.OnToggleOn -= ToggleOn;
            experiencesButton.OnToggleOff -= ToggleOff;
        }
    }

    public void Destroy() => Destroy(gameObject);

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetExperiencesVisibility(bool visible)
    {
        experiencesContainer.SetActive(visible);
        experiencesContainer.transform.SetAsLastSibling();
    }

    public void ToggleOn(TaskbarButtonType buttonType) => ToggleOn(buttonsByType[buttonType], false);

    public void ToggleOff(TaskbarButtonType buttonType) => ToggleOff(buttonsByType[buttonType], false);
    
    private void ToggleOn(TaskbarButton obj) => ToggleOn(obj, true);

    private void ToggleOn(TaskbarButton obj, bool useCallback)
    {
        var wasToggled = lastToggledOnButton == obj;
        lastToggledOnButton = obj;
        
        foreach (var btn in buttonsByType.Values)
            btn.SetToggleState(btn == obj, useCallback);

        if (!useCallback) return;
        if (wasToggled) return;
        
        if (obj == friendsButton)
            OnFriendsToggle?.Invoke(true);
        if (obj == emotesButton)
            OnEmotesToggle?.Invoke(true);
        else if (obj == chatButton)
            OnChatToggle?.Invoke(true);
        else if (obj == experiencesButton)
            OnExperiencesToggle?.Invoke(true);
        else if (obj == voiceChatButton)
            OnVoiceChatToggle?.Invoke(true);
    }

    private void ToggleOff(TaskbarButton obj) => ToggleOff(obj, true);

    private void ToggleOff(TaskbarButton obj, bool useCallback)
    {
        var wasToggled = lastToggledOnButton == obj;
        
        if (wasToggled)
            lastToggledOnButton = null;
        
        obj.SetToggleState(false, useCallback);

        if (!useCallback) return;
        if (!wasToggled) return;
        
        if (obj == friendsButton)
            OnFriendsToggle?.Invoke(false);
        if (obj == emotesButton)
            OnEmotesToggle?.Invoke(false);
        else if (obj == chatButton)
            OnChatToggle?.Invoke(false);
        else if (obj == experiencesButton)
            OnExperiencesToggle?.Invoke(false);
        else if (obj == experiencesButton)
            OnExperiencesToggle?.Invoke(false);
        else if (obj == voiceChatButton)
            OnVoiceChatToggle?.Invoke(false);
    }

    internal void ShowChatButton()
    {
        chatButton.transform.parent.gameObject.SetActive(true);
    }

    internal void ShowFriendsButton()
    {
        friendsButton.transform.parent.gameObject.SetActive(true);
    }

    internal void ShowEmotesButton()
    {
        emotesButton.transform.parent.gameObject.SetActive(true);
    }

    internal void ShowExperiencesButton()
    {
        experiencesButton.transform.parent.gameObject.SetActive(true);
    }

    internal void ShowVoiceChatButton()
    {
        voiceChatButton.transform.parent.gameObject.SetActive(true);
    }

    private void ShowBar(bool visible, bool instant = false)
    {
        if (visible)
            taskbarAnimator.Show(instant);
        else
            taskbarAnimator.Hide(instant);
    }

    public enum TaskbarButtonType
    {
        None,
        Friends,
        Emotes,
        Chat,
        Experiences,
        VoiceChat
    }
}