namespace DCL.Huds
{
    [System.Serializable]
    public class QuestModel
    {
        public string id;
        public string name;
        public string description;
        public string[] requirements;
        public bool active;
        public string visibility;
        public QuestTask[] tasks;
        public string progressStatus;
    }

    [System.Serializable]
    public class QuestTask
    {
        public string id;
        public string description;
        public string coordinates;
        public bool required;
        public string[] requirements;
        public QuestStep[] steps;
    }

    [System.Serializable]
    public class QuestStep
    {
        public string id;
        public string type;
        public string payload;
    }
}
