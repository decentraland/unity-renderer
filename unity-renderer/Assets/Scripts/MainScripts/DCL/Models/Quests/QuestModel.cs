using System;
using System.Linq;
using DCL.Helpers;

public static class QuestsLiterals
{
    public static class Status
    {
        public const string BLOCKED = "blocked";
        public const string NOT_STARTED = "not_started";
        public const string ON_GOING = "on_going";
        public const string COMPLETED = "completed";
        public const string FAILED = "failed";
    }

    public static class RewardStatus
    {
        public const string NOT_GIVEN = "not_given";
        public const string OK = "ok";
        public const string ALREADY_GIVEN = "already_given";
        public const string TASK_ALREADY_COMPLETED = "task_already_completed";
        public const string FAILED = "failed";
    }

    public static class Visibility
    {
        public const string VISIBLE = "visible";
        public const string VISIBLE_IF_CAN_START = "visible_if_can_start";
        public const string SECRET = "secret";
    }
}

[System.Serializable]
public class QuestModel : BaseModel
{
    public string id;
    public string name;
    public string description;
    public string thumbnail_entry;
    public string status;
    public string visibility;
    public string thumbnail_banner;
    public QuestSection[] sections;
    public DateTime assignmentTime = DateTime.Now; //TODO remove this once kernel send the data properly
    public DateTime completionTime = DateTime.Now; //TODO remove this once kernel send the data properly
    public QuestReward[] rewards;

    [NonSerialized]
    public float oldProgress = 0;

    public bool TryGetSection(string sectionId, out QuestSection section)
    {
        section = sections.FirstOrDefault(x => x.id == sectionId);
        return section != null;
    }

    public bool TryGetReward(string rewardId, out QuestReward reward)
    {
        reward = rewards?.FirstOrDefault(x => x.id == rewardId);
        return reward != null;
    }

    public bool canBePinned => !isCompleted && status != QuestsLiterals.Status.BLOCKED;
    public bool isCompleted => status == QuestsLiterals.Status.COMPLETED;
    public bool hasAvailableTasks => sections.Any(x => x.tasks.Any(y => y.status != QuestsLiterals.Status.BLOCKED));
    public bool justProgressed => sections.Any(x => x.tasks.Any(y => y.status != QuestsLiterals.Status.BLOCKED && y.justProgressed));
    public float progress => sections.Average(x => x.progress);

    public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<QuestModel>(json); }
}