using System;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

internal class UsersSearchPromptController: IDisposable
{
    private const int MAX_USERS_RESULT = 10;
    
    public event Action<string> OnRemoveUser;
    public event Action<string> OnAddUser;
    
    private readonly UsersSearchPromptView view;
    internal readonly UsersSearchUserViewsHandler userViewsHandler;
    private readonly UsersSearcher usersSearcher;

    internal Promise<UserProfileModel[]> usersSearchPromise = null;
    
    public UsersSearchPromptController(UsersSearchPromptView promptView)
    {
        view = promptView;
        view.SetIdleSearchTime(1.5f);
        
        userViewsHandler = new UsersSearchUserViewsHandler(view.GetUsersBaseElement(), view.GetUserElementsParent());
        usersSearcher = new UsersSearcher();

        view.OnSearchText += OnSearchText;
        view.OnShouldHide += OnShouldHidePrompt;
        userViewsHandler.OnAddUser += OnAddUserPressed;
        userViewsHandler.OnRemoveUser += OnRemoveUserPressed;
    }
    
    public void Dispose()
    {
        view.OnSearchText -= OnSearchText;
        view.OnShouldHide -= OnShouldHidePrompt;
        userViewsHandler.OnAddUser -= OnAddUserPressed;
        userViewsHandler.OnRemoveUser -= OnRemoveUserPressed;

        usersSearchPromise?.Dispose();
        userViewsHandler.Dispose();
        view.Dispose();
    }

    public void Show()
    {
        view.ClearSearch();
        view.SetFriendListEmpty(true);
        view.Show();
    }

    public void Hide()
    {
        view.Hide();
        usersSearchPromise?.Dispose();
    }

    public void SetUsersInRolList(List<string> usersId)
    {
        userViewsHandler.SetUsersInRolList(usersId);
    }

    private void OnSearchText(string searchText)
    {
        usersSearchPromise?.Dispose();

        view.ShowSearchSpinner();
        usersSearchPromise = usersSearcher.SearchUser(searchText, MAX_USERS_RESULT);
        usersSearchPromise.Then(
            result =>
            {
                view.ShowClearButton();
                if (result == null || result.Length == 0)
                {
                    view.SetFriendListEmpty(true);
                }
                else
                {
                    view.SetFriendListEmpty(false);
                    userViewsHandler.SetUserViewsList(result);
                }
            });
    }

    private void OnShouldHidePrompt()
    {
        Hide();
    }

    private void OnAddUserPressed(string userId)
    {
        OnAddUser?.Invoke(userId);
    }
    
    private void OnRemoveUserPressed(string userId)
    {
        OnRemoveUser?.Invoke(userId);
    }
}
