namespace DCL
{
    public class DataStore_Quests
    {
        public readonly BaseVariable<bool> isInitialized = new BaseVariable<bool>(false);
        public readonly BaseDictionary<string, QuestModel> quests = new BaseDictionary<string, QuestModel>();
        public readonly BaseCollection<string> pinnedQuests = new BaseCollection<string>();
    }
}