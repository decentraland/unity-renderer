using System;

namespace DCL.Chat.HUD
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