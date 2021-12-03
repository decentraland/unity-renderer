namespace DCL
{
    public class DataStore_Quests
    {
        public readonly BaseDictionary<string, QuestModel> quests = new BaseDictionary<string, QuestModel>();
        public readonly BaseCollection<string> pinnedQuests = new BaseCollection<string>();
    }
}