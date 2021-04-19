using System;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Interface;

public class UserProfileFetcher : IDisposable
{
    private readonly Dictionary<string, List<Promise<UserProfile>>> pendingPromises = new Dictionary<string, List<Promise<UserProfile>>>();

    /// <summary>
    /// Look for profile in userProfilesCatalog or request kernel for a profile if not available
    /// </summary>
    /// <param name="userId">id of profile to fetch</param>
    /// <returns>a promise of the user profile</returns>
    public Promise<UserProfile> FetchProfile(string userId)
    {
        Promise<UserProfile> promise = new Promise<UserProfile>();
        if (UserProfileController.userProfilesCatalog.TryGetValue(userId, out UserProfile profile))
        {
            promise.Resolve(profile);
            return promise;
        }

        if (!pendingPromises.TryGetValue(userId, out List<Promise<UserProfile>> promisesForUserId))
        {
            promisesForUserId = new List<Promise<UserProfile>>();
            pendingPromises.Add(userId, promisesForUserId);
            WebInterface.RequestUserProfile(userId);
        }

        promisesForUserId.Add(promise);
        return promise;
    }

    public UserProfileFetcher()
    {
        UserProfileController.userProfilesCatalog.OnAdded += OnProfileAddedToCatalog;
    }

    public void Dispose()
    {
        UserProfileController.userProfilesCatalog.OnAdded -= OnProfileAddedToCatalog;
    }

    private void OnProfileAddedToCatalog(string userId, UserProfile profile)
    {
        if (!pendingPromises.TryGetValue(userId, out List<Promise<UserProfile>> promisesForUserId))
        {
            return;
        }

        for (int i = 0; i < promisesForUserId.Count; i++)
        {
            if (promisesForUserId[i] == null)
                continue;

            promisesForUserId[i].Resolve(profile);
        }

        pendingPromises.Remove(userId);
    }
}