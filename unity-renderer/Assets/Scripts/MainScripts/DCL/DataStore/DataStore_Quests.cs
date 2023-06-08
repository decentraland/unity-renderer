namespace DCL
{
    public class DataStore_Quests
    {
        public readonly BaseVariable<bool> isInitialized = new BaseVariable<bool>(false);
        public readonly BaseDictionary<string, QuestModel> quests = new BaseDictionary<string, QuestModel>();
        public readonly BaseVariable<string> pinnedQuest = new BaseVariable<string>("");
    }
}
