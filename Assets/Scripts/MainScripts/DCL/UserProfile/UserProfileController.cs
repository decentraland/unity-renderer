using System;
using UnityEngine;

public class UserProfileController : MonoBehaviour
{
    public static UserProfileController i { get; private set; }

    [NonSerialized] public UserProfile ownUserProfile;
    public void Awake()
    {
        i = this;
        ownUserProfile = UserProfile.GetOwnUserProfile();
    }

    public void LoadProfile(string payload)
    {
        ownUserProfile.UpdateData(JsonUtility.FromJson<UserProfile.Model>(payload));
    }
}
