using DCL;
using DCL.Interface;
using DCL.SettingsCommon;
using SocialFeaturesAnalytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DCL.SettingsCommon.GeneralSettings;

public class VoiceChatWindowController : IHUD
{
    internal const string VOICE_CHAT_FEATURE_FLAG = "voice_chat";
    internal const float MUTE_STATUS_UPDATE_INTERVAL = 0.3f;
    internal const string TALKING_MESSAGE_YOU = "You";
    internal const string TALKING_MESSAGE_JUST_YOU_IN_THE_VOICE_CHAT = "No one else is here";
    internal const string TALKING_MESSAGE_NOBODY_TALKING = "Nobody is talking";
    internal const string TALKING_MESSAGE_SEVERAL_PEOPLE_TALKING = "Several people talking";

    public IVoiceChatWindowComponentView VoiceChatWindowView => voiceChatWindowView;
    public IVoiceChatBarComponentView VoiceChatBarView => voiceChatBarView;

    private bool isVoiceChatFFEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled(VOICE_CHAT_FEATURE_FLAG);
    private UserProfile ownProfile => userProfileBridge.GetOwn();

    private IVoiceChatWindowComponentView voiceChatWindowView;
    private IVoiceChatBarComponentView voiceChatBarView;
    private IUserProfileBridge userProfileBridge;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private DataStore dataStore;
    private Settings settings;
    internal HashSet<string> trackedUsersHashSet = new HashSet<string>();
    internal readonly List<string> usersToMute = new List<string>();
    internal readonly List<string> usersToUnmute = new List<string>();
    internal bool isOwnPLayerTalking = false;
    private Coroutine updateMuteStatusRoutine = null;
    internal bool isMuteAll = false;
    internal bool isJoined = false;

    public VoiceChatWindowController() { }

    public VoiceChatWindowController(
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics,
        DataStore dataStore,
        Settings settings)
    {
        Initialize(userProfileBridge, friendsController, socialAnalytics, dataStore, settings);
    }

    public void Initialize(
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics,
        DataStore dataStore,
        Settings settings)
    {
        this.userProfileBridge = userProfileBridge;
        this.friendsController = friendsController;
        this.socialAnalytics = socialAnalytics;
        this.dataStore = dataStore;
        this.settings = settings;

        if (!isVoiceChatFFEnabled)
            return;

        voiceChatWindowView = CreateVoiceChatWindowView();
        voiceChatWindowView.Hide(instant: true);

        voiceChatWindowView.OnClose += CloseView;
        voiceChatWindowView.OnJoinVoiceChat += JoinVoiceChat;
        voiceChatWindowView.OnGoToCrowd += GoToCrowd;
        voiceChatWindowView.OnMuteAll += OnMuteAllToggled;
        voiceChatWindowView.OnMuteUser += MuteUser;
        voiceChatWindowView.OnAllowUsersFilterChange += ChangeAllowUsersFilter;
        voiceChatWindowView.SetNumberOfPlayers(0);

        voiceChatBarView = CreateVoiceChatBatView();
        voiceChatBarView.SetAsJoined(false);
        voiceChatBarView.OnJoinVoiceChat += JoinVoiceChat;

        dataStore.player.otherPlayers.OnAdded += OnOtherPlayersStatusAdded;
        dataStore.player.otherPlayers.OnRemoved += OnOtherPlayerStatusRemoved;
        ownProfile.OnUpdate += OnUserProfileUpdated;
        friendsController.OnUpdateFriendship += OnUpdateFriendship;

        settings.generalSettings.OnChanged += OnSettingsChanged;

        CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;
        RendererState_OnChange(CommonScriptableObjects.rendererState.Get(), false);
    }

    public void SetVisibility(bool visible)
    {
        if (voiceChatWindowView == null)
            return;

        if (visible)
            voiceChatWindowView.Show();
        else
            voiceChatWindowView.Hide();
    }

    public void SetUsersMuted(string[] usersId, bool isMuted) 
    {
        for (int i = 0; i < usersId.Length; i++)
        {
            voiceChatWindowView.SetPlayerMuted(usersId[i], isMuted);
        }
    }

    public void SetUserRecording(string userId, bool isRecording)
    {
        voiceChatWindowView.SetPlayerRecording(userId, isRecording);
        SetWhichPlayerIsTalking();
    }

    public void SetVoiceChatRecording(bool recording) 
    { 
        voiceChatBarView.PlayVoiceChatRecordingAnimation(recording);
        isOwnPLayerTalking = recording;
        SetWhichPlayerIsTalking();
    }

