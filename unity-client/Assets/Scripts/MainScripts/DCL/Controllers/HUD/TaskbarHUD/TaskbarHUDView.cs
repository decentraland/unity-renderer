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
    [SerializeField] internal TaskbarButton chatButton;
    [SerializeField] internal TaskbarButton friendsButton;
    [SerializeField] internal ChatHeadGroupView chatHeadsGroup;

    [Header("Right Side Config")]
    [SerializeField] internal TaskbarButton settingsButton;
    [SerializeField] internal TaskbarButton backpackButton;
    [SerializeField] internal TaskbarButton exploreButton;
    [SerializeField] internal TaskbarButton helpAndSupportButton;
    [SerializeField] internal GameObject separatorMark;

    [Header("More Button Config")]
    [SerializeField] internal TaskbarButton moreButton;
    [SerializeField] internal TaskbarMoreMenu moreMenu;

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
    public event System.Action OnBackpackToggleOn;
    public event System.Action OnBackpackToggleOff;
    public event System.Action OnExploreToggleOn;
    public event System.Action OnExploreToggleOff;
    public event System.Action OnHelpAndSupportToggleOn;
    public event System.Action OnHelpAndSupportToggleOff;
    public event System.Action OnMoreToggleOn;
    public event System.Action OnMoreToggleOff;

    internal List<TaskbarButton> GetButtonList()
    {
        var taskbarButtonList = new List<TaskbarButton>();
        taskbarButtonList.Add(chatButton);
        taskbarButtonList.Add(friendsButton);
        taskbarButtonList.AddRange(chatHeadsGroup.chatHeads);
        taskbarButtonList.Add(settingsButton);
        taskbarButtonList.Add(backpackButton);
        taskbarButtonList.Add(exploreButton);
        taskbarButtonList.Add(helpAndSupportButton);
        taskbarButtonList.Add(moreButton);
        return taskbarButtonList;
    }

    internal static TaskbarHUDView Create(TaskbarHUDController controller, IChatController chatController,
        IFriendsController friendsController, bool newTaskbarIsEnabled)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TaskbarHUDView>();
        view.Initialize(controller, chatController, friendsController, newTaskbarIsEnabled);
        return view;
    }

    public void Initialize(TaskbarHUDController controller, IChatController chatController,
        IFriendsController friendsController, bool newTaskbarIsEnabled)
    {
        this.controller = controller;

        ShowBar(true, true);
        chatButton.transform.parent.gameObject.SetActive(false);
        friendsButton.transform.parent.gameObject.SetActive(false);
        settingsButton.transform.parent.gameObject.SetActive(false);
        backpackButton.transform.parent.gameObject.SetActive(false);
        exploreButton.transform.parent.gameObject.SetActive(false);
        helpAndSupportButton.transform.parent.gameObject.SetActive(false);
        separatorMark.SetActive(false);

        moreButton.gameObject.SetActive(true);
        moreMenu.Initialize(this);
        moreMenu.ShowMoreMenu(false, true);

        chatHeadsGroup.Initialize(chatController, friendsController);
        chatButton.Initialize();
        friendsButton.Initialize();
        settingsButton.Initialize();
        backpackButton.Initialize();
        exploreButton.Initialize();
        helpAndSupportButton.Initialize();
        moreButton.Initialize();

        chatHeadsGroup.OnHeadToggleOn += OnWindowToggleOn;
        chatHeadsGroup.OnHeadToggleOff += OnWindowToggleOff;

        chatButton.OnToggleOn += OnWindowToggleOn;
        chatButton.OnToggleOff += OnWindowToggleOff;

        friendsButton.OnToggleOn += OnWindowToggleOn;
        friendsButton.OnToggleOff += OnWindowToggleOff;

        settingsButton.OnToggleOn += OnWindowToggleOn;
        settingsButton.OnToggleOff += OnWindowToggleOff;

        backpackButton.OnToggleOn += OnWindowToggleOn;
        backpackButton.OnToggleOff += OnWindowToggleOff;

        exploreButton.OnToggleOn += OnWindowToggleOn;
        exploreButton.OnToggleOff += OnWindowToggleOff;

        helpAndSupportButton.OnToggleOn += OnWindowToggleOn;
        helpAndSupportButton.OnToggleOff += OnWindowToggleOff;

        moreButton.OnToggleOn += OnWindowToggleOn;
        moreButton.OnToggleOff += OnWindowToggleOff;

        if (!newTaskbarIsEnabled)
            ActivateOldTaskbar();
    }

    private void OnWindowToggleOff(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOff?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOff?.Invoke();
        else if (obj == settingsButton)
            OnSettingsToggleOff?.Invoke();
        else if (obj == backpackButton)
            OnBackpackToggleOff?.Invoke();
        else if (obj == exploreButton)
            OnExploreToggleOff?.Invoke();
        else if (obj == helpAndSupportButton)
            OnHelpAndSupportToggleOff?.Invoke();
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
        else if (obj == backpackButton)
            OnBackpackToggleOn?.Invoke();
        else if (obj == exploreButton)
            OnExploreToggleOn?.Invoke();
        else if (obj == helpAndSupportButton)
            OnHelpAndSupportToggleOn?.Invoke();
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
        separatorMark.SetActive(true);
    }

    internal void OnAddBackpackWindow()
    {
        backpackButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddExploreWindow()
    {
        exploreButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddHelpAndSupportWindow()
    {
        helpAndSupportButton.transform.parent.gameObject.SetActive(true);
        separatorMark.SetActive(true);
    }

    internal void OnAddControlsMoreOption()
    {
        moreMenu.ActivateControlsButton();
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

        if (backpackButton != null)
        {
            backpackButton.OnToggleOn -= OnWindowToggleOn;
            backpackButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (exploreButton != null)
        {
            exploreButton.OnToggleOn -= OnWindowToggleOn;
            exploreButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (helpAndSupportButton != null)
        {
            helpAndSupportButton.OnToggleOn -= OnWindowToggleOn;
            helpAndSupportButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (moreButton != null)
        {
            moreButton.OnToggleOn -= OnWindowToggleOn;
            moreButton.OnToggleOff -= OnWindowToggleOff;
        }
    }

    // NOTE(Santi): This is temporal, until we remove the old taskbar
    private void ActivateOldTaskbar()
    {
        taskbarPanelTransf.offsetMax = new Vector2(-200, taskbarPanelTransf.offsetMax.y);
        taskbarPanelImage.color = new Color(taskbarPanelImage.color.r, taskbarPanelImage.color.g, taskbarPanelImage.color.b, 0f);
        moreButton.gameObject.SetActive(false);
        rightButtonsContainer.SetActive(false);
    }
}