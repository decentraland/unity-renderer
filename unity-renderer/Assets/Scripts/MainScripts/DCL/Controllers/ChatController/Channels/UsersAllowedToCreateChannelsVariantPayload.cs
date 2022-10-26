using System;

namespace DCL.Chat.Channels
{
    [Serializable]
    public enum AllowChannelsCreationMode
    {
        ALLOWLIST,
        NAMES,
        WALLET
    }

    [Serializable]
    public class UsersAllowedToCreateChannelsVariantPayload
    {
        public AllowChannelsCreationMode mode;
        public string[] allowList;
    }
}