using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionSceneContributorsSettingsController : SectionBase, ISelectSceneListener, ISectionUpdateSceneContributorsRequester
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionSceneContributorsSettingsView";
    
    public event Action<string, SceneContributorsUpdatePayload> OnRequestUpdateSceneContributors;

    private readonly SectionSceneContributorsSettingsView view;
    private readonly FriendsSearchPromptController friendsSearchPromptController;
    private readonly UserProfileFetcher profileFetcher = new UserProfileFetcher();
    private readonly SceneContributorsUpdatePayload contributorsUpdatePayload = new SceneContributorsUpdatePayload();

    private List<string> contributorsList = new List<string>();
    private string sceneId;

    public SectionSceneContributorsSettingsController() : this(
        Object.Instantiate(Resources.Load<SectionSceneContributorsSettingsView>(VIEW_PREFAB_PATH)),
        FriendsController.i
    )
    {
    }
    
    public SectionSceneContributorsSettingsController(SectionSceneContributorsSettingsView view, IFriendsController friendsController)
    {
        this.view = view;
        friendsSearchPromptController = new FriendsSearchPromptController(view.GetSearchPromptView(), friendsController);

        view.OnSearchUserButtonPressed += () => friendsSearchPromptController.Show();
        friendsSearchPromptController.OnAddUser += OnAddUserPressed;
        friendsSearchPromptController.OnRemoveUser += OnRemoveUserPressed;
    }

    public override void Dispose()
    {
        view.Dispose();
        profileFetcher.Dispose();
        friendsSearchPromptController.Dispose();
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
        UpdateContributors(sceneCardView.sceneData.contributors);
    }

    internal void UpdateContributors(string[] contributors)
    {
        if (contributors == null || contributors.Length == 0)
        {
            if (contributorsList.Count > 0)
                contributorsList.Clear();
            
            view.SetEmptyList(true);
            view.SetContributorsCount(0);
            return;
        }

        var newContributors = new List<string>(contributors);
        for (int i = 0; i < newContributors.Count; i++)
        {
            AddContributor(newContributors[i]);
            contributorsList.Remove(newContributors[i]);
        }
        
        for (int i = 0; i < contributorsList.Count; i++)
        {
            view.RemoveUser(contributorsList[i]);
        }
        
        contributorsList = newContributors;

        friendsSearchPromptController.SetUsersInRolList(contributorsList);
        view.SetEmptyList(false);
        view.SetContributorsCount(contributorsList.Count);
    }

    void AddContributor(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        var userView = view.AddUser(userId);
        profileFetcher.FetchProfile(userId)
                      .Then(userProfile => userView.SetUserProfile(userProfile));

        userView.OnAddPressed -= OnAddUserPressed;
        userView.OnRemovePressed -= OnRemoveUserPressed;
        userView.OnAddPressed += OnAddUserPressed;
        userView.OnRemovePressed += OnRemoveUserPressed;
    }

    void OnAddUserPressed(string userId)
    {
        if (contributorsList.Contains(userId))
            return;

        contributorsList.Add(userId);
        AddContributor(userId);
        friendsSearchPromptController.SetUsersInRolList(contributorsList);
        view.SetEmptyList(false);
        view.SetContributorsCount(contributorsList.Count);
        contributorsUpdatePayload.contributors = contributorsList.ToArray();
        OnRequestUpdateSceneContributors?.Invoke(sceneId, contributorsUpdatePayload);
    }
    
    void OnRemoveUserPressed(string userId)
    {
        if (!contributorsList.Remove(userId))
            return;

        view.RemoveUser(userId);
        friendsSearchPromptController.SetUsersInRolList(contributorsList);
        view.SetEmptyList(contributorsList.Count == 0);
        view.SetContributorsCount(contributorsList.Count);
        contributorsUpdatePayload.contributors = contributorsList.ToArray();
        OnRequestUpdateSceneContributors?.Invoke(sceneId, contributorsUpdatePayload);
    }
}
