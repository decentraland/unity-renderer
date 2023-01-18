using Cysharp.Threading.Tasks;
using System;
using DCL;
using System.Collections.Generic;
using UnityEngine;

public class UserProfileController : MonoBehaviour
{
    public static UserProfileController i { get; private set; }

    public event Action OnBaseWereablesFail;

    private static UserProfileDictionary userProfilesCatalogValue;

    private readonly Dictionary<string, UniTaskCompletionSource<UserProfile>> pendingUserProfileTasks = new ();
    private bool baseWearablesAlreadyRequested = false;

    public static UserProfileDictionary userProfilesCatalog
    {
        get
        {
            if (userProfilesCatalogValue == null)
            {
                userProfilesCatalogValue = Resources.Load<UserProfileDictionary>("UserProfilesCatalog");
            }

            return userProfilesCatalogValue;
        }
    }

    [NonSerialized] public UserProfile ownUserProfile;

    public void Awake()
    {
        i = this;
        ownUserProfile = UserProfile.GetOwnUserProfile();
    }

    public void LoadProfile(string payload)
    {
        if (!baseWearablesAlreadyRequested)
        {
            baseWearablesAlreadyRequested = true;
            CatalogController.RequestBaseWearables()
                             .Catch((error) =>
                             {
                                 OnBaseWereablesFail?.Invoke();
                                 Debug.LogError(error);
                             });
        }

        if (payload == null)
            return;

        var model = JsonUtility.FromJson<UserProfileModel>(payload);

        ownUserProfile.UpdateData(model);
        userProfilesCatalog.Add(model.userId, ownUserProfile);
    }

    public void AddUserProfileToCatalog(string payload) { AddUserProfileToCatalog(JsonUtility.FromJson<UserProfileModel>(payload)); }

    public void AddUserProfilesToCatalog(string payload)
    {
        var usersPayload = JsonUtility.FromJson<AddUserProfilesToCatalogPayload>(payload);
        var users = usersPayload.users;
        var count = users.Length;

        for (var i = 0; i < count; ++i)
            AddUserProfileToCatalog(users[i]);
    }

    public void AddUserProfileToCatalog(UserProfileModel model)
    {
        // TODO: the renderer should not alter the userId nor ethAddress, this is just a patch derived from a kernel issue
        model.userId = model.userId.ToLower();
        model.ethAddress = model.ethAddress?.ToLower();

        if (!userProfilesCatalog.TryGetValue(model.userId, out UserProfile userProfile))
            userProfile = ScriptableObject.CreateInstance<UserProfile>();

        userProfile.UpdateData(model);
        userProfilesCatalog.Add(model.userId, userProfile);

        if (pendingUserProfileTasks.TryGetValue(userProfile.userId, out var existingTask))
        {
            existingTask.TrySetResult(userProfile);
            pendingUserProfileTasks.Remove(userProfile.userId);
        }
    }

    public static UserProfile GetProfileByName(string targetUserName)
    {
        foreach (var userProfile in userProfilesCatalogValue)
        {
            if (userProfile.Value.userName.ToLower() == targetUserName.ToLower())
                return userProfile.Value;
        }

        return null;
    }

    public static UserProfile GetProfileByUserId(string targetUserId) { return userProfilesCatalog.Get(targetUserId); }

    public void RemoveUserProfilesFromCatalog(string payload)
    {
        string[] usernames = JsonUtility.FromJson<string[]>(payload);
        for (int index = 0; index < usernames.Length; index++)
        {
            RemoveUserProfileFromCatalog(userProfilesCatalog.Get(usernames[index]));
        }
    }

    public void RemoveUserProfileFromCatalog(UserProfile userProfile)
    {
        if (userProfile == null)
            return;

        userProfilesCatalog.Remove(userProfile.userId);
        Destroy(userProfile);
    }

    public void ClearProfilesCatalog() { userProfilesCatalog?.Clear(); }

    public UniTask<UserProfile> RequestFullUserProfileAsync(string userId)
    {
        // TODO: the renderer should not alter the userId nor ethAddress
        // There is a problem in case management of addresses that we cannot handle from renderer only
        userId = userId?.ToLower();

        if (pendingUserProfileTasks.TryGetValue(userId, out var existingTask))
            existingTask.TrySetCanceled();

        var task = new UniTaskCompletionSource<UserProfile>();
        pendingUserProfileTasks[userId] = task;

        return task.Task;
    }
}
