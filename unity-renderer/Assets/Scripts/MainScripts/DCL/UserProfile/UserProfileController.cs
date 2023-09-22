using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using DCL.UserProfiles;
using DCLServices.WearablesCatalogService;
using Decentraland.Bff;
using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Variables.RealmsInfo;

public class UserProfileController : MonoBehaviour
{
    private const int REQUEST_TIMEOUT = 30;

    public static UserProfileController i { get; private set; }

    public event Action OnBaseWereablesFail;

    private static UserProfileDictionary userProfilesCatalogValue;

    private readonly Dictionary<string, UniTaskCompletionSource<UserProfile>> pendingUserProfileTasks = new (StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, UniTaskCompletionSource<UserProfile>> saveProfileTask = new ();
    private readonly List<WebInterface.SaveLinksPayload.Link> linkList = new ();
    private bool baseWearablesAlreadyRequested;

    private readonly IUserProfileAPIClient userProfileAPIClient = new UserProfileAPIClient();
    public ProfileValidator ProfileValidator { get; private set; }

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

        // FD:: Initialize validator
        ProfileValidator = new ProfileValidator();
        // FD:: Subscribe for /about endpoint initialization
        DataStore.i.realm.playerRealmAboutConfiguration.OnChange += PlayerRealmAboutConfigurationChanged;

        // FD:: Fetch Catalyst public key
        FetchCatalystPublicKey(CancellationToken.None).Forget();
    }

    private void PlayerRealmAboutConfigurationChanged(AboutResponse.Types.AboutConfiguration current, AboutResponse.Types.AboutConfiguration previous)
    {
        var playerRealmAboutConfiguration = DataStore.i.realm.playerRealmAboutConfiguration.Get();
        string debugFieldsPlayerRealmAboutConfiguration = playerRealmAboutConfiguration.ToString();

        // Debug.Log ("FD:: playerRealmAboutConfiguration:\n" + debugFieldsPlayerRealmAboutConfiguration);
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

        foreach (var task in saveProfileTask.Values)
            task.TrySetResult(ownUserProfile);
        saveProfileTask.Clear();
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
        return SaveUserProfile(() =>
            {
                linkList.Clear();

                foreach (UserProfileModel.Link link in links)
                {
                    linkList.Add(new WebInterface.SaveLinksPayload.Link
                    {
                        title = link.title,
                        url = link.url,
                    });
                }

                WebInterface.SaveProfileLinks(new WebInterface.SaveLinksPayload
                {
                    links = linkList,
                });
            },
            "links", cancellationToken);
    }

    public UniTask<UserProfile> SaveVerifiedName(string name, CancellationToken cancellationToken)
    {
        return SaveUserProfile(() => WebInterface.SendSaveUserVerifiedName(name),
            "verified_name", cancellationToken);
    }

    public UniTask<UserProfile> SaveUnverifiedName(string name, CancellationToken cancellationToken)
    {
        return SaveUserProfile(() => WebInterface.SendSaveUserUnverifiedName(name),
            "unverified_name", cancellationToken);
    }

    public UniTask<UserProfile> SaveDescription(string description, CancellationToken cancellationToken)
    {
        return SaveUserProfile(() => WebInterface.SendSaveUserDescription(description),
            "description", cancellationToken);
    }

    public UniTask<UserProfile> SaveAdditionalInfo(AdditionalInfo additionalInfo, CancellationToken cancellationToken)
    {
        return SaveUserProfile(() => WebInterface.SaveAdditionalInfo(new WebInterface.SaveAdditionalInfoPayload
            {
                country = additionalInfo.Country,
                gender = additionalInfo.Gender,
                pronouns = additionalInfo.Pronouns,
                relationshipStatus = additionalInfo.RelationshipStatus,
                sexualOrientation = additionalInfo.SexualOrientation,
                language = additionalInfo.Language,
                profession = additionalInfo.Profession,
                birthdate = additionalInfo.BirthDate != null ? new DateTimeOffset(additionalInfo.BirthDate.Value).ToUnixTimeSeconds() : 0,
                realName = additionalInfo.RealName,
                hobbies = additionalInfo.Hobbies,
                employmentStatus = additionalInfo.EmploymentStatus,
            }),
            "additional_info", cancellationToken);
    }

    private UniTask<UserProfile> SaveUserProfile(Action webInterfaceCall, string operationType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (saveProfileTask.ContainsKey(operationType))
            return saveProfileTask[operationType].Task.AttachExternalCancellation(cancellationToken);

        var task = new UniTaskCompletionSource<UserProfile>();
        saveProfileTask[operationType] = task;

        webInterfaceCall.Invoke();

        return task.Task
                   .Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT))
                   .AttachExternalCancellation(cancellationToken);
    }

    // =================================================================
    // FD:: ========== New test stuff for profile validation ===========

    private async UniTask FetchCatalystPublicKey(CancellationToken ct)
    {
        try
        {
            string publicKey = await userProfileAPIClient.FetchCatalystPublicKey(ct);
            ProfileValidator.CatalystPublicKey = publicKey;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch Catalyst public key: {e.Message}");
        }
    }


    private async UniTask<UserProfileModel> FetchAndUpdateUserProfile(string ethAddress, CancellationToken ct)
    {
        try
        {
            UserProfileModel userProfileModel = await userProfileAPIClient.FetchUserProfile(ethAddress, ct);

            // Update the profile
            ownUserProfile.UpdateData(userProfileModel);
            return userProfileModel;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update user profile: {e.Message}");
            return null;
        }
    }

    private string CatalystUrlFromAboutEndpoint
    {
        get
        {
            var playerRealmAboutConfiguration = DataStore.i.realm.playerRealmAboutConfiguration.Get();
            string catalystUrl = playerRealmAboutConfiguration.ToString(); // FD:: catalystUrl field not present yet in this /about implementation
            return catalystUrl;
        }
    }

    public async UniTask<bool> ValidateUserProfile(string ethAddress, CancellationToken ct)
    {
        UserProfileModel model = await FetchAndUpdateUserProfile(ethAddress, ct);

        if (model == null)
        {
            return false;
        }

        string checksum = model.checksum;
        string signedChecksum = model.checksum; // FD:: signedChecksum does not exists yet

        return ProfileValidator.ValidateUserProfile(model, checksum, signedChecksum, CatalystUrlFromAboutEndpoint);
    }

}
