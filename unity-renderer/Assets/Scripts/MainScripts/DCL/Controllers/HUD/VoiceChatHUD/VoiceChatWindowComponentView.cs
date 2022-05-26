using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoiceChatWindowComponentView : BaseComponentView, IVoiceChatWindowComponentView
{
    enum AllowUsersFilter
    {
        All,
        Registered,
        Friends
    }

    [Header("Prefab References")]
    [SerializeField] private ButtonComponentView closeButton;
    [SerializeField] private ButtonComponentView joinButton;
    [SerializeField] private ButtonComponentView leaveButton;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private DropdownComponentView allowUsersDropdown;
    [SerializeField] private VoiceChatPlayerComponentView userElementView;
    [SerializeField] private Transform usersContainer;
    [SerializeField] private GameObject emptyListGameObject;

    public event Action OnClose;
    public event Action<bool> OnJoinVoiceChat;
    public event Action<string> OnAllowUsersFilterChange;
    public event Action<string, bool> OnRequestMuteUser;

    public RectTransform Transform => (RectTransform)transform;

    internal Queue<VoiceChatPlayerComponentView> availableElements;
    internal Dictionary<string, VoiceChatPlayerComponentView> userElementDictionary;

    public override void Awake()
    {
        base.Awake();

        availableElements = new Queue<VoiceChatPlayerComponentView>();
        userElementDictionary = new Dictionary<string, VoiceChatPlayerComponentView>();

        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        joinButton.onClick.AddListener(() => SetAsJoined(true));
        leaveButton.onClick.AddListener(() => SetAsJoined(false));
        allowUsersDropdown.OnOptionSelectionChanged += AllowUsersOptionChanged;

        userElementView.OnMuteUser += OnMuteUser;
        userElementView.gameObject.SetActive(false);
        availableElements.Enqueue(userElementView);
    }

    public override void Start()
    {
        base.Start();

        ConfigureAllowUsersFilter();
        SetNumberOfPlayers(0);
        SetAsJoined(false);
    }

    public override void RefreshControl() { }

    public override void Show(bool instant = false) { gameObject.SetActive(true); }

    public override void Hide(bool instant = false) { gameObject.SetActive(false); }

    public void SetNumberOfPlayers(int numPlayers) { playersText.text = $"PLAYERS ({numPlayers})"; }

    public void AddOrUpdatePlayer(Player player)
    {
        if (userElementDictionary.ContainsKey(player.id))
            return;

        var profile = UserProfileController.userProfilesCatalog.Get(player.id);

        if (profile == null)
            return;

        VoiceChatPlayerComponentView view = null;
        if (availableElements.Count > 0)
        {
            view = availableElements.Dequeue();
        }
        else
        {
            view = Instantiate(userElementView, usersContainer);
            view.OnMuteUser += OnMuteUser;
        }

        view.gameObject.SetActive(true);
        view.Configure(new VoiceChatPlayerComponentModel
        {
            userId = profile.userId,
            userImageUrl = profile.face256SnapshotURL,
            userName = profile.userName,
            isMuted = false,
            isTalking = false,
            isBlocked = false,
            isFriend = false,
            isBackgroundHover = false
        });

        userElementDictionary.Add(player.id, view);
        SetNumberOfPlayers(userElementDictionary.Count);
        CheckListEmptyState();
    }

    public void RemoveUser(string userId)
    {
        if (!userElementDictionary.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        if (!elementView)
            return;

        PoolElementView(elementView);
        userElementDictionary.Remove(userId);
        SetNumberOfPlayers(userElementDictionary.Count);
        CheckListEmptyState();
    }

    public void SetUserRecording(string userId, bool isRecording)
    {
        if (!userElementDictionary.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        elementView.SetAsTalking(isRecording);
    }

    public void SetUserMuted(string userId, bool isMuted)
    {
        if (!userElementDictionary.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        elementView.SetAsMuted(isMuted);
    }

    public void SetUserBlocked(string userId, bool blocked)
    {
        if (!userElementDictionary.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        elementView.SetAsBlocked(blocked);
    }

    public override void Dispose()
    {
        base.Dispose();

        closeButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
        leaveButton.onClick.RemoveAllListeners();
        allowUsersDropdown.OnOptionSelectionChanged -= AllowUsersOptionChanged;

        userElementView.OnMuteUser -= OnMuteUser;

        userElementDictionary.Clear();
        availableElements.Clear();
    }

    internal void ConfigureAllowUsersFilter()
    {
        allowUsersDropdown.SetOptions(new List<ToggleComponentModel>
        {
            new ToggleComponentModel
            {
                isOn = true,
                id = AllowUsersFilter.All.ToString(),
                text = AllowUsersFilter.All.ToString(),
                isTextActive = true
            },
            new ToggleComponentModel
            {
                isOn = false,
                id = AllowUsersFilter.Registered.ToString(),
                text = AllowUsersFilter.Registered.ToString(),
                isTextActive = true
            },
            new ToggleComponentModel
            {
                isOn = false,
                id = AllowUsersFilter.Friends.ToString(),
                text = AllowUsersFilter.Friends.ToString(),
                isTextActive = true
            }
        });

        AllowUsersOptionChanged(true, AllowUsersFilter.All.ToString(), AllowUsersFilter.All.ToString());
    }

    internal void AllowUsersOptionChanged(bool isOn, string optionId, string optionName)
    {
        if (!isOn)
            return;

        allowUsersDropdown.SetTitle(optionName);
        OnAllowUsersFilterChange?.Invoke(optionId);
        allowUsersDropdown.Close();
    }

    internal void SetAsJoined(bool isJoined)
    {
        joinButton.gameObject.SetActive(!isJoined);
        leaveButton.gameObject.SetActive(isJoined);
        allowUsersDropdown.gameObject.SetActive(isJoined);

        OnJoinVoiceChat?.Invoke(isJoined);
    }

    internal void OnMuteUser(string userId, bool mute) { OnRequestMuteUser?.Invoke(userId, mute); }

    internal void CheckListEmptyState() { emptyListGameObject.SetActive(userElementDictionary.Count == 0); }

    internal void PoolElementView(VoiceChatPlayerComponentView element)
    {
        element.gameObject.SetActive(false);
        availableElements.Enqueue(element);
    }

    internal static VoiceChatWindowComponentView Create()
    {
        VoiceChatWindowComponentView voiceChatWindowComponentView = Instantiate(Resources.Load<GameObject>("VoiceChatHUD")).GetComponent<VoiceChatWindowComponentView>();
        voiceChatWindowComponentView.name = "_VoiceChatHUD";

        return voiceChatWindowComponentView;
    }
}