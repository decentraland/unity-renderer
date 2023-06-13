using System;

namespace DCL.MyAccount
{
    [Serializable]
    public record MyProfileSectionModel
    {
        public bool IsClaimedMode;
        public string CurrentName;
        public bool ShowClaimBanner;
        public bool ShowInputForClaimedMode;
    }
}
