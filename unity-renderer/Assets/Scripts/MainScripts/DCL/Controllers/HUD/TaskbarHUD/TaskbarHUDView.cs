using DCL;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";
    const string PORTABLE_EXPERIENCE_ITEMS_POOL = "PortableExperienceItems";

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
    [SerializeField] internal PortableExperienceTaskbarItem portableExperienceItem;

    [SerializeField] internal RectTransform socialTooltipReference;

    [Header("Old TaskbarCompatibility (temporal)")]
    [SerializeField] internal RectTransform taskbarPanelTransf;

    [SerializeField] internal Image taskbarPanelImage;
    [SerializeField] internal GameObject rightButtonsContainer;

    internal TaskbarHUDController controller;
    internal bool isBarVisible = true;
    internal Dictionary<string, PortableExperienceTaskbarItem> activePortableExperienceItems = new Dictionary<string, PortableExperienceTaskbarItem>();
    internal Dictionary<string, PoolableObject> activePortableExperiencesPoolables = new Dictionary<string, PoolableObject>();
    internal Pool portableExperiencesPool = null;

    public event System.Action OnChatToggleOn;
    public event System.Action OnChatToggleOff;
    public event System.Action OnFriendsToggleOn;
    public event System.Action OnFriendsToggleOff;

    internal List<TaskbarButton> GetButtonList()
    {
        var taskbarButtonList = new List<TaskbarButton>();
        taskbarButtonList.Add(chatButton);
        taskbarButtonList.Add(friendsButton);
        taskbarButtonList.AddRange(chatHeadsGroup.chatHeads);

        using (var iterator = activePortableExperienceItems.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                taskbarButtonList.Add(iterator.Current.Value.mainButton);
            }
        }

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
        voiceChatButtonPlaceholder.SetActive(false);
        voiceChatButton.gameObject.SetActive(false);

        chatHeadsGroup.Initialize(chatController, friendsController);
        chatButton.Initialize();
        friendsButton.Initialize();

        chatHeadsGroup.OnHeadToggleOn += OnWindowToggleOn;
        chatHeadsGroup.OnHeadToggleOff += OnWindowToggleOff;

        chatButton.OnToggleOn += OnWindowToggleOn;
        chatButton.OnToggleOff += OnWindowToggleOff;

        friendsButton.OnToggleOn += OnWindowToggleOn;
        friendsButton.OnToggleOff += OnWindowToggleOff;

        portableExperiencesPool = PoolManager.i.AddPool(
            PORTABLE_EXPERIENCE_ITEMS_POOL,
            Instantiate(portableExperienceItem.gameObject),
            maxPrewarmCount: 5,
            isPersistent: true);

        portableExperiencesPool.ForcePrewarm();

        AdjustRightButtonsLayoutWidth();
    }

    private void OnWindowToggleOff(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOff?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOff?.Invoke();
        else
        {
            using (var iterator = activePortableExperienceItems.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.mainButton == obj)
                    {
                        iterator.Current.Value.ShowContextMenu(false);
                        break;
                    }
                }
            }
        }

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
        else
        {
            using (var iterator = activePortableExperienceItems.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.mainButton == obj)
                    {
                        iterator.Current.Value.ShowContextMenu(true);
                        break;
                    }
                }
            }
        }

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
    }

    internal void AddPortableExperienceElement(string id, string name, string iconUrl)
    {
        if (portableExperiencesPool == null)
            return;

        PoolableObject newPEPoolable = portableExperiencesPool.Get();
        newPEPoolable.gameObject.name = $"PortableExperienceItem ({id})";
        newPEPoolable.gameObject.transform.SetParent(rightButtonsContainer.transform);
        newPEPoolable.gameObject.transform.localScale = Vector3.one;
        newPEPoolable.gameObject.transform.SetAsFirstSibling();

        PortableExperienceTaskbarItem newPEItem = newPEPoolable.gameObject.GetComponent<PortableExperienceTaskbarItem>();
        newPEItem.ConfigureItem(id, name, iconUrl, controller);
        newPEItem.mainButton.OnToggleOn += OnWindowToggleOn;
        newPEItem.mainButton.OnToggleOff += OnWindowToggleOff;

        activePortableExperienceItems.Add(id, newPEItem);
        activePortableExperiencesPoolables.Add(id, newPEPoolable);

        AdjustRightButtonsLayoutWidth();
    }

    internal void RemovePortableExperienceElement(string id)
    {
        if (portableExperiencesPool == null)
            return;

        if (activePortableExperienceItems.ContainsKey(id))
        {
            PortableExperienceTaskbarItem peToRemove = activePortableExperienceItems[id];

            peToRemove.mainButton.OnToggleOn -= OnWindowToggleOn;
            peToRemove.mainButton.OnToggleOff -= OnWindowToggleOff;

            activePortableExperienceItems.Remove(id);
            portableExperiencesPool.Release(activePortableExperiencesPoolables[id]);
            activePortableExperiencesPoolables.Remove(id);
        }

        AdjustRightButtonsLayoutWidth();
    }

    [ContextMenu("AdjustRightButtonsLayoutWidth")]
    private void AdjustRightButtonsLayoutWidth()
    {
        float totalWidth = 0f;
        int numActiveChild = 0;
        RectTransform rightButtonsContainerRT = (RectTransform)rightButtonsContainer.transform;

        for (int i = 0; i < rightButtonsContainerRT.childCount; i++)
        {
            RectTransform child = (RectTransform)rightButtonsContainerRT.GetChild(i);

            if (!child.gameObject.activeSelf)
                continue;

            totalWidth += child.sizeDelta.x;
            numActiveChild++;
        }

        totalWidth +=
            ((numActiveChild - 1) * rightButtonsHorizontalLayout.spacing) +
            rightButtonsHorizontalLayout.padding.left +
            rightButtonsHorizontalLayout.padding.right;

        ((RectTransform)rightButtonsContainer.transform).sizeDelta = new Vector2(totalWidth, ((RectTransform)rightButtonsContainer.transform).sizeDelta.y);
    }
}