using System;
using System.Collections.Generic;

namespace DCL.MyAccount
{
    [Serializable]
    public record MyProfileModel
    {
        public bool IsClaimedMode;
        public string CurrentName;
        public bool ShowClaimBanner;
        public bool ShowInputForClaimedMode;
        public List<string> loadedClaimedNames = new ();
    }
}
