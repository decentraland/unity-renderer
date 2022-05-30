using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoiceChatWindowComponentView : BaseComponentView, IVoiceChatWindowComponentView, IComponentModelConfig
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
    [SerializeField] private GameObject emptyListGameObject;
    [SerializeField] private VoiceChatPlayerComponentView playerPrefab;
    [SerializeField] private Transform usersContainer;

    [Header("Configuration")]
    [SerializeField] internal VoiceChatWindowComponentModel model;

    public event Action OnClose;
    public event Action<bool> OnJoinVoiceChat;
    public event Action<string> OnAllowUsersFilterChange;

    public RectTransform Transform => (RectTransform)transform;

    public override void Awake()
    {
        base.Awake();

        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        joinButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(true));
        leaveButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(false));
        allowUsersDropdown.OnOptionSelectionChanged += AllowUsersOptionChanged;
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
        SetEmptyListActive(model.isEmptyListActive);
        SetAsJoined(model.isJoined);
    }

    public override void Show(bool instant = false) { gameObject.SetActive(true); }

    public override void Hide(bool instant = false) { gameObject.SetActive(false); }

    public void SetNumberOfPlayers(int numPlayers) { playersText.text = $"PLAYERS ({numPlayers})"; }

    public void SetEmptyListActive(bool isActive) { emptyListGameObject.SetActive(isActive); }

    public void SetAsJoined(bool isJoined)
    {
        joinButton.gameObject.SetActive(!isJoined);
        leaveButton.gameObject.SetActive(isJoined);
        allowUsersDropdown.gameObject.SetActive(isJoined);
    }

    public VoiceChatPlayerComponentView CreateNewPlayerInstance() => Instantiate(playerPrefab, usersContainer);

    public override void Dispose()
    {
        closeButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
        leaveButton.onClick.RemoveAllListeners();
        allowUsersDropdown.OnOptionSelectionChanged -= AllowUsersOptionChanged;

        base.Dispose();
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

    internal static VoiceChatWindowComponentView Create()
    {
        VoiceChatWindowComponentView voiceChatWindowComponentView = Instantiate(Resources.Load<GameObject>("VoiceChatHUD")).GetComponent<VoiceChatWindowComponentView>();
        voiceChatWindowComponentView.name = "_VoiceChatHUD";

        return voiceChatWindowComponentView;
    }
}