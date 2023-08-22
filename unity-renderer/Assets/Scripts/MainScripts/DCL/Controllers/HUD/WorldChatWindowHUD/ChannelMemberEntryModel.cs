using System;

namespace DCL.Social.Chat
{
    [Serializable]
    public class ChannelMemberEntryModel : BaseComponentModel
    {
        public string userId;
        public string userName;
        public string thumnailUrl;
        public bool isOnline;
        public bool isOptionsButtonHidden;
    }
}
