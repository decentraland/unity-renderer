using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DCL.SettingsCommon.GeneralSettings;

public class VoiceChatWindowComponentView : BaseComponentView, IVoiceChatWindowComponentView, IComponentModelConfig<VoiceChatWindowComponentModel>
{
    private const string ALLOW_USERS_TITLE_ALL = "All";
    private const string ALLOW_USERS_TITLE_REGISTERED = "Verified Users";
    private const string ALLOW_USERS_TITLE_FRIENDS = "Friends";

    [Header("Prefab References")]
    [SerializeField] internal ButtonComponentView closeButton;
    [SerializeField] internal ButtonComponentView joinButton;
    [SerializeField] internal ButtonComponentView leaveButton;
    [SerializeField] internal TMP_Text playersText;
    [SerializeField] internal DropdownComponentView allowUsersDropdown;
    [SerializeField] internal ToggleComponentView muteAllToggle;
    [SerializeField] internal GameObject emptyListGameObject;
    [SerializeField] internal ButtonComponentView goToCrowdButton;
    [SerializeField] internal VoiceChatPlayerComponentView playerPrefab;
    [SerializeField] internal Transform usersContainer;
    [SerializeField] internal UserContextMenu contextMenuPanel;

    [Header("Configuration")]
    [SerializeField] internal VoiceChatWindowComponentModel model;

    public event Action OnClose;
    public event Action<bool> OnJoinVoiceChat;
    public event Action<string> OnAllowUsersFilterChange;
    public event Action OnGoToCrowd;
    public event Action<bool> OnMuteAll;
    public event Action<string, bool> OnMuteUser;

    public RectTransform Transform => (RectTransform)transform;
    public bool isMuteAllOn => muteAllToggle.isOn;
    public int numberOfPlayers => currentPlayers.Count;
    public int numberOfPlayersTalking => usersTalking.Count;

    private readonly Queue<VoiceChatPlayerComponentView> playersPool = new Queue<VoiceChatPlayerComponentView>();
    internal Dictionary<string, VoiceChatPlayerComponentView> currentPlayers = new Dictionary<string, VoiceChatPlayerComponentView>();
    internal List<string> usersTalking = new List<string>();

