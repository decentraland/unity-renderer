namespace SocialFeaturesAnalytics
{
    public enum PlayerType
    {
        Wallet,
        Guest
    }

    public enum ChatContentType
    {
        Text,
        Coords,
        Link
    }

    public enum ChatMessageType
    {
        None,
        Public,
        Private,
        System
    }

    public enum FriendActionSource
    {
        Passport,
        ProfileContextMenu,
        FriendsHUD,
        Conversation
    }

    public enum PlayerReportIssueType
    {
        None
    }

    public enum EmoteSource
    {
        EmotesWheel,
        Shortcut,
        Command
    }
}