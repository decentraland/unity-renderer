using System.Collections.Generic;

[System.Serializable]
public class UserProfileModel
{
    [System.Serializable]
    public class Snapshots
    {
        public string face;
        public string body;
    }

    public string userId;
    public string name;
    public string email;
    public string description;
    public ulong created_at;
    public ulong updated_at;
    public string version;
    public AvatarModel avatar;
    public string[] inventory;
    public Snapshots snapshots = new Snapshots();
    public UserProfileModel Clone() => (UserProfileModel)MemberwiseClone();

    public bool hasConnectedWeb3 = true;

    public int tutorialFlagsMask;
    public List<string> blocked;
    public int tutorialStep;
    public bool hasClaimedName = false;
}
