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
}