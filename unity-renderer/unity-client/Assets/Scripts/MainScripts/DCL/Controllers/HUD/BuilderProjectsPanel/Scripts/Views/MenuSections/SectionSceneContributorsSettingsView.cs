using System;
using System.Collections.Generic;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SectionSceneContributorsSettingsView : MonoBehaviour, IDisposable
{
    [SerializeField] internal UsersSearchPromptView usersSearchPromptView;
    [SerializeField] internal Button addUserButton;
    [SerializeField] internal UserElementView userElementView;
    [SerializeField] internal Transform usersContainer;
    [SerializeField] internal GameObject emptyListContainer;
    [SerializeField] internal TextMeshProUGUI labelContributor;

    public event Action OnSearchUserButtonPressed;

    internal readonly Dictionary<string, UserElementView> userElementViews = new Dictionary<string, UserElementView>();
    private readonly Queue<UserElementView> userElementViewsPool = new Queue<UserElementView>();

    private string contributorLabelFormat;
    private bool isDestroyed = false;

    private void Awake()
    {
        addUserButton.onClick.AddListener(()=> OnSearchUserButtonPressed?.Invoke());
        PoolView(userElementView);
        contributorLabelFormat = labelContributor.text;
    }

    private void OnDestroy()
    {
        isDestroyed  = true;
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

    public UsersSearchPromptView GetSearchPromptView()
    {
        return usersSearchPromptView;
    }
    
    public void SetEmptyList(bool isEmpty)
    {
        usersContainer.gameObject.SetActive(!isEmpty);
        emptyListContainer.SetActive(isEmpty);

        if (isEmpty)
        {
            foreach (UserElementView view in userElementViews.Values)
            {
                PoolView(view);
            }
            userElementViews.Clear();
        }
    }

    public void SetContributorsCount(int count)
    {
        labelContributor.text = string.Format(contributorLabelFormat, count);
    }

    public UserElementView AddUser(string userId)
    {
        if (!userElementViews.TryGetValue(userId, out UserElementView view))
        {
            view = GetView();
            view.SetUserName(userId);
            view.SetUserId(userId);
            view.SetAlwaysHighlighted(false);
            view.SetIsAdded(true);
            view.SetActive(true);
            userElementViews.Add(userId, view);
        }
        
        bool isBlocked = UserProfile.GetOwnUserProfile().blocked.Contains(userId);
        view.SetBlocked(isBlocked);
        return view;
    }

    public bool RemoveUser(string userId)
    {
        if (!userElementViews.TryGetValue(userId, out UserElementView view))
        {
            return false;
        }
        userElementViews.Remove(userId);
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
            userView = Instantiate(userElementView, usersContainer);
        }
        userView.ClearThumbnail();
        return userView;
    }
}
