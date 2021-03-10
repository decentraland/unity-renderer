using System.Collections;
using System.Collections.Generic;
using DCL.Interface;
using UnityEngine;

public class UsersAroundListHUDController : IHUD
{
    const float MUTE_STATUS_UPDATE_INTERVAL = 0.3f;

    internal IUsersAroundListHUDButtonView usersButtonView;
    internal IUsersAroundListHUDListView usersListView;

    private bool isVisible = false;
    private readonly HashSet<string> trackedUsersHashSet = new HashSet<string>();
    private UserProfile profile => UserProfile.GetOwnUserProfile();

    private readonly List<string> usersToMute = new List<string>();
    private readonly List<string> usersToUnmute = new List<string>();
    private bool isMuteAll = false;
    private Coroutine updateMuteStatusRoutine = null;

    public event System.Action OnOpen;

    public UsersAroundListHUDController()
    {
        UsersAroundListHUDListView view = Object.Instantiate(Resources.Load<GameObject>("UsersAroundListHUD")).GetComponent<UsersAroundListHUDListView>();
        view.name = "_UsersAroundListHUD";
        view.gameObject.SetActive(false);
        Initialize(view);
    }

    public UsersAroundListHUDController(IUsersAroundListHUDListView usersListView)
    {
        Initialize(usersListView);
    }

    /// <summary>
    /// Dispose HUD controller
    /// </summary>
    public void Dispose()
    {
        ReportMuteStatuses();

        if (updateMuteStatusRoutine != null)
        {
            CoroutineStarter.Stop(updateMuteStatusRoutine);
        }

        MinimapMetadata.GetMetadata().OnUserInfoUpdated -= MapRenderer_OnUserInfoUpdated;
        MinimapMetadata.GetMetadata().OnUserInfoRemoved -= MapRenderer_OnUserInfoRemoved;

        CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
        profile.OnUpdate -= OnUserProfileUpdate;

        if (usersListView != null)
        {
            usersListView.OnRequestMuteUser -= OnMuteUser;
            usersListView.OnRequestMuteGlobal -= OnMuteAll;
            usersListView.OnGoToCrowdPressed -= OnGoToCrowd;
            usersListView.OnOpen -= OnListOpen;

            usersListView.Dispose();
        }

        if (usersButtonView != null)
        {
            usersButtonView.OnClick -= ToggleVisibility;
        }
    }

    /// <summary>
    /// Set HUD's visibility
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisibility(bool visible)
    {
        isVisible = visible;
        usersListView.SetVisibility(visible);
    }

    /// <summary>
    /// Set button to toggle HUD visibility and display users count
    /// </summary>
    /// <param name="view">Button view</param>
    public void SetButtonView(IUsersAroundListHUDButtonView view)
    {
        usersButtonView = view;
        usersButtonView.OnClick += ToggleVisibility;
    }

    /// <summary>
    /// Set mute status for users' id
    /// </summary>
    /// <param name="usersId">Array of user ids</param>
    /// <param name="isMuted">Set if users should be mute or unmute</param>
    public void SetUsersMuted(string[] usersId, bool isMuted)
    {
        for (int i = 0; i < usersId.Length; i++)
        {
            usersListView.SetUserMuted(usersId[i], isMuted);
        }
    }

    /// <summary>
    /// Set user status as "talking"
    /// </summary>
    /// <param name="userId">User's id</param>
    /// <param name="isRecording">Set user status as "talking" or "not talking"</param>
    public void SetUserRecording(string userId, bool isRecording)
    {
        usersListView.SetUserRecording(userId, isRecording);
    }

    void Initialize(IUsersAroundListHUDListView view)
    {
        usersListView = view;

        usersListView.OnRequestMuteUser += OnMuteUser;
        usersListView.OnRequestMuteGlobal += OnMuteAll;
        usersListView.OnGoToCrowdPressed += OnGoToCrowd;
        usersListView.OnOpen += OnListOpen;

        MinimapMetadata.GetMetadata().OnUserInfoUpdated += MapRenderer_OnUserInfoUpdated;
        MinimapMetadata.GetMetadata().OnUserInfoRemoved += MapRenderer_OnUserInfoRemoved;

        CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
        profile.OnUpdate += OnUserProfileUpdate;
    }

    void MapRenderer_OnUserInfoUpdated(MinimapMetadata.MinimapUserInfo userInfo)
    {
        usersListView.AddOrUpdateUser(userInfo);

        if (!trackedUsersHashSet.Contains(userInfo.userId))
        {
            trackedUsersHashSet.Add(userInfo.userId);

            bool isMuted = profile.muted.Contains(userInfo.userId);
            bool isBlocked = profile.blocked != null ? profile.blocked.Contains(userInfo.userId) : false;

            usersListView.SetUserMuted(userInfo.userId, isMuted);
            usersListView.SetUserBlocked(userInfo.userId, isBlocked);

            if (isMuteAll && !isMuted)
            {
                OnMuteUser(userInfo.userId, true);
            }
        }

        usersButtonView?.SetUsersCount(trackedUsersHashSet.Count);
    }

    void MapRenderer_OnUserInfoRemoved(string userId)
    {
        if (trackedUsersHashSet.Contains(userId))
        {
            trackedUsersHashSet.Remove(userId);
            usersButtonView?.SetUsersCount(trackedUsersHashSet.Count);
        }
        usersListView.RemoveUser(userId);
    }

    void ToggleVisibility()
    {
        bool setVisible = !isVisible;
        SetVisibility(setVisible);
    }

    void OnMuteUser(string userId, bool mute)
    {
        var list = mute ? usersToMute : usersToUnmute;
        list.Add(userId);

        if (updateMuteStatusRoutine == null)
        {
            updateMuteStatusRoutine = CoroutineStarter.Start(MuteStateUpdateRoutine());
        }
    }

    void OnMuteUsers(IEnumerable<string> usersId, bool mute)
    {
        using (var iterator = usersId.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                OnMuteUser(iterator.Current, mute);
            }
        }
    }

    void OnMuteAll(bool mute)
    {
        isMuteAll = mute;

        if (mute)
        {
            usersToUnmute.Clear();
        }
        else
        {
            usersToMute.Clear();
        }
        OnMuteUsers(trackedUsersHashSet, mute);
    }

    void OnGoToCrowd()
    {
        WebInterface.GoToCrowd();
    }

    void OnListOpen()
    {
        OnOpen?.Invoke();
    }

    private void OnRendererStateChanged(bool isEnable, bool prevState)
    {
        if (isEnable || !isVisible)
            return;

        SetVisibility(false);
    }

    private void OnUserProfileUpdate(UserProfile profile)
    {
        using (var iterator = trackedUsersHashSet.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                usersListView.SetUserBlocked(iterator.Current, profile.blocked != null ? profile.blocked.Contains(iterator.Current) : false);
            }
        }
    }

    void ReportMuteStatuses()
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

    IEnumerator MuteStateUpdateRoutine()
    {
        yield return WaitForSecondsCache.Get(MUTE_STATUS_UPDATE_INTERVAL);
        ReportMuteStatuses();
        updateMuteStatusRoutine = null;
    }
}
