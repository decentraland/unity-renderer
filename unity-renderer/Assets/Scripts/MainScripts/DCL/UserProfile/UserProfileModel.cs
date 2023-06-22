using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UserProfileModel
{
    [System.Serializable]
    public class Snapshots
    {
        public string face256;
        public string body;

        public bool Equals(Snapshots snapshots)
        {
            if (snapshots == null) return false;
            if (snapshots.face256 != face256) return false;
            if (snapshots.body != body) return false;
            return true;
        }
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

        public bool Equals(ParcelsWithAccess parcelsWithAccess)
        {
            if (parcelsWithAccess == null) return false;
            if (parcelsWithAccess.x != x) return false;
            if (parcelsWithAccess.y != y) return false;
            if (parcelsWithAccess.landRole != landRole) return false;
            return true;
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
    public Snapshots snapshots = new ();
    public UserProfileModel Clone() => (UserProfileModel)MemberwiseClone();

    public bool hasConnectedWeb3 = true;

    public int tutorialFlagsMask;
    public List<string> blocked = new ();
    public List<string> muted = new ();
    public int tutorialStep;
    public bool hasClaimedName = false;

    public static UserProfileModel FallbackModel(string name, int id)
    {
        var fallbackId = $"{name}_{id}";

        return new UserProfileModel
        {
            // Required fields (otherwise exceptions will be thrown by UserProfile.OnUpdate subscribers)
            userId = fallbackId,
            name = name,
            description = "There was a problem with loading this profile. This is a fallback profile",

            avatar = AvatarModel.FallbackModel(name, id),

            // Optional (exceptions-free) fields
            ethAddress = fallbackId,
            email = fallbackId,
            baseUrl = fallbackId,
        };
    }

    public bool Equals(UserProfileModel model)
    {
        if (model == null) return false;
        if (model.userId != userId) return false;
        if (model.ethAddress != ethAddress) return false;
        if (model.name != name) return false;
        if (model.email != email) return false;
        if (model.description != description) return false;
        if (model.baseUrl != baseUrl) return false;
        if (model.created_at != created_at) return false;
        if (model.updated_at != updated_at) return false;
        if (model.version != version) return false;
        if (!model.avatar.Equals(avatar)) return false;

        if (model.parcelsWithAccess != null)
        {
            if (!model.parcelsWithAccess.Equals(parcelsWithAccess))
                return false;
        }
        else if (parcelsWithAccess != null)
            return false;

        if (model.hasConnectedWeb3 != hasConnectedWeb3) return false;
        if (model.tutorialFlagsMask != tutorialFlagsMask) return false;
        if (model.tutorialFlagsMask != tutorialFlagsMask) return false;

        if (model.blocked != null && blocked != null)
        {
            // We're comparing only the lenght of the 'blocked' list because we found several cases where it is too big and it would cause huge hiccups.
            // Also, taking into account that this list should be removed from the profile data in the catalysts, we decided to nor compare the content.
            if (model.blocked.Count != blocked.Count)
                return false;
        }
        else if (model.blocked != null || blocked != null)
            return false;

        if (model.muted != null && muted != null)
        {
            // We're comparing only the lenght of the 'muted' list because we found several cases where it is too big and it would cause huge hiccups.
            // Also, taking into account that this list should be removed from the profile data in the catalysts, we decided to nor compare the content.
            if (model.muted.Count != muted.Count)
                return false;
        }
        else if (model.muted != null || muted != null)
            return false;

        if (model.tutorialStep != tutorialStep) return false;
        if (model.hasClaimedName != hasClaimedName) return false;

        return true;
    }

    public string ComposeCorrectUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        if (!url.StartsWith("Qm") && !url.StartsWith("ba"))
            return url;

        return baseUrl + url;
    }
}
