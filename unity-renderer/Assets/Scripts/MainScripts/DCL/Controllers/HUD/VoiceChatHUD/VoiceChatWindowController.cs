using DCL;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoiceChatWindowController : IHUD
{
    const float MUTE_STATUS_UPDATE_INTERVAL = 0.3f;
    private const string TALKING_MESSAGE_YOU = "You";
    private const string TALKING_MESSAGE_JUST_YOU_IN_THE_VOICE_CHAT = "Just you in the voice chat";
    private const string TALKING_MESSAGE_NOBODY_TALKING = "Nobody is talking";
    private const string TALKING_MESSAGE_SEVERAL_PEOPLE_TALKING = "Several people talking";

    public IVoiceChatWindowComponentView VoiceChatWindowView => voiceChatWindowView;
    public IVoiceChatBarComponentView VoiceChatBarView => voiceChatBarView;

    private UserProfile ownProfile => userProfileBridge.GetOwn();

    private IVoiceChatWindowComponentView voiceChatWindowView;
    private IVoiceChatBarComponentView voiceChatBarView;
    private IUserProfileBridge userProfileBridge;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private DataStore dataStore;
    private readonly HashSet<string> trackedUsersHashSet = new HashSet<string>();
    private readonly Queue<VoiceChatPlayerComponentView> playersPool;
    private readonly Dictionary<string, VoiceChatPlayerComponentView> currentPlayers;
    private readonly List<string> usersToMute = new List<string>();
    private readonly List<string> usersToUnmute = new List<string>();
    private readonly List<string> usersTalking = new List<string>();
    private bool isOwnPLayerTalking = false;
    private Coroutine updateMuteStatusRoutine = null;
    private bool isMuteAll = false;
    private bool isOpenByFirstTime = true;
    private bool isJoined = false;

    public VoiceChatWindowController(
        IUserProfileBridge userProfileBridge,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics,
        DataStore dataStore)
    {
        this.userProfileBridge = userProfileBridge;
        this.friendsController = friendsController;
        this.socialAnalytics = socialAnalytics;
        this.dataStore = dataStore;

        voiceChatWindowView = CreateVoiceChatWindowView();
        voiceChatWindowView.Hide(instant: true);
        voiceChatWindowView.OnClose += CloseView;
        voiceChatWindowView.OnJoinVoiceChat += JoinVoiceChat;
        voiceChatWindowView.OnGoToCrowd += GoToCrowd;
        voiceChatWindowView.OnMuteAll += MuteAll;
        voiceChatWindowView.SetNumberOfPlayers(0);

        voiceChatBarView = CreateVoiceChatBatView();
        voiceChatBarView.Hide(instant: true);
        voiceChatBarView.OnLeaveVoiceChat += LeaveVoiceChat;

        dataStore.player.otherPlayers.OnAdded += OnOtherPlayersStatusAdded;
        dataStore.player.otherPlayers.OnRemoved += OnOtherPlayerStatusRemoved;
        ownProfile.OnUpdate += OnUserProfileUpdated;
        friendsController.OnUpdateFriendship += OnUpdateFriendship;

        currentPlayers = new Dictionary<string, VoiceChatPlayerComponentView>();
        playersPool = new Queue<VoiceChatPlayerComponentView>();

        JoinVoiceChat(false);
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            voiceChatWindowView.Show();

            if (isOpenByFirstTime)
                JoinVoiceChat(true);

            isOpenByFirstTime = false;
        }
        else
            voiceChatWindowView.Hide();
    }

    public void SetUsersMuted(string[] usersId, bool isMuted)
    {
        for (int i = 0; i < usersId.Length; i++)
        {
            if (!currentPlayers.TryGetValue(usersId[i], out VoiceChatPlayerComponentView elementView))
                continue;
                
            elementView.SetAsMuted(isMuted);
        }

        CheckMuteAllState();
    }

    public void SetUserRecording(string userId, bool isRecording)
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

        SetWhichPlayerIsTalking();
    }

    public void SetVoiceChatRecording(bool recording) 
    { 
        voiceChatBarView.PlayVoiceChatRecordingAnimation(recording);
        isOwnPLayerTalking = recording;
        SetWhichPlayerIsTalking();
    }

    public void SetVoiceChatEnabledByScene(bool enabled) { voiceChatBarView.SetVoiceChatEnabledByScene(enabled); }

    public void Dispose()
    {
        ReportMuteStatuses();

        if (updateMuteStatusRoutine != null)
            CoroutineStarter.Stop(updateMuteStatusRoutine);

        voiceChatWindowView.OnClose -= CloseView;
        voiceChatWindowView.OnJoinVoiceChat -= JoinVoiceChat;
        voiceChatWindowView.OnGoToCrowd -= GoToCrowd;
        voiceChatWindowView.OnMuteAll -= MuteAll;
        voiceChatBarView.OnLeaveVoiceChat -= LeaveVoiceChat;
        dataStore.player.otherPlayers.OnAdded -= OnOtherPlayersStatusAdded;
        dataStore.player.otherPlayers.OnRemoved -= OnOtherPlayerStatusRemoved;
        ownProfile.OnUpdate -= OnUserProfileUpdated;
        friendsController.OnUpdateFriendship -= OnUpdateFriendship;

        currentPlayers.Clear();
        playersPool.Clear();
    }

    internal void CloseView() { SetVisibility(false); }

    internal void JoinVoiceChat(bool isJoined)
    { 
        dataStore.player.isJoinedToVoiceChat.Set(isJoined);

        using (var iterator = currentPlayers.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.SetAsJoined(isJoined);
            }
        }

        voiceChatWindowView.SetAsJoined(isJoined);

        if (isJoined)
        {
            voiceChatBarView.Show();
            SetWhichPlayerIsTalking();
        }
        else
        {
            voiceChatBarView.Hide();
        }

        this.isJoined = isJoined;

        if (!isJoined)
            MuteAll(true);
        else if (!isMuteAll)
            MuteAll(false);
    }

    internal void LeaveVoiceChat() { JoinVoiceChat(false); }

    internal void GoToCrowd() { WebInterface.GoToCrowd(); }

    internal void OnOtherPlayersStatusAdded(string userId, Player player)
    {
        if (!currentPlayers.ContainsKey(player.id))
        {
            var otherProfile = UserProfileController.userProfilesCatalog.Get(player.id);

            if (otherProfile != null)
            {
                VoiceChatPlayerComponentView elementView = null;
                if (playersPool.Count > 0)
                    elementView = playersPool.Dequeue();
                else
                {
                    elementView = voiceChatWindowView.CreateNewPlayerInstance();
                    elementView.OnMuteUser += MuteUser;
                    elementView.OnContextMenuOpen += OpenContextMenu;
                }

                elementView.Configure(new VoiceChatPlayerComponentModel
                {
                    userId = otherProfile.userId,
                    userImageUrl = otherProfile.face256SnapshotURL,
                    userName = otherProfile.userName,
                    isMuted = false,
                    isTalking = false,
                    isBlocked = false,
                    isFriend = friendsController.GetFriends().ContainsKey(userId),
                    isJoined = dataStore.player.isJoinedToVoiceChat.Get(),
                    isBackgroundHover = false
                });

                elementView.SetActive(true);
                currentPlayers.Add(player.id, elementView);
                voiceChatWindowView.SetNumberOfPlayers(currentPlayers.Count);
            }
        }

        if (!trackedUsersHashSet.Contains(userId))
        {
            trackedUsersHashSet.Add(userId);

            bool isMuted = ownProfile.muted.Contains(userId);
            bool isBlocked = ownProfile.blocked != null ? ownProfile.blocked.Contains(userId) : false;

            if (currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            {
                elementView.SetAsMuted(isMuted);
                elementView.SetAsBlocked(isBlocked);
            }

            if ((isMuteAll || !isJoined) && !isMuted)
                MuteUser(userId, true);
        }

        SetWhichPlayerIsTalking();
        CheckMuteAllState();
    }

    internal void OnOtherPlayerStatusRemoved(string userId, Player player)
    {
        if (trackedUsersHashSet.Contains(userId))
            trackedUsersHashSet.Remove(userId);

        if (!currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        if (!elementView)
            return;

        playersPool.Enqueue(elementView);
        currentPlayers.Remove(userId);
        voiceChatWindowView.SetNumberOfPlayers(currentPlayers.Count);

        elementView.SetActive(false);

        SetWhichPlayerIsTalking();
        CheckMuteAllState();
    }

    internal void MuteAll(bool isMute)
    {
        if (isJoined)
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

    internal void OpenContextMenu(string userId)
    {
        currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView);
        if (elementView != null)
            elementView.DockAndOpenUserContextMenu(voiceChatWindowView.ContextMenuPanel);
    }

    internal void OnUserProfileUpdated(UserProfile profile)
    {
        using (var iterator = trackedUsersHashSet.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (currentPlayers.TryGetValue(iterator.Current, out VoiceChatPlayerComponentView elementView))
                    elementView.SetAsBlocked(profile.blocked != null ? profile.blocked.Contains(iterator.Current) : false);
            }
        }
    }

    internal void OnUpdateFriendship(string userId, FriendshipAction action)
    {
        currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView playerView);
        
        if (playerView != null)
            playerView.SetAsFriend(action == FriendshipAction.APPROVED);
    }

    internal void SetWhichPlayerIsTalking()
    {
        if (isOwnPLayerTalking)
            voiceChatBarView.SetTalkingMessage(true, TALKING_MESSAGE_YOU);
        else if (currentPlayers.Count == 0)
            voiceChatBarView.SetTalkingMessage(false, TALKING_MESSAGE_JUST_YOU_IN_THE_VOICE_CHAT);
        else if (usersTalking.Count == 0)
            voiceChatBarView.SetTalkingMessage(false, TALKING_MESSAGE_NOBODY_TALKING);
        else if (usersTalking.Count == 1)
        {
            UserProfile userProfile = userProfileBridge.Get(usersTalking[0]);
            voiceChatBarView.SetTalkingMessage(true, userProfile != null ? userProfile.userName : usersTalking[0]);
        }
        else
            voiceChatBarView.SetTalkingMessage(true, TALKING_MESSAGE_SEVERAL_PEOPLE_TALKING);
    }

    internal void CheckMuteAllState()
    {
        isMuteAll = currentPlayers.Count(x => x.Value.model.isMuted) == currentPlayers.Count();
        voiceChatWindowView.SetMuteAllIsOn(isMuteAll, false);
    }

    protected internal virtual IVoiceChatWindowComponentView CreateVoiceChatWindowView() => VoiceChatWindowComponentView.Create();

    protected internal virtual IVoiceChatBarComponentView CreateVoiceChatBatView() => VoiceChatBarComponentView.Create();

    protected internal virtual IVoiceChatPlayerComponentView CreateVoiceChatPlayerView() => VoiceChatPlayerComponentView.Create();
}
