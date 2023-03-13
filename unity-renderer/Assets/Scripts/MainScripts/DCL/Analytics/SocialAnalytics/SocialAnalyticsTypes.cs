namespace SocialFeaturesAnalytics
{
    public enum PlayerType
    {
        Wallet,
        Guest
    }

    public enum PlayerActionSource
    {
        Passport,
        ProfileContextMenu,
        FriendsHUD,
        Conversation,
        ProfileEditHUD
    }

    public enum AvatarOpenSource
    {
        FriendsHUD,
        Mention,
        World,
        FriendRequest,
        ProfileHUD
    }

    public enum ItemType
    {
        Wearable,
        Emote,
        Land,
        Name
    }

    public enum PlayerReportIssueType
    {
        None
    }

    public enum VoiceMessageSource
    {
        Shortcut,
        Button
    }

    public enum MentionCreationSource
    {
        SuggestionList,
        ProfileContextMenu,
    }
}
