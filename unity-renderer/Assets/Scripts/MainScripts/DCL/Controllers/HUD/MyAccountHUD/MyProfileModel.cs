using System;

namespace DCL.MyAccount
{
    [Serializable]
    public record MyProfileModel
    {
        public bool IsClaimedMode;
        public string CurrentName;
        public bool ShowClaimBanner;
        public bool ShowInputForClaimedMode;
    }
}
