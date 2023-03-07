namespace DCL
{
    public class DataStore_Mentions
    {
        public readonly BaseVariable<string> ownPlayerMentionedInDM = new BaseVariable<string>();
        public readonly BaseVariable<string> ownPlayerMentionedInChannel = new BaseVariable<string>();
    }
}
