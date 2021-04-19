using System;
using DCL.Helpers;
using DCL.Interface;

/// <summary>
/// Query profiles by name or address
/// ENS query (by name) use a partial match checking if any ENS contains the input string.
/// </summary>
public class UsersSearcher : IDisposable
{
    private readonly IUsersSearchBridge bridge;
    
    private Promise<UserProfileModel[]> searchPrommise;
    private string currentSearchInput;

    /// <summary>
    /// Query profiles of users who their owned names contains string "name"
    /// If "name" is an address it will return the profile for that address
    /// </summary>
    /// <param name="name">name or address</param>
    /// <param name="maxResults">max results for the query</param>
    /// <returns>Profiles or null (if no profile found) promise</returns>
    public Promise<UserProfileModel[]> SearchUser(string name, int maxResults)
    {
        searchPrommise?.Dispose();
        searchPrommise = new Promise<UserProfileModel[]>();
        currentSearchInput = name;
        WebInterface.SearchENSOwner(name, maxResults);
        return searchPrommise;
    }

    public UsersSearcher() : this(UsersSearchBridge.i) { }

    public UsersSearcher(IUsersSearchBridge bridge)
    {
        this.bridge = bridge;
        if (bridge != null)
            bridge.OnSearchResult += OnSearchResult;
    }

    public void Dispose()
    {
        if (bridge != null)
            bridge.OnSearchResult -= OnSearchResult;
        searchPrommise?.Dispose();
    }

    private void OnSearchResult(string searchInput, UserProfileModel[] profiles)
    {
        if (searchInput == currentSearchInput)
        {
            searchPrommise?.Resolve(profiles);
        }
    }
}