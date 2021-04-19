using System;
using DCL.Helpers;
using UnityEngine;

public delegate void OnSearchResultDelegate (string searchInput, UserProfileModel[] profiles);

public interface IUsersSearchBridge
{
    event OnSearchResultDelegate OnSearchResult;
}

public class UsersSearchBridge : MonoBehaviour, IUsersSearchBridge
{
    public event OnSearchResultDelegate OnSearchResult;

    public static UsersSearchBridge i { get; private set; }

    void Awake()
    {
        if (i != null)
        {
            Utils.SafeDestroy(this);
            return;
        }

        i = this;
    }

    /// <summary>
    /// Called by kernel when a query for ENS owners has finished
    /// </summary>
    /// <param name="payload">query result sent by kernel</param>
    public void SetENSOwnerQueryResult(string payload)
    {
        ResultPayload result = Utils.SafeFromJson<ResultPayload>(payload);
        
        UserProfileModel[] profiles = null;
        if (result.success && result.profiles.Length > 0)
        {
            profiles = result.profiles;
        }
        OnSearchResult?.Invoke(result.searchInput, profiles);
    }

    [Serializable]
    class ResultPayload
    {
        public string searchInput;
        public bool success;
        public UserProfileModel[] profiles;
    }
}
