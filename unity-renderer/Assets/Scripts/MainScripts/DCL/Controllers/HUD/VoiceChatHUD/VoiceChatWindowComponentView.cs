using DCL.SettingsCommon;
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

    public override void Awake()
    {
        base.Awake();

        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        joinButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(true));
        leaveButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(false));
        goToCrowdButton.onClick.AddListener(() => OnGoToCrowd?.Invoke());
        allowUsersDropdown.OnOptionSelectionChanged += AllowUsersOptionChanged;
        muteAllToggle.OnSelectedChanged += OnMuteAllToggleChanged;
        Settings.i.generalSettings.OnChanged += OnSettingsChanged;
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
            playersText.text = $"PLAYERS ({numPlayers})";

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

        if (allowUsersDropdown != null)
            allowUsersDropdown.gameObject.SetActive(isJoined);

        if (muteAllToggle != null)
            muteAllToggle.gameObject.SetActive(isJoined);
    }

    public VoiceChatPlayerComponentView CreateNewPlayerInstance() => Instantiate(playerPrefab, usersContainer);

    public override void Dispose()
    {
        closeButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
        leaveButton.onClick.RemoveAllListeners();
        allowUsersDropdown.OnOptionSelectionChanged -= AllowUsersOptionChanged;
        muteAllToggle.OnSelectedChanged -= OnMuteAllToggleChanged;
        Settings.i.generalSettings.OnChanged -= OnSettingsChanged;

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
                isTextActive = true
            },
            new ToggleComponentModel
            {
                isOn = false,
                id = VoiceChatAllow.VERIFIED_ONLY.ToString(),
                text = ALLOW_USERS_TITLE_REGISTERED,
                isTextActive = true
            },
            new ToggleComponentModel
            {
                isOn = false,
                id = VoiceChatAllow.FRIENDS_ONLY.ToString(),
                text = ALLOW_USERS_TITLE_FRIENDS,
                isTextActive = true
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
        allowUsersDropdown.Close();

        var newSettings = Settings.i.generalSettings.Data;

        if (optionId == VoiceChatAllow.ALL_USERS.ToString())
            newSettings.voiceChatAllow = VoiceChatAllow.ALL_USERS;
        else if (optionId == VoiceChatAllow.VERIFIED_ONLY.ToString())
            newSettings.voiceChatAllow = VoiceChatAllow.VERIFIED_ONLY;
        else if (optionId == VoiceChatAllow.FRIENDS_ONLY.ToString())
            newSettings.voiceChatAllow = VoiceChatAllow.FRIENDS_ONLY;

        Settings.i.generalSettings.Apply(newSettings);
    }

    internal void OnSettingsChanged(GeneralSettings settings) 
    {
        switch (settings.voiceChatAllow)
        {
            case VoiceChatAllow.ALL_USERS:
                allowUsersDropdown.GetOption(0).isOn = true;
                break;
            case VoiceChatAllow.VERIFIED_ONLY:
                allowUsersDropdown.GetOption(1).isOn = true;
                break;
            case VoiceChatAllow.FRIENDS_ONLY:
                allowUsersDropdown.GetOption(2).isOn = true;
                break;
        }
    }

    internal void OnMuteAllToggleChanged(bool isOn, string id, string text) { OnMuteAll?.Invoke(isOn); }

    internal static VoiceChatWindowComponentView Create()
    {
        VoiceChatWindowComponentView voiceChatWindowComponentView = Instantiate(Resources.Load<GameObject>("VoiceChatHUD")).GetComponent<VoiceChatWindowComponentView>();
        voiceChatWindowComponentView.name = "_VoiceChatHUD";

        return voiceChatWindowComponentView;
    }
}