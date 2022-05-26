using DCL;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceChatWindowController : IHUD
{
    const float MUTE_STATUS_UPDATE_INTERVAL = 0.3f;

    IVoiceChatWindowComponentView view;
    ISocialAnalytics socialAnalytics;

    public IVoiceChatWindowComponentView View => view;
    
    private UserProfile profile => UserProfile.GetOwnUserProfile();
    private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

    private readonly HashSet<string> trackedUsersHashSet = new HashSet<string>();
    private readonly List<string> usersToMute = new List<string>();
    private readonly List<string> usersToUnmute = new List<string>();
    private Coroutine updateMuteStatusRoutine = null;    

    public VoiceChatWindowController(ISocialAnalytics socialAnalytics)
    {
        this.socialAnalytics = socialAnalytics;

        view = CreateView();
        view.Hide(instant: true);
        view.OnClose += OnViewClosed;
        view.OnRequestMuteUser += OnMuteUser;
        otherPlayers.OnAdded += OnOtherPlayersStatusAdded;
        otherPlayers.OnRemoved += OnOtherPlayerStatusRemoved;
        profile.OnUpdate += OnUserProfileUpdate;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            view.Show();
        else
            view.Hide();
    }

    public void Dispose()
    {
        ReportMuteStatuses();

        if (updateMuteStatusRoutine != null)
            CoroutineStarter.Stop(updateMuteStatusRoutine);

        view.OnClose -= OnViewClosed;
        view.OnRequestMuteUser -= OnMuteUser;
        otherPlayers.OnAdded -= OnOtherPlayersStatusAdded;
        otherPlayers.OnRemoved -= OnOtherPlayerStatusRemoved;
        profile.OnUpdate -= OnUserProfileUpdate;
    }

    internal void OnViewClosed() { SetVisibility(false); }

    internal void OnMuteUser(string userId, bool mute)
    {
        var list = mute ? usersToMute : usersToUnmute;
        list.Add(userId);

        if (updateMuteStatusRoutine == null)
        {
            updateMuteStatusRoutine = CoroutineStarter.Start(MuteStateUpdateRoutine());
        }

        if (mute)
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
        {
            WebInterface.SetMuteUsers(usersToUnmute.ToArray(), false);
        }
        if (usersToMute.Count > 0)
        {
            WebInterface.SetMuteUsers(usersToMute.ToArray(), true);
        }
        usersToUnmute.Clear();
        usersToMute.Clear();
    }

    internal void OnOtherPlayersStatusAdded(string userId, Player player)
    {
        view.AddOrUpdatePlayer(player);

        if (!trackedUsersHashSet.Contains(userId))
        {
            trackedUsersHashSet.Add(userId);

            bool isMuted = profile.muted.Contains(userId);
            bool isBlocked = profile.blocked != null ? profile.blocked.Contains(userId) : false;

            view.SetUserMuted(userId, isMuted);
            view.SetUserBlocked(userId, isBlocked);
        }
    }

    internal void OnOtherPlayerStatusRemoved(string userId, Player player)
    {
        if (trackedUsersHashSet.Contains(userId))
            trackedUsersHashSet.Remove(userId);

        view.RemoveUser(userId);
    }

    private void OnUserProfileUpdate(UserProfile profile)
    {
        using (var iterator = trackedUsersHashSet.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                view.SetUserBlocked(iterator.Current, profile.blocked != null ? profile.blocked.Contains(iterator.Current) : false);
            }
        }
    }

    protected internal virtual IVoiceChatWindowComponentView CreateView() => VoiceChatWindowComponentView.Create();
}