    public void SetVoiceChatEnabledByScene(bool enabled)
    {
        if (voiceChatBarView == null)
            return;

        voiceChatBarView.SetVoiceChatEnabledByScene(enabled);
    }

    public void Dispose()
    {
        ReportMuteStatuses();

        if (updateMuteStatusRoutine != null)
            CoroutineStarter.Stop(updateMuteStatusRoutine);

        if (voiceChatWindowView != null)
        {
            voiceChatWindowView.OnClose -= CloseView;
            voiceChatWindowView.OnJoinVoiceChat -= JoinVoiceChat;
            voiceChatWindowView.OnGoToCrowd -= GoToCrowd;
            voiceChatWindowView.OnMuteAll -= OnMuteAllToggled;
            voiceChatWindowView.OnMuteUser -= MuteUser;
            voiceChatWindowView.OnAllowUsersFilterChange -= ChangeAllowUsersFilter;
        }

        if (voiceChatBarView != null)
            voiceChatBarView.OnJoinVoiceChat -= JoinVoiceChat;

        dataStore.player.otherPlayers.OnAdded -= OnOtherPlayersStatusAdded;
        dataStore.player.otherPlayers.OnRemoved -= OnOtherPlayerStatusRemoved;
        ownProfile.OnUpdate -= OnUserProfileUpdated;
        friendsController.OnUpdateFriendship -= OnUpdateFriendship;
        settings.generalSettings.OnChanged -= OnSettingsChanged;
        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
    }

    internal void CloseView() { SetVisibility(false); }

    internal void JoinVoiceChat(bool isJoined)
    { 
        voiceChatWindowView.SetAsJoined(isJoined);
        voiceChatBarView.SetAsJoined(isJoined);

        if (isJoined)
        {
            SetWhichPlayerIsTalking();
        }
        else
        {
            dataStore.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(false, false), true);
            isOwnPLayerTalking = false;
        }

        this.isJoined = isJoined;

        if (!isJoined)
        {
            WebInterface.LeaveVoiceChat();
            socialAnalytics.SendVoiceChannelDisconnection();
        }
        else
        {
            WebInterface.JoinVoiceChat();
            socialAnalytics.SendVoiceChannelConnection(voiceChatWindowView.numberOfPlayers);
        }

