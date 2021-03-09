using System.Linq;
using DCL.Helpers;

public static class QuestLiterals
{
    public static class Status
    {
        public static string BLOCKED = "blocked";
        public static string NOT_STARTED = "not_started";
        public static string ON_GOING = "on_going";
        public static string COMPLETED = "completed";
        public static string FAILED = "failed";
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
    public string thumbnail_banner;
    public string icon;
    public QuestSection[] sections;

    public bool TryGetSection(string sectionId, out QuestSection section)
    {
        section = sections.FirstOrDefault(x => x.id == sectionId);
        return section != null;
    }

    public bool canBePinned => !isCompleted && status != QuestLiterals.Status.BLOCKED;
    public bool isCompleted => status == QuestLiterals.Status.COMPLETED;
    public float progress => sections.Average(x => x.progress);

    public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<QuestModel>(json); }
}