    public override void Awake()
    {
        base.Awake();

        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        joinButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(true));
        leaveButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(false));
        goToCrowdButton.onClick.AddListener(() => OnGoToCrowd?.Invoke());
        allowUsersDropdown.OnOptionSelectionChanged += AllowUsersOptionChanged;
        muteAllToggle.OnSelectedChanged += OnMuteAllToggleChanged;
        
        ConfigureAllowUsersFilter();
    }

    public void Configure(VoiceChatWindowComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetNumberOfPlayers(model.numberOfPlayers);
        SetAsJoined(model.isJoined);
    }

    public override void Show(bool instant = false) { gameObject.SetActive(true); }

    public override void Hide(bool instant = false)
    {
        contextMenuPanel.Hide();
        gameObject.SetActive(false);
    }

    public void SetNumberOfPlayers(int numPlayers) 
    {
        model.numberOfPlayers = numPlayers;

        if (playersText != null)
        {
            playersText.text = $"PLAYERS ({numPlayers})";
            playersText.gameObject.SetActive(numPlayers > 0);
        }

        if (emptyListGameObject != null)
            emptyListGameObject.SetActive(numPlayers == 0);
    }

    public void SetAsJoined(bool isJoined)
    {
        model.isJoined = isJoined;

        if (joinButton != null)
            joinButton.gameObject.SetActive(!isJoined);

        if (leaveButton != null)
            leaveButton.gameObject.SetActive(isJoined);

        using (var iterator = currentPlayers.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.SetAsJoined(isJoined);
            }
        }
    }

    public void SelectAllowUsersOption(int optionIndex) { allowUsersDropdown.GetOption(optionIndex).isOn = true; }

    public void SetPlayerMuted(string userId, bool isMuted)
    {
        if (!currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        elementView.SetAsMuted(isMuted);
    }

    public void SetPlayerRecording(string userId, bool isRecording)
    {
        if (!currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        elementView.SetAsTalking(isRecording);

        if (isRecording)
        {
            if (!usersTalking.Contains(userId))
                usersTalking.Add(userId);
        }
        else
        {
            usersTalking.Remove(userId);
        }
    }

    public void SetPlayerBlocked(string userId, bool isBlocked)
    {
        if (currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            elementView.SetAsBlocked(isBlocked);
    }

    public void SetPlayerAsFriend(string userId, bool isFriend)
    {
        currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView playerView);

        if (playerView != null)
            playerView.SetAsFriend(isFriend);
    }

    public void SetPlayerAsJoined(string userId, bool isJoined)
    {
        currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView playerView);

        if (playerView != null)
            playerView.SetAsJoined(isJoined);
    }

    public void AddOrUpdatePlayer(UserProfile otherProfile)
    {
        if (currentPlayers.ContainsKey(otherProfile.userId))
            return;

        VoiceChatPlayerComponentView elementView = null;

        if (playersPool.Count > 0)
        {
            elementView = playersPool.Dequeue();
        }
        else
        {
            elementView = Instantiate(playerPrefab, usersContainer);
            elementView.OnContextMenuOpen += OpenContextMenu;
            elementView.OnMuteUser += MuteUser;
        }

        elementView.Configure(new VoiceChatPlayerComponentModel
        {
            userId = otherProfile.userId,
            userImageUrl = otherProfile.face256SnapshotURL,
            userName = otherProfile.userName,
            isMuted = false,
            isTalking = false,
            isBlocked = false,
            isFriend = false,
            isJoined = false,
            isBackgroundHover = false
        });

        elementView.SetActive(true);
        currentPlayers.Add(otherProfile.userId, elementView);
        SetNumberOfPlayers(currentPlayers.Count);
    }

    public void RemoveUser(string userId)
    {
        if (!currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        if (!elementView)
            return;

        playersPool.Enqueue(elementView);
        SetPlayerRecording(userId, false);
        currentPlayers.Remove(userId);
        SetNumberOfPlayers(currentPlayers.Count);

        elementView.SetActive(false);
    }

    public string GetUserTalkingByIndex(int index) { return usersTalking[index]; }

    public override void Dispose()
    {
        closeButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
        leaveButton.onClick.RemoveAllListeners();
        allowUsersDropdown.OnOptionSelectionChanged -= AllowUsersOptionChanged;
        muteAllToggle.OnSelectedChanged -= OnMuteAllToggleChanged;

        currentPlayers.Clear();
        playersPool.Clear();

        base.Dispose();
    }

    internal void ConfigureAllowUsersFilter()
    {
        allowUsersDropdown.SetOptions(new List<ToggleComponentModel>
        {
            new ToggleComponentModel
            {
                isOn = true,
                id = VoiceChatAllow.ALL_USERS.ToString(),
                text = ALLOW_USERS_TITLE_ALL,
                isTextActive = true,
                changeTextColorOnSelect = true
            },
            new ToggleComponentModel
            {
                isOn = false,
                id = VoiceChatAllow.VERIFIED_ONLY.ToString(),
                text = ALLOW_USERS_TITLE_REGISTERED,
                isTextActive = true,
                changeTextColorOnSelect = true
            },
            new ToggleComponentModel
            {
                isOn = false,
                id = VoiceChatAllow.FRIENDS_ONLY.ToString(),
                text = ALLOW_USERS_TITLE_FRIENDS,
                isTextActive = true,
                changeTextColorOnSelect = true
            }
        });

        allowUsersDropdown.SetTitle(ALLOW_USERS_TITLE_ALL);
    }

    internal void AllowUsersOptionChanged(bool isOn, string optionId, string optionName)
    {
        if (!isOn)
            return;

        allowUsersDropdown.SetTitle(optionName);
        OnAllowUsersFilterChange?.Invoke(optionId);
    }

    internal void OnMuteAllToggleChanged(bool isOn, string id, string text) { OnMuteAll?.Invoke(isOn); }

    internal void MuteUser(string userId, bool isMute) { OnMuteUser?.Invoke(userId, isMute); }

    internal void OpenContextMenu(string userId)
    {
        currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView);

        if (elementView != null)
            elementView.DockAndOpenUserContextMenu(contextMenuPanel);
    }

    internal static VoiceChatWindowComponentView Create()
    {
        VoiceChatWindowComponentView voiceChatWindowComponentView = Instantiate(Resources.Load<GameObject>("SocialBarV1/VoiceChatHUD")).GetComponent<VoiceChatWindowComponentView>();
        voiceChatWindowComponentView.name = "_VoiceChatHUD";

        return voiceChatWindowComponentView;
    }
}