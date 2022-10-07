using System;

[Serializable]
public enum AllowChannelsCreationMode
{
    ALLOWLIST = 0,
    NAMES = 1,
    WALLET = 2
}

[Serializable]
public class UsersAllowedToCreateChannelsVariantPayload
{
    public AllowChannelsCreationMode mode;
    public string[] allowList;
}
