using System;

namespace DCL.MyAccount
{
    [Serializable]
    public class BlockedUserEntryModel : BaseComponentModel
    {
        public string userId;
        public string userName;
        public string thumbnailUrl;
    }
}
