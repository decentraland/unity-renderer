using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";

    [Header("Taskbar Animation")]
    [SerializeField] internal ShowHideAnimator taskbarAnimator;

    [Header("Left Side Config")]
    [SerializeField] internal RectTransform leftWindowContainer;

    [SerializeField] internal ShowHideAnimator leftWindowContainerAnimator;
    [SerializeField] internal LayoutGroup leftWindowContainerLayout;
    [SerializeField] internal GameObject voiceChatButtonPlaceholder;
    [SerializeField] internal VoiceChatButton voiceChatButton;
    [SerializeField] internal TaskbarButton chatButton;
    [SerializeField] internal TaskbarButton friendsButton;
    [SerializeField] internal ChatHeadGroupView chatHeadsGroup;

    [SerializeField] internal GameObject experiencesContainer;
    [SerializeField] internal TaskbarButton experiencesButton;

    [SerializeField] internal RectTransform socialTooltipReference;

    [Header("Old TaskbarCompatibility (temporal)")]
    [SerializeField] internal RectTransform taskbarPanelTransf;

    [SerializeField] internal Image taskbarPanelImage;

    internal TaskbarHUDController controller;
    internal bool isBarVisible = true;

    public event System.Action OnChatToggleOn;
    public event System.Action OnChatToggleOff;
    public event System.Action OnFriendsToggleOn;
    public event System.Action OnFriendsToggleOff;
    public event System.Action OnExperiencesToggleOn;
    public event System.Action OnExperiencesToggleOff;

    internal List<TaskbarButton> GetButtonList()
    {
        var taskbarButtonList = new List<TaskbarButton>();
        taskbarButtonList.Add(chatButton);
        taskbarButtonList.Add(friendsButton);
        taskbarButtonList.AddRange(chatHeadsGroup.chatHeads);
        taskbarButtonList.Add(experiencesButton);

        return taskbarButtonList;
    }

    internal static TaskbarHUDView Create(TaskbarHUDController controller, IChatController chatController,
        IFriendsController friendsController)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TaskbarHUDView>();
        view.Initialize(controller, chatController, friendsController);
        return view;
    }

    public void Initialize(TaskbarHUDController controller, IChatController chatController,
        IFriendsController friendsController)
    {
        this.controller = controller;

        ShowBar(true, true);
        chatButton.transform.parent.gameObject.SetActive(false);
        friendsButton.transform.parent.gameObject.SetActive(false);
        experiencesButton.transform.parent.gameObject.SetActive(false);
        voiceChatButtonPlaceholder.SetActive(false);
        voiceChatButton.gameObject.SetActive(false);

        chatHeadsGroup.Initialize(chatController, friendsController);
        chatButton.Initialize();
        friendsButton.Initialize();
        experiencesButton.Initialize();

        chatHeadsGroup.OnHeadToggleOn += OnWindowToggleOn;
        chatHeadsGroup.OnHeadToggleOff += OnWindowToggleOff;

        chatButton.OnToggleOn += OnWindowToggleOn;
        chatButton.OnToggleOff += OnWindowToggleOff;

        friendsButton.OnToggleOn += OnWindowToggleOn;
        friendsButton.OnToggleOff += OnWindowToggleOff;

        experiencesButton.OnToggleOn += OnWindowToggleOn;
        experiencesButton.OnToggleOff += OnWindowToggleOff;
    }

    private void OnWindowToggleOff(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOff?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOff?.Invoke();
        else if (obj == experiencesButton)
            OnExperiencesToggleOff?.Invoke();

        if (AllButtonsToggledOff())
        {
            chatButton.SetToggleState(false, useCallback: false);
            controller.worldChatWindowHud.SetVisibility(true);
        }
    }

    public bool AllButtonsToggledOff()
    {
        var btns = GetButtonList();

        bool allToggledOff = true;

        foreach (var btn in btns)
        {
            if (btn.toggledOn)
                allToggledOff = false;
        }

        return allToggledOff;
    }

    private void OnWindowToggleOn(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOn?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOn?.Invoke();
        else if (obj == experiencesButton)
            OnExperiencesToggleOn?.Invoke();

        SelectButton(obj);
    }

    public void SelectButton(TaskbarButton obj)
    {
        var taskbarButtonList = GetButtonList();

        foreach (var btn in taskbarButtonList)
        {
            if (btn != obj)
                btn.SetToggleState(false, useCallback: true);
        }
    }

    internal void OnAddChatWindow() { chatButton.transform.parent.gameObject.SetActive(true); }

    internal void OnAddFriendsWindow() { friendsButton.transform.parent.gameObject.SetActive(true); }

    internal void OnAddExperiencesWindow() { experiencesButton.transform.parent.gameObject.SetActive(true); }

    internal void OnAddVoiceChat()
    {
        voiceChatButtonPlaceholder.SetActive(true);
        voiceChatButton.gameObject.SetActive(true);
    }

    internal void ShowBar(bool visible, bool instant = false)
    {
        if (visible)
            taskbarAnimator.Show(instant);
        else
            taskbarAnimator.Hide(instant);

        isBarVisible = visible;
    }

    public void SetVisibility(bool visible) { gameObject.SetActive(visible); }

    public void SetExperiencesVisbility(bool visible) { experiencesContainer.SetActive(visible); }

    private void OnDestroy()
    {
        if (chatHeadsGroup != null)
        {
            chatHeadsGroup.OnHeadToggleOn -= OnWindowToggleOn;
            chatHeadsGroup.OnHeadToggleOff -= OnWindowToggleOff;
        }

        if (chatButton != null)
        {
            chatButton.OnToggleOn -= OnWindowToggleOn;
            chatButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (friendsButton != null)
        {
            friendsButton.OnToggleOn -= OnWindowToggleOn;
            friendsButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (experiencesButton != null)
        {
            experiencesButton.OnToggleOn -= OnWindowToggleOn;
            experiencesButton.OnToggleOff -= OnWindowToggleOff;
        }
    }
}