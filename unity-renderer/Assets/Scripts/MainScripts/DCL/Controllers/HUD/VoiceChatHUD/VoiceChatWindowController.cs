using DCL;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceChatWindowController : IHUD
{
    const float MUTE_STATUS_UPDATE_INTERVAL = 0.3f;

    public IVoiceChatWindowComponentView View => view;
    
    private UserProfile ownProfile => userProfileBridge.GetOwn();

    private IVoiceChatWindowComponentView view;
    private IUserProfileBridge userProfileBridge;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private DataStore dataStore;
    private readonly HashSet<string> trackedUsersHashSet = new HashSet<string>();
    private readonly Queue<VoiceChatPlayerComponentView> playersPool;
    private readonly Dictionary<string, VoiceChatPlayerComponentView> currentPlayers;
    private readonly List<string> usersToMute = new List<string>();
    private readonly List<string> usersToUnmute = new List<string>();
    private Coroutine updateMuteStatusRoutine = null;    

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

        view = CreateVoiceChatWindowView();
        view.Hide(instant: true);
        view.OnClose += CloseView;
        view.OnJoinVoiceChat += JoinVoiceChat;
        dataStore.player.otherPlayers.OnAdded += OnOtherPlayersStatusAdded;
        dataStore.player.otherPlayers.OnRemoved += OnOtherPlayerStatusRemoved;
        ownProfile.OnUpdate += OnUserProfileUpdated;
        friendsController.OnUpdateFriendship += OnUpdateFriendship;

        currentPlayers = new Dictionary<string, VoiceChatPlayerComponentView>();
        playersPool = new Queue<VoiceChatPlayerComponentView>();
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            view.Show();
        else
            view.Hide();
    }

    public void SetUsersMuted(string[] usersId, bool isMuted)
    {
        for (int i = 0; i < usersId.Length; i++)
        {
            if (!currentPlayers.TryGetValue(usersId[i], out VoiceChatPlayerComponentView elementView))
                continue;
                
            elementView.SetAsMuted(isMuted);
        }
    }

    public void SetUserRecording(string userId, bool isRecording)
    {
        if (!currentPlayers.TryGetValue(userId, out VoiceChatPlayerComponentView elementView))
            return;

        elementView.SetAsTalking(isRecording);
    }

    public void Dispose()
    {
        ReportMuteStatuses();

        if (updateMuteStatusRoutine != null)
            CoroutineStarter.Stop(updateMuteStatusRoutine);

        view.OnClose -= CloseView;
        view.OnJoinVoiceChat -= JoinVoiceChat;
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
    }

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
                    elementView = view.CreateNewPlayerInstance();
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
                    isFriend = friendsController.GetFriends().ContainsKey(userId),
                    isJoined = dataStore.player.isJoinedToVoiceChat.Get(),
                    isBackgroundHover = false
                });

                elementView.SetActive(true);
                currentPlayers.Add(player.id, elementView);
                view.SetNumberOfPlayers(currentPlayers.Count);
                CheckListEmptyState();
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
        }
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
        view.SetNumberOfPlayers(currentPlayers.Count);
        CheckListEmptyState();

        elementView.SetActive(false);
    }

    internal void CheckListEmptyState() { view.SetEmptyListActive(currentPlayers.Count == 0); }

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

    protected internal virtual IVoiceChatWindowComponentView CreateVoiceChatWindowView() => VoiceChatWindowComponentView.Create();

    protected internal virtual IVoiceChatPlayerComponentView CreateVoiceChatPlayerView() => VoiceChatPlayerComponentView.Create();
}