        dataStore.voiceChat.isJoinedToVoiceChat.Set(isJoined);
    }

    internal void GoToCrowd() { WebInterface.GoToCrowd(); }

    internal void OnOtherPlayersStatusAdded(string userId, Player player)
    {
        var otherProfile = userProfileBridge.Get(player.id);

        if (otherProfile == null)
            return;
        
        voiceChatWindowView.AddOrUpdatePlayer(otherProfile);

        if (!trackedUsersHashSet.Contains(userId))
        {
            trackedUsersHashSet.Add(userId);

            bool isMuted = ownProfile.muted.Contains(userId);
            voiceChatWindowView.SetPlayerMuted(userId, isMuted);
            voiceChatWindowView.SetPlayerBlocked(userId, ownProfile.blocked != null ? ownProfile.blocked.Contains(userId) : false);
            voiceChatWindowView.SetPlayerAsFriend(userId, friendsController.ContainsStatus(userId, FriendshipStatus.FRIEND));
            voiceChatWindowView.SetPlayerAsJoined(userId, dataStore.voiceChat.isJoinedToVoiceChat.Get());

            if (isMuteAll && !isMuted)
                MuteUser(userId, true);
        }

        SetWhichPlayerIsTalking();
    }

    internal void OnOtherPlayerStatusRemoved(string userId, Player player)
    {
        if (trackedUsersHashSet.Contains(userId))
            trackedUsersHashSet.Remove(userId);

        voiceChatWindowView.RemoveUser(userId);
        SetWhichPlayerIsTalking();
    }

    internal void OnMuteAllToggled(bool isMute)
    {
        isMuteAll = isMute;

        if (!isJoined)
            return;

        MuteAll(isMute);
    }

    internal void MuteAll(bool isMute)
    {
        isMuteAll = isMute;

        if (isMute)
            usersToUnmute.Clear();
        else
            usersToMute.Clear();

        using (var iterator = trackedUsersHashSet.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                MuteUser(iterator.Current, isMute);
            }
        }
    }

    internal void MuteUser(string userId, bool isMuted)
    {
        var list = isMuted ? usersToMute : usersToUnmute;
        list.Add(userId);

        if (updateMuteStatusRoutine == null)
            updateMuteStatusRoutine = CoroutineStarter.Start(MuteStateUpdateRoutine());

        if (isMuted)
            socialAnalytics.SendPlayerMuted(userId);
        else
            socialAnalytics.SendPlayerUnmuted(userId);
    }

    internal IEnumerator MuteStateUpdateRoutine()
    {
        yield return WaitForSecondsCache.Get(MUTE_STATUS_UPDATE_INTERVAL);
        ReportMuteStatuses();
        updateMuteStatusRoutine = null;
    }

    internal void ReportMuteStatuses()
    {
        if (usersToUnmute.Count > 0)
            WebInterface.SetMuteUsers(usersToUnmute.ToArray(), false);

        if (usersToMute.Count > 0)
            WebInterface.SetMuteUsers(usersToMute.ToArray(), true);

        usersToUnmute.Clear();
        usersToMute.Clear();
    }

    internal void ChangeAllowUsersFilter(string optionId)
    {
        var newSettings = settings.generalSettings.Data;

        if (optionId == VoiceChatAllow.ALL_USERS.ToString())
            newSettings.voiceChatAllow = VoiceChatAllow.ALL_USERS;
        else if (optionId == VoiceChatAllow.VERIFIED_ONLY.ToString())
            newSettings.voiceChatAllow = VoiceChatAllow.VERIFIED_ONLY;
        else if (optionId == VoiceChatAllow.FRIENDS_ONLY.ToString())
            newSettings.voiceChatAllow = VoiceChatAllow.FRIENDS_ONLY;

        settings.generalSettings.Apply(newSettings);
    }

    internal void OnUserProfileUpdated(UserProfile profile)
    {
        using (var iterator = trackedUsersHashSet.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                voiceChatWindowView.SetPlayerBlocked(iterator.Current, profile.blocked != null ? profile.blocked.Contains(iterator.Current) : false);
            }
        }
    }

    internal void OnUpdateFriendship(string userId, FriendshipAction action) { voiceChatWindowView.SetPlayerAsFriend(userId, action == FriendshipAction.APPROVED); }

    internal void OnSettingsChanged(GeneralSettings settings)
    {
        switch (settings.voiceChatAllow)
        {
            case VoiceChatAllow.ALL_USERS:
                voiceChatWindowView.SelectAllowUsersOption(0);
                break;
            case VoiceChatAllow.VERIFIED_ONLY:
                voiceChatWindowView.SelectAllowUsersOption(1);
                break;
            case VoiceChatAllow.FRIENDS_ONLY:
                voiceChatWindowView.SelectAllowUsersOption(2);
                break;
        }

        socialAnalytics.SendVoiceChatPreferencesChanged(settings.voiceChatAllow);
    }

    internal void RendererState_OnChange(bool current, bool previous)
    {
        if (!current)
            return;

        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
        JoinVoiceChat(true);
    }

    internal void SetWhichPlayerIsTalking()
    {
        if (isOwnPLayerTalking)
            voiceChatBarView.SetTalkingMessage(true, TALKING_MESSAGE_YOU);
        else if (voiceChatWindowView.numberOfPlayers == 0)
            voiceChatBarView.SetTalkingMessage(false, TALKING_MESSAGE_JUST_YOU_IN_THE_VOICE_CHAT);
        else if (voiceChatWindowView.numberOfPlayersTalking == 0)
            voiceChatBarView.SetTalkingMessage(false, TALKING_MESSAGE_NOBODY_TALKING);
        else if (voiceChatWindowView.numberOfPlayersTalking == 1)
        {
            UserProfile userProfile = userProfileBridge.Get(voiceChatWindowView.GetUserTalkingByIndex(0));
            voiceChatBarView.SetTalkingMessage(true, userProfile != null ? userProfile.userName : voiceChatWindowView.GetUserTalkingByIndex(0));
        }
        else
            voiceChatBarView.SetTalkingMessage(true, TALKING_MESSAGE_SEVERAL_PEOPLE_TALKING);
    }

    protected internal virtual IVoiceChatWindowComponentView CreateVoiceChatWindowView() => VoiceChatWindowComponentView.Create();

    protected internal virtual IVoiceChatBarComponentView CreateVoiceChatBatView() => VoiceChatBarComponentView.Create();

    protected internal virtual IVoiceChatPlayerComponentView CreateVoiceChatPlayerView() => VoiceChatPlayerComponentView.Create();
}
