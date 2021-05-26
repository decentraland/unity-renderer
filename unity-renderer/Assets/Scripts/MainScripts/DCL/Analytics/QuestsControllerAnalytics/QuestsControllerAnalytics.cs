using System.Collections.Generic;

/// <summary>
/// These events should be tracked in the quest server.
/// In the meantime we will implement them here
/// </summary>
public static class QuestsControllerAnalytics
{
    private const string QUEST_DISCOVERED = "quest_discovered";
    private const string QUEST_COMPLETED = "quest_completed";
    private const string TASK_COMPLETED = "task_completed";
    private const string TASK_PROGRESSED = "task_progressed";
    private const string REWARD_OBTAINED = "reward_obtained";

    public static void SendQuestDiscovered(QuestModel quest)
    {
        var data = new Dictionary<string, string>();
        FillQuestData(data, quest);
        GenericAnalytics.SendAnalytic(QUEST_DISCOVERED, data);
    }

    public static void SendQuestCompleted(QuestModel quest)
    {
        var data = new Dictionary<string, string>();
        FillQuestData(data, quest);
        GenericAnalytics.SendAnalytic(QUEST_COMPLETED, data);
    }

    public static void SendTaskCompleted(QuestModel quest, QuestSection section, QuestTask task)
    {
        var data = new Dictionary<string, string>();
        FillQuestData(data, quest);
        FillSectionData(data, section);
        FillTaskData(data, task);
        GenericAnalytics.SendAnalytic(TASK_COMPLETED, data);
    }

    public static void SendTaskProgressed(QuestModel quest, QuestSection section, QuestTask task)
    {
        var data = new Dictionary<string, string>();
        FillQuestData(data, quest);
        FillSectionData(data, section);
        FillTaskData(data, task);
        GenericAnalytics.SendAnalytic(TASK_PROGRESSED, data);
    }

    public static void SendRewardObtained(QuestModel quest, QuestReward reward)
    {
        var data = new Dictionary<string, string>();
        FillQuestData(data, quest);
        FillRewardData(data, reward);
        GenericAnalytics.SendAnalytic(REWARD_OBTAINED, data);
    }

    private static void FillQuestData(Dictionary<string, string> data, QuestModel quest)
    {
        if (quest == null)
            return;

        data.Add("quest_id", quest.id);
        data.Add("quest_name", quest.name);
        data.Add("quest_progress", quest.progress.ToString());
        data.Add("quest_status", quest.status);
    }

    private static void FillSectionData(Dictionary<string, string> data, QuestSection section)
    {
        if (section == null)
            return;

        data.Add("section_id", section.id);
        data.Add("section_name", section.name);
        data.Add("section_progress", section.progress.ToString());
    }

    private static void FillTaskData(Dictionary<string, string> data, QuestTask task)
    {
        if (task == null)
            return;

        data.Add("task_id", task.id);
        data.Add("task_name", task.name);
        data.Add("task_progress", task.progress.ToString());
        data.Add("task_status", task.status);
    }

    private static void FillRewardData(Dictionary<string, string> data, QuestReward reward)
    {
        if (reward == null)
            return;

        data.Add("reward_id", reward.id);
        data.Add("reward_name", reward.name);
        data.Add("reward_type", reward.type);
    }
}