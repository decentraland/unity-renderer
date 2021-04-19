using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionSceneAdminsSettingsController : SectionBase, ISelectSceneListener, 
                                                      ISectionUpdateSceneAdminsRequester, ISectionUpdateSceneBannedUsersRequester
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionSceneAdminsSettingsView";
    
    public event Action<string, SceneAdminsUpdatePayload> OnRequestUpdateSceneAdmins;
    public event Action<string, SceneBannedUsersUpdatePayload> OnRequestUpdateSceneBannedUsers;

    private readonly SectionSceneAdminsSettingsView view;
    private readonly FriendsSearchPromptController friendsSearchPromptController;
    private readonly UsersSearchPromptController usersSearchPromptController;
    private readonly UserProfileFetcher profileFetcher = new UserProfileFetcher();
    
    private readonly SceneAdminsUpdatePayload adminsUpdatePayload = new SceneAdminsUpdatePayload();
    private readonly SceneBannedUsersUpdatePayload bannedUsersUpdatePayload = new SceneBannedUsersUpdatePayload();

    private List<string> admins = new List<string>();
    private List<string> bannedUsers = new List<string>();
    private string sceneId;

    public SectionSceneAdminsSettingsController() : this(
        Object.Instantiate(Resources.Load<SectionSceneAdminsSettingsView>(VIEW_PREFAB_PATH)),
        FriendsController.i
    )
    {
    }
    
    public SectionSceneAdminsSettingsController(SectionSceneAdminsSettingsView view, IFriendsController friendsController)
    {
        this.view = view;
        friendsSearchPromptController = new FriendsSearchPromptController(view.GetAdminsSearchPromptView(), friendsController);
        usersSearchPromptController = new UsersSearchPromptController(view.GetBlockedSearchPromptView());

        view.OnSearchFriendButtonPressed += () => friendsSearchPromptController.Show();
        view.OnSearchUserButtonPressed += () => usersSearchPromptController.Show();
        
        friendsSearchPromptController.OnAddUser += OnAddAdminPressed;
        friendsSearchPromptController.OnRemoveUser += OnRemoveAdminPressed;
        usersSearchPromptController.OnAddUser += OnAddBannedUserPressed;
        usersSearchPromptController.OnRemoveUser += OnRemoveBannedUserPressed;
    }

    public override void Dispose()
    {
        view.Dispose();
        profileFetcher.Dispose();
        friendsSearchPromptController.Dispose();
        usersSearchPromptController.Dispose();
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.SetParent(viewContainer);
    }

    protected override void OnShow()
    {
        view.SetActive(true);
    }

    protected override void OnHide()
    {
        view.SetActive(false);
    }
    
    void ISelectSceneListener.OnSelectScene(SceneCardView sceneCardView)
    {
        sceneId = sceneCardView.sceneData.id;
        SetAdmins(sceneCardView.sceneData.admins);
        SetBannedUsers(sceneCardView.sceneData.bannedUsers);
    }

    internal void SetAdmins(string[] usersId)
    {
        if (usersId == null || usersId.Length == 0)
        {
            if (admins.Count > 0)
                admins.Clear();
            
            view.SetAdminsEmptyList(true);
            view.SetAdminsCount(0);
            return;
        }

        var newAdmins = new List<string>(usersId);
        for (int i = 0; i < newAdmins.Count; i++)
        {
            AddAdmin(newAdmins[i]);
            admins.Remove(newAdmins[i]);
        }
        
        for (int i = 0; i < admins.Count; i++)
        {
            view.RemoveAdmin(admins[i]);
        }
        
        admins = newAdmins;

        friendsSearchPromptController.SetUsersInRolList(admins);
        view.SetAdminsEmptyList(false);
        view.SetAdminsCount(admins.Count);
    }
    
    internal void SetBannedUsers(string[] usersId)
    {
        if (usersId == null || usersId.Length == 0)
        {
            if (bannedUsers.Count > 0)
                bannedUsers.Clear();
            
            view.SetBannedUsersEmptyList(true);
            view.SetBannedUsersCount(0);
            return;
        }

        var newBlocked = new List<string>(usersId);
        for (int i = 0; i < newBlocked.Count; i++)
        {
            AddBannedUser(newBlocked[i]);
            bannedUsers.Remove(newBlocked[i]);
        }
        
        for (int i = 0; i < bannedUsers.Count; i++)
        {
            view.RemoveBannedUser(bannedUsers[i]);
        }
        
        bannedUsers = newBlocked;

        usersSearchPromptController.SetUsersInRolList(bannedUsers);
        view.SetBannedUsersEmptyList(false);
        view.SetBannedUsersCount(bannedUsers.Count);
    }

    void AddAdmin(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        var userView = view.AddAdmin(userId);
        profileFetcher.FetchProfile(userId)
                      .Then(userProfile => userView.SetUserProfile(userProfile));

        userView.OnAddPressed -= OnAddAdminPressed;
        userView.OnRemovePressed -= OnRemoveAdminPressed;
        userView.OnAddPressed += OnAddAdminPressed;
        userView.OnRemovePressed += OnRemoveAdminPressed;
    }
    
    void AddBannedUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        var userView = view.AddBannedUser(userId);
        profileFetcher.FetchProfile(userId)
                      .Then(userProfile => userView.SetUserProfile(userProfile));

        userView.OnAddPressed -= OnAddBannedUserPressed;
        userView.OnRemovePressed -= OnRemoveBannedUserPressed;
        userView.OnAddPressed += OnAddBannedUserPressed;
        userView.OnRemovePressed += OnRemoveBannedUserPressed;
    }

    void OnAddAdminPressed(string userId)
    {
        if (admins.Contains(userId))
            return;

        admins.Add(userId);
        AddAdmin(userId);
        friendsSearchPromptController.SetUsersInRolList(admins);
        view.SetAdminsEmptyList(false);
        view.SetAdminsCount(admins.Count);
        adminsUpdatePayload.admins = admins.ToArray();
        OnRequestUpdateSceneAdmins?.Invoke(sceneId, adminsUpdatePayload);
    }
    
    void OnRemoveAdminPressed(string userId)
    {
        if (!admins.Remove(userId))
            return;

        view.RemoveAdmin(userId);
        friendsSearchPromptController.SetUsersInRolList(admins);
        view.SetAdminsEmptyList(admins.Count == 0);
        view.SetAdminsCount(admins.Count);
        adminsUpdatePayload.admins = admins.ToArray();
        OnRequestUpdateSceneAdmins?.Invoke(sceneId, adminsUpdatePayload);
    }
    
    void OnAddBannedUserPressed(string userId)
    {
        if (bannedUsers.Contains(userId))
            return;

        bannedUsers.Add(userId);
        AddBannedUser(userId);
        usersSearchPromptController.SetUsersInRolList(bannedUsers);
        view.SetBannedUsersEmptyList(false);
        view.SetBannedUsersCount(bannedUsers.Count);
        bannedUsersUpdatePayload.bannedUsers = bannedUsers.ToArray();
        OnRequestUpdateSceneBannedUsers?.Invoke(sceneId, bannedUsersUpdatePayload);
    }
    
    void OnRemoveBannedUserPressed(string userId)
    {
        if (!bannedUsers.Remove(userId))
            return;

        view.RemoveBannedUser(userId);
        usersSearchPromptController.SetUsersInRolList(bannedUsers);
        view.SetBannedUsersEmptyList(bannedUsers.Count == 0);
        view.SetBannedUsersCount(bannedUsers.Count);
        bannedUsersUpdatePayload.bannedUsers = bannedUsers.ToArray();
        OnRequestUpdateSceneBannedUsers?.Invoke(sceneId, bannedUsersUpdatePayload);
    }
}
