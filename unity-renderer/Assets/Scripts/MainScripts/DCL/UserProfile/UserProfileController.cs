using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCLServices.WearablesCatalogService;
using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class UserProfileController : MonoBehaviour
{
    private const int REQUEST_TIMEOUT = 30;

    public static UserProfileController i { get; private set; }

    public event Action OnBaseWereablesFail;

    private static UserProfileDictionary userProfilesCatalogValue;

    private readonly Dictionary<string, UniTaskCompletionSource<UserProfile>> pendingUserProfileTasks = new (StringComparer.OrdinalIgnoreCase);
    private UniTaskCompletionSource<UserProfile> saveLinkTask;
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

    public UserProfileDictionary AllProfiles => userProfilesCatalog;

    public void Awake()
    {
        i = this;
        ownUserProfile = UserProfile.GetOwnUserProfile();
    }

    [PublicAPI]
    public void LoadProfile(string payload)
    {
        async UniTaskVoid RequestBaseWearablesAsync(CancellationToken ct)
        {
            try
            {
                await DCL.Environment.i.serviceLocator.Get<IWearablesCatalogService>().RequestBaseWearablesAsync(ct);
            }
            catch (Exception e)
            {
                OnBaseWereablesFail?.Invoke();
                Debug.LogError(e.Message);
            }
        }

        if (!baseWearablesAlreadyRequested)
        {
            baseWearablesAlreadyRequested = true;
            RequestBaseWearablesAsync(CancellationToken.None).Forget();
        }

        if (payload == null)
            return;

        var model = JsonUtility.FromJson<UserProfileModelDTO>(payload).ToUserProfileModel();
        ownUserProfile.UpdateData(model);
        userProfilesCatalog.Add(model.userId, ownUserProfile);

        if (saveLinkTask != null)
        {
            saveLinkTask.TrySetResult(ownUserProfile);
            saveLinkTask = null;
        }
    }

    [PublicAPI]
    public void AddUserProfileToCatalog(string payload) { AddUserProfileToCatalog(JsonUtility.FromJson<UserProfileModelDTO>(payload).ToUserProfileModel()); }

    [PublicAPI]
    public void AddUserProfilesToCatalog(string payload)
    {
        var usersPayload = JsonUtility.FromJson<AddUserProfilesToCatalogPayload>(payload);
        var users = usersPayload.users;
        var count = users.Length;

        for (var i = 0; i < count; ++i)
            AddUserProfileToCatalog(users[i]);
    }

    [PublicAPI]
    public void RemoveUserProfilesFromCatalog(string payload)
    {
        string[] usernames = JsonUtility.FromJson<string[]>(payload);
        for (int index = 0; index < usernames.Length; index++)
        {
            RemoveUserProfileFromCatalog(userProfilesCatalog.Get(usernames[index]));
        }
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

    public static UserProfile GetProfileByUserId(string targetUserId) { return userProfilesCatalog.Get(targetUserId); }

    public void RemoveUserProfileFromCatalog(UserProfile userProfile)
    {
        if (userProfile == null)
            return;

        userProfilesCatalog.Remove(userProfile.userId);
        Destroy(userProfile);
    }

    public void ClearProfilesCatalog() { userProfilesCatalog?.Clear(); }

    public UniTask<UserProfile> RequestFullUserProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (pendingUserProfileTasks.TryGetValue(userId, out var existingTask))
            return existingTask.Task;

        var task = new UniTaskCompletionSource<UserProfile>();
        cancellationToken.RegisterWithoutCaptureExecutionContext(() => task.TrySetCanceled());
        pendingUserProfileTasks[userId] = task;

        WebInterface.SendRequestUserProfile(userId);

        return task.Task.Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT));
    }

    public UniTask<UserProfile> SaveLinks(List<UserProfileModel.Link> links, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (saveLinkTask != null)
            return saveLinkTask.Task.AttachExternalCancellation(cancellationToken);

        saveLinkTask = new UniTaskCompletionSource<UserProfile>();

        WebInterface.SaveProfileLinks(new WebInterface.SaveLinksPayload
        {
            links = links.Select(link => new WebInterface.SaveLinksPayload.Link
            {
                title = link.title,
                url = link.url,
            }).ToList(),
        });

        return saveLinkTask.Task
                           .Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT))
                           .AttachExternalCancellation(cancellationToken);
    }
}
