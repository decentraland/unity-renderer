using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DCL.SettingsCommon.GeneralSettings;

public class VoiceChatWindowComponentView : BaseComponentView, IVoiceChatWindowComponentView, IComponentModelConfig
{
    private const string ALLOW_USERS_TITLE_ALL = "All";
    private const string ALLOW_USERS_TITLE_REGISTERED = "Registered Users";
    private const string ALLOW_USERS_TITLE_FRIENDS = "Friends";

    [Header("Prefab References")]
    [SerializeField] private ButtonComponentView closeButton;
    [SerializeField] private ButtonComponentView joinButton;
    [SerializeField] private ButtonComponentView leaveButton;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private DropdownComponentView allowUsersDropdown;
    [SerializeField] private ToggleComponentView muteAllToggle;
    [SerializeField] private GameObject emptyListGameObject;
    [SerializeField] private ButtonComponentView goToCrowdButton;
    [SerializeField] private VoiceChatPlayerComponentView playerPrefab;
    [SerializeField] private Transform usersContainer;
    [SerializeField] private UserContextMenu contextMenuPanel;

    [Header("Configuration")]
    [SerializeField] internal VoiceChatWindowComponentModel model;

    public event Action OnClose;
    public event Action<bool> OnJoinVoiceChat;
    public event Action<string> OnAllowUsersFilterChange;
    public event Action OnGoToCrowd;
    public event Action<bool> OnMuteAll;

    public RectTransform Transform => (RectTransform)transform;

    public UserContextMenu ContextMenuPanel => contextMenuPanel;

    public bool isMuteAllOn => muteAllToggle.isOn;

    public override void Awake()
    {
        base.Awake();

        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        joinButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(true));
        leaveButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(false));
        goToCrowdButton.onClick.AddListener(() => OnGoToCrowd?.Invoke());
        allowUsersDropdown.OnOptionSelectionChanged += AllowUsersOptionChanged;
        muteAllToggle.OnSelectedChanged += OnMuteAllToggleChanged;
    }

    public override void Start()
    {
        base.Start();

        ConfigureAllowUsersFilter();
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (VoiceChatWindowComponentModel)newModel;
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
    }

    public void SetMuteAllIsOn(bool isOn, bool notify = true)
    {
        if (notify)
            muteAllToggle.isOn = isOn;
        else
            muteAllToggle.SetIsOnWithoutNotify(isOn);
    }

    public void SelectAllowUsersOption(int optionIndex) { allowUsersDropdown.GetOption(optionIndex).isOn = true; }

    public VoiceChatPlayerComponentView CreateNewPlayerInstance() => Instantiate(playerPrefab, usersContainer);

    public override void Dispose()
    {
        closeButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
        leaveButton.onClick.RemoveAllListeners();
        allowUsersDropdown.OnOptionSelectionChanged -= AllowUsersOptionChanged;
        muteAllToggle.OnSelectedChanged -= OnMuteAllToggleChanged;

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

        AllowUsersOptionChanged(true, VoiceChatAllow.ALL_USERS.ToString(), ALLOW_USERS_TITLE_ALL);
    }

    internal void AllowUsersOptionChanged(bool isOn, string optionId, string optionName)
    {
        if (!isOn)
            return;

        allowUsersDropdown.SetTitle(optionName);
        OnAllowUsersFilterChange?.Invoke(optionId);
    }

    internal void OnMuteAllToggleChanged(bool isOn, string id, string text) { OnMuteAll?.Invoke(isOn); }

    internal static VoiceChatWindowComponentView Create()
    {
        VoiceChatWindowComponentView voiceChatWindowComponentView = Instantiate(Resources.Load<GameObject>("VoiceChatHUD")).GetComponent<VoiceChatWindowComponentView>();
        voiceChatWindowComponentView.name = "_VoiceChatHUD";

        return voiceChatWindowComponentView;
    }
}