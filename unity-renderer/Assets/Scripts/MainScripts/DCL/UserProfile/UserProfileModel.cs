using System.Collections.Generic;

[System.Serializable]
public class UserProfileModel
{
    [System.Serializable]
    public class Snapshots
    {
        public string face256;
        public string body;
    }

    [System.Serializable]
    public class ParcelsWithAccess
    {
        public int x;
        public int y;
        public LandRole landRole;

        [System.Serializable]
        public enum LandRole
        {
            OWNER = 0,
            OPERATOR = 1
        }
    }

    public string userId;
    public string ethAddress;
    public string name;
    public string email;
    public string description;
    public string baseUrl;
    public ParcelsWithAccess[] parcelsWithAccess;
    public ulong created_at;
    public ulong updated_at;
    public int version;
    public AvatarModel avatar;
    public Snapshots snapshots = new Snapshots();
    public UserProfileModel Clone() => (UserProfileModel)MemberwiseClone();

    public bool hasConnectedWeb3 = true;

    public int tutorialFlagsMask;
    public List<string> blocked;
    public List<string> muted;
    public int tutorialStep;
    public bool hasClaimedName = false;


    public string ComposeCorrectUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        if (!url.StartsWith("Qm") && !url.StartsWith("ba"))
            return url;

        return baseUrl + url;
    }
}