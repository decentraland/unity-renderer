using System.Linq;

[System.Serializable]
public class QuestSection
{
    public string id;
    public string name;
    public float progress;
    public QuestTask[] tasks;

    public bool TryGetTask(string taskId, out QuestTask task)
    {
        task = tasks.FirstOrDefault(x => x.id == taskId);
        return task != null;
    }
}