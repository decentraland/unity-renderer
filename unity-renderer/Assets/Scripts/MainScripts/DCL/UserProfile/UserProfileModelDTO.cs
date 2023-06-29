using DCL.UserProfiles;
using System;
using System.Collections.Generic;

[Serializable]
public class UserProfileModelDTO
{
    public string userId;
    public string ethAddress;
    public string name;
    public string email;
    public string description;
    public string baseUrl;
    public UserProfileModel.ParcelsWithAccess[] parcelsWithAccess;
    public ulong created_at;
    public ulong updated_at;
    public int version;
    public AvatarModelDTO avatar;
    public UserProfileModel.Snapshots snapshots = new ();

    public bool hasConnectedWeb3 = true;

    public int tutorialFlagsMask;
    public List<string> blocked;
    public List<string> muted;
    public int tutorialStep;
    public bool hasClaimedName = false;
    public List<UserProfileModel.Link> links;

    public string country;
    public string employmentStatus;
    public string gender;
    public string pronouns;
    public string relationshipStatus;
    public string sexualOrientation;
    public string language;
    public string profession;
    public long birthdate;
    public string realName;
    public string hobbies;

    public UserProfileModel ToUserProfileModel()
    {
        UserProfileModel userProfileModel = new UserProfileModel
            {
                userId = this.userId,
                ethAddress = this.ethAddress,
                name = this.name,
                email = this.email,
                description = this.description,
                baseUrl = this.baseUrl,
                parcelsWithAccess = this.parcelsWithAccess,
                created_at = this.created_at,
                updated_at = this.updated_at,
                version = this.version,
                avatar = this.avatar.ToAvatarModel(),
                snapshots = this.snapshots,
                hasConnectedWeb3 = this.hasConnectedWeb3,
                tutorialFlagsMask = this.tutorialFlagsMask,
                blocked = this.blocked,
                muted = this.muted,
                tutorialStep = this.tutorialStep,
                hasClaimedName = this.hasClaimedName,
                links = this.links,
                AdditionalInfo = new AdditionalInfo
                {
                    Country = this.country,
                    EmploymentStatus = this.employmentStatus,
                    Gender = this.gender,
                    Pronouns = this.pronouns,
                    RelationshipStatus = this.relationshipStatus,
                    SexualOrientation = this.sexualOrientation,
                    Language = this.language,
                    Profession = this.profession,
                    BirthDate = birthdate == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(this.birthdate).DateTime,
                    RealName = this.realName,
                    Hobbies = this.hobbies,
                },
            };

        return userProfileModel;
    }

}
