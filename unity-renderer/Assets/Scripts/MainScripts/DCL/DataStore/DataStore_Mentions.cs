namespace DCL
{
    public class DataStore_Mentions
    {
        public readonly BaseVariable<string> ownPlayerMentionedInDM = new ();
        public readonly BaseVariable<string> ownPlayerMentionedInChannel = new ();
        public readonly BaseVariable<bool> isMentionSuggestionVisible = new ();
        public readonly BaseVariable<string> someoneMentionedFromContextMenu = new ();
    }
}
