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

    [Header("Right Side Config")]
    [SerializeField] internal HorizontalLayoutGroup rightButtonsHorizontalLayout;
    [SerializeField] internal TaskbarButton settingsButton;
    [SerializeField] internal TaskbarButton exploreButton;

    [Header("More Button Config")]
    [SerializeField] internal TaskbarButton moreButton;
    [SerializeField] internal TaskbarMoreMenu moreMenu;

    [Header("Tutorial Config")]
    [SerializeField] internal RectTransform exploreTooltipReference;
    [SerializeField] internal RectTransform moreTooltipReference;

    [Header("Old TaskbarCompatibility (temporal)")]
    [SerializeField] internal RectTransform taskbarPanelTransf;
    [SerializeField] internal Image taskbarPanelImage;
    [SerializeField] internal GameObject rightButtonsContainer;

    internal TaskbarHUDController controller;
    internal bool isBarVisible = true;

    public event System.Action OnChatToggleOn;
    public event System.Action OnChatToggleOff;
    public event System.Action OnFriendsToggleOn;
    public event System.Action OnFriendsToggleOff;
    public event System.Action OnSettingsToggleOn;
    public event System.Action OnSettingsToggleOff;
    public event System.Action OnExploreToggleOn;
    public event System.Action OnExploreToggleOff;
    public event System.Action OnMoreToggleOn;
    public event System.Action OnMoreToggleOff;

    internal List<TaskbarButton> GetButtonList()
    {
        var taskbarButtonList = new List<TaskbarButton>();
        taskbarButtonList.Add(chatButton);
        taskbarButtonList.Add(friendsButton);
        taskbarButtonList.AddRange(chatHeadsGroup.chatHeads);
        taskbarButtonList.Add(settingsButton);
        taskbarButtonList.Add(exploreButton);
        taskbarButtonList.Add(moreButton);
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
        settingsButton.transform.parent.gameObject.SetActive(false);
        exploreButton.transform.parent.gameObject.SetActive(false);
        voiceChatButtonPlaceholder.SetActive(false);
        voiceChatButton.gameObject.SetActive(false);

        moreButton.gameObject.SetActive(true);
        moreMenu.Initialize(this);
        moreMenu.ShowMoreMenu(false, true);

        chatHeadsGroup.Initialize(chatController, friendsController);
        chatButton.Initialize();
        friendsButton.Initialize();
        settingsButton.Initialize();
        exploreButton.Initialize();
        moreButton.Initialize();

        chatHeadsGroup.OnHeadToggleOn += OnWindowToggleOn;
        chatHeadsGroup.OnHeadToggleOff += OnWindowToggleOff;

        chatButton.OnToggleOn += OnWindowToggleOn;
        chatButton.OnToggleOff += OnWindowToggleOff;

        friendsButton.OnToggleOn += OnWindowToggleOn;
        friendsButton.OnToggleOff += OnWindowToggleOff;

        settingsButton.OnToggleOn += OnWindowToggleOn;
        settingsButton.OnToggleOff += OnWindowToggleOff;

        exploreButton.OnToggleOn += OnWindowToggleOn;
        exploreButton.OnToggleOff += OnWindowToggleOff;

        moreButton.OnToggleOn += OnWindowToggleOn;
        moreButton.OnToggleOff += OnWindowToggleOff;
    }

    private void OnWindowToggleOff(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOff?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOff?.Invoke();
        else if (obj == settingsButton)
            OnSettingsToggleOff?.Invoke();
        else if (obj == exploreButton)
            OnExploreToggleOff?.Invoke();
        else if (obj == moreButton)
            moreMenu.ShowMoreMenu(false);

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
        else if (obj == settingsButton)
            OnSettingsToggleOn?.Invoke();
        else if (obj == exploreButton)
            OnExploreToggleOn?.Invoke();
        else if (obj == moreButton)
            moreMenu.ShowMoreMenu(true);

        SelectButton(obj);
    }

    void SelectButton(TaskbarButton obj)
    {
        var taskbarButtonList = GetButtonList();

        foreach (var btn in taskbarButtonList)
        {
            if (btn != obj)
            {
                // We let the use of the chat and friends windows while we are using the explore at the same time
                if ((btn == exploreButton && (obj == chatButton || obj == friendsButton || obj is ChatHeadButton)) ||
                    ((btn == chatButton || btn == friendsButton || btn is ChatHeadButton) && obj == exploreButton))
                    continue;

                btn.SetToggleState(false, useCallback: true);
            }
        }
    }

    internal void OnAddChatWindow()
    {
        chatButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddFriendsWindow()
    {
        friendsButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddSettingsWindow()
    {
        settingsButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddExploreWindow()
    {
        exploreButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddHelpAndSupportWindow()
    {
        moreMenu.ActivateHelpAndSupportButton();
    }

    internal void OnAddControlsMoreOption()
    {
        moreMenu.ActivateControlsButton();
    }

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

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Return))
    //    {
    //        controller.OnPressReturn();
    //    }

    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        controller.OnPressEsc();
    //    }
    //}

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

        if (settingsButton != null)
        {
            settingsButton.OnToggleOn -= OnWindowToggleOn;
            settingsButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (exploreButton != null)
        {
            exploreButton.OnToggleOn -= OnWindowToggleOn;
            exploreButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (moreButton != null)
        {
            moreButton.OnToggleOn -= OnWindowToggleOn;
            moreButton.OnToggleOff -= OnWindowToggleOff;
        }
    }
}