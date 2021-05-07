public class SceneDataUpdatePayload
{
    public string name;
    public string description;
    public string[] requiredPermissions;
    public bool isMatureContent;
    public bool allowVoiceChat;
}

public class SceneContributorsUpdatePayload
{
    public string[] contributors;
}

public class SceneAdminsUpdatePayload
{
    public string[] admins;
}

public class SceneBannedUsersUpdatePayload
{
    public string[] bannedUsers;
}