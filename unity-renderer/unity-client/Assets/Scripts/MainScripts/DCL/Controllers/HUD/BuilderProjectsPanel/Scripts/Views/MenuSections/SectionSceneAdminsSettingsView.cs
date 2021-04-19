using System;
using System.Collections.Generic;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SectionSceneAdminsSettingsView : MonoBehaviour, IDisposable
{
    [SerializeField] internal UsersSearchPromptView adminsSearchPromptView;
    [SerializeField] internal UsersSearchPromptView blockedSearchPromptView;
    [SerializeField] internal Button addAdminButton;
    [SerializeField] internal Button addBlockedButton;
    [SerializeField] internal UserElementView adminElementView;
    [SerializeField] internal UserElementView blockedElementView;
    [SerializeField] internal Transform adminsContainer;
    [SerializeField] internal Transform blockedContainer;
    [SerializeField] internal GameObject adminsEmptyListContainer;
    [SerializeField] internal GameObject blockedEmptyListContainer;
    [SerializeField] internal TextMeshProUGUI labelAdmins;
    [SerializeField] internal TextMeshProUGUI labelBlocked;

    public event Action OnSearchFriendButtonPressed;
    public event Action OnSearchUserButtonPressed;

    internal readonly Dictionary<string, UserElementView> adminsElementViews = new Dictionary<string, UserElementView>();
    internal readonly Dictionary<string, UserElementView> bannedUsersElementViews = new Dictionary<string, UserElementView>();
    private readonly Queue<UserElementView> userElementViewsPool = new Queue<UserElementView>();

    private string adminsLabelFormat;
    private string blockedLabelFormat;
    private bool isDestroyed;

    private void Awake()
    {
        addAdminButton.onClick.AddListener(()=> OnSearchFriendButtonPressed?.Invoke());
        addBlockedButton.onClick.AddListener(()=> OnSearchUserButtonPressed?.Invoke());
        PoolView(adminElementView);
        PoolView(blockedElementView);
        adminsLabelFormat = labelAdmins.text;
        blockedLabelFormat = labelBlocked.text;
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public UsersSearchPromptView GetAdminsSearchPromptView()
    {
        return adminsSearchPromptView;
    }
    
    public UsersSearchPromptView GetBlockedSearchPromptView()
    {
        return blockedSearchPromptView;
    }
    
    public void SetAdminsEmptyList(bool isEmpty)
    {
        adminsContainer.gameObject.SetActive(!isEmpty);
        adminsEmptyListContainer.SetActive(isEmpty);
        
        if (isEmpty)
        {
            ClearElementViewsDictionary(adminsElementViews);
        }        
    }
    
    public void SetBannedUsersEmptyList(bool isEmpty)
    {
        blockedContainer.gameObject.SetActive(!isEmpty);
        blockedEmptyListContainer.SetActive(isEmpty);
        
        if (isEmpty)
        {
            ClearElementViewsDictionary(bannedUsersElementViews);
        }   
    }

    public void SetAdminsCount(int count)
    {
        labelAdmins.text = string.Format(adminsLabelFormat, count);
    }
    
    public void SetBannedUsersCount(int count)
    {
        labelBlocked.text = string.Format(blockedLabelFormat, count);
    }

    public UserElementView AddAdmin(string userId)
    {
        return AddUser(userId, true);
    }
    
    public UserElementView AddBannedUser(string userId)
    {
        return AddUser(userId, false);
    }

    public bool RemoveAdmin(string userId)
    {
        return RemoveUser(userId, true);
    }
    
    public bool RemoveBannedUser(string userId)
    {
        return RemoveUser(userId, false);
    }

    UserElementView AddUser(string userId, bool addAsAdmin)
    {
        var dictionary = addAsAdmin ? adminsElementViews : bannedUsersElementViews;
        if (!dictionary.TryGetValue(userId, out UserElementView view))
        {
            view = GetView();
            view.SetUserName(userId);
            view.SetUserId(userId);
            view.SetAlwaysHighlighted(false);
            view.SetIsAdded(true);
            view.SetActive(true);
            dictionary.Add(userId, view);
        }
        
        bool isBlocked = UserProfile.GetOwnUserProfile().blocked.Contains(userId);
        view.SetBlocked(isBlocked);
        view.SetParent(addAsAdmin? adminsContainer : blockedContainer);
        return view;
    }

    bool RemoveUser(string userId, bool removeAsAdmin)
    {
        var dictionary = removeAsAdmin ? adminsElementViews : bannedUsersElementViews;
        if (!dictionary.TryGetValue(userId, out UserElementView view))
        {
            return false;
        }
        
        Transform container = removeAsAdmin ? adminsContainer : blockedContainer;

        if (view.GetParent() != container)
        {
            return false;
        }

        dictionary.Remove(userId);
        PoolView(view);
        return true;
    }

    void PoolView(UserElementView view)
    {
        view.SetActive(false);
        userElementViewsPool.Enqueue(view);
    }

    UserElementView GetView()
    {
        UserElementView userView;

        if (userElementViewsPool.Count > 0)
        {
            userView = userElementViewsPool.Dequeue();
        }
        else
        {
            userView = Instantiate(adminElementView, adminsContainer);
        }
        userView.ClearThumbnail();
        return userView;
    }

    void ClearElementViewsDictionary(Dictionary<string, UserElementView> dictionary)
    {
        foreach (UserElementView view in dictionary.Values)
        {
            PoolView(view);
        }
        dictionary.Clear();
    }
}
