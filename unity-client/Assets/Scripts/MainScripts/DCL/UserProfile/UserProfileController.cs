using System;
using UnityEngine;

public class UserProfileController : MonoBehaviour
{
    public static UserProfileController i { get; private set; }

    private static UserProfileDictionary userProfilesCatalogValue;
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
        if (payload == null)
        {
            return;
        }

        ownUserProfile.UpdateData(JsonUtility.FromJson<UserProfileModel>(payload));
    }

    public void AddUserProfileToCatalog(string payload)
    {
        AddUserProfileToCatalog(JsonUtility.FromJson<UserProfileModel>(payload));
    }

    public void AddUserProfilesToCatalog(string payload)
    {
        UserProfileModel[] items = JsonUtility.FromJson<UserProfileModel[]>(payload);
        int count = items.Length;
        for (int i = 0; i < count; ++i)
        {
            AddUserProfileToCatalog(items[i]);
        }
    }

    public void AddUserProfileToCatalog(UserProfileModel model)
    {
        var userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(model);
        userProfilesCatalog.Add(model.name, userProfile);
    }

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
        if (userProfile == null) return;

        userProfilesCatalog.Remove(userProfile.userName);
        Destroy(userProfile);
    }

    public void ClearWearableCatalog()
    {
        userProfilesCatalog?.Clear();
    }
}
