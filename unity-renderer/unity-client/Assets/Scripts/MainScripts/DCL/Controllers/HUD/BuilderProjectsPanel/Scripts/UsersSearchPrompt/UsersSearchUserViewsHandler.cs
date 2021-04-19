using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class UsersSearchUserViewsHandler : IDisposable
{
    public event Action<string> OnRemoveUser;
    public event Action<string> OnAddUser;
    
    internal readonly Dictionary<string, UserElementView> userElementViews = new Dictionary<string, UserElementView>();
    private readonly Queue<UserElementView> viewPool = new Queue<UserElementView>();

    private readonly UserElementView userElementViewBase;
    private readonly Transform elementsParent;
    
    private List<string> usersInRolList;
    
    public int userElementViewCount => userElementViews.Count;

    public UsersSearchUserViewsHandler(UserElementView userElementViewBase, Transform elementsParent)
    {
        this.userElementViewBase = userElementViewBase;
        this.elementsParent = elementsParent;
        PoolUserView(userElementViewBase);
    }
    
    public void Dispose()
    {
        foreach (UserElementView userView in userElementViews.Values)
        {
            userView.Dispose();
        }
        userElementViews.Clear();

        while (viewPool.Count > 0)
        {
            viewPool.Dequeue().Dispose();
        }
    }
    
    public void SetUserViewsList(List<UserProfile> profiles)
    {
        UserProfile profile;
        for (int i = 0; i < profiles.Count; i++)
        {
            profile = profiles[i];
            if (profile == null) 
                continue;

            SetUserViewList(profile.userId,
                view =>
                {
                    view.SetUserProfile(profile);
                });
        }
    }
    
    public void SetUserViewsList(UserProfileModel[] profiles)
    {
        List<UserElementView> newUserList = new List<UserElementView>();
        
        UserProfileModel profile;
        for (int i = 0; i < profiles.Length; i++)
        {
            profile = profiles[i];
            if (profile == null) 
                continue;
            
            var userElementView = SetUserViewList(profile.userId,
                view =>
                {
                    view.SetUserProfile(profile);
                });
            newUserList.Add(userElementView);
        }
        SetVisibleList(newUserList);
    }

    public void RemoveUserView(string userId)
    {
        if (userElementViews.TryGetValue(userId, out UserElementView userElementView))
        {
            PoolUserView(userElementView);
        }
    }

    public void SetVisibleList(List<UserElementView> viewsList)
    {
        if (viewsList == null)
        {
            return;
        }
        
        foreach (UserElementView userElementView in userElementViews.Values)
        {
            userElementView.SetActive(false);
        }
        for (int i = 0; i < viewsList.Count; i++)
        {
            viewsList[i].SetActive(true);
            viewsList[i].SetOrder(i);
        }
    }

    public void SetUsersInRolList(List<string> usersId)
    {
        usersInRolList = usersId;
        foreach (KeyValuePair<string, UserElementView> keyValuePair in userElementViews)
        {
            keyValuePair.Value.SetIsAdded(usersId.Contains(keyValuePair.Key));
        }
    }

    public List<UserElementView> GetUserElementViews()
    {
        return userElementViews.Values.ToList();
    }

    private void PoolUserView(UserElementView userView)
    {
        userView.SetActive(false);
        viewPool.Enqueue(userView);
    }

    private UserElementView GetUserView()
    {
        UserElementView userView;

        if (viewPool.Count > 0)
        {
            userView = viewPool.Dequeue();
        }
        else
        {
            userView = Object.Instantiate(userElementViewBase, elementsParent);
        }
        userView.ClearThumbnail();
        
        userView.OnAddPressed -= OnAddUserPressed;
        userView.OnRemovePressed -= OnRemoveUserPressed;
        userView.OnAddPressed += OnAddUserPressed;
        userView.OnRemovePressed += OnRemoveUserPressed;

        return userView;
    }
    
    private UserElementView SetUserViewList(string userId, Action<UserElementView> OnAddNewElement)
    {
        bool isBlocked = UserProfile.GetOwnUserProfile().blocked.Contains(userId);
        bool isAddedRol = usersInRolList?.Contains(userId) ?? false;

        if (!userElementViews.TryGetValue(userId, out UserElementView userElementView))
        {
            userElementView = GetUserView();
            OnAddNewElement?.Invoke(userElementView);
            userElementView.SetAlwaysHighlighted(true);
            userElementViews.Add(userId, userElementView);
        }
        
        userElementView.SetBlocked(isBlocked);
        userElementView.SetIsAdded(isAddedRol);
        return userElementView;
    }
    
    void OnAddUserPressed(string userId)
    {
        OnAddUser?.Invoke(userId);
    }
    
    void OnRemoveUserPressed(string userId)
    {
        OnRemoveUser?.Invoke(userId);
    }
}