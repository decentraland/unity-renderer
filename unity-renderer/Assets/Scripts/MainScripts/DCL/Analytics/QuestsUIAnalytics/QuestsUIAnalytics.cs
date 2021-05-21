using System.Collections.Generic;

/// <summary>
/// These events should be tracked in the quest server.
/// In the meantime we will implement them here
/// </summary>
public static class QuestsUIAnalytics
{
    private const string QUEST_PIN_CHANGED = "quest_pin_changed";
    private const string QUEST_JUMP_IN_PRESSED = "quest_jump_in_pressed";

    public enum UISource
    {
        QuestsLog,
        QuestDetails,
        QuestsTracker
    }

    public static void SendQuestPinChanged(string questId, bool isPinned, UISource uiSource)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("quest_id", questId);
        data.Add("ui_source", uiSource.ToString());
        data.Add("quest_is_pinned", isPinned.ToString());
        GenericAnalytics.SendAnalytic(QUEST_PIN_CHANGED, data);
    }

    public static void SendJumpInPressed(string questId, string taskId, string coordinates, UISource uiSource)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("quest_id", questId);
        data.Add("task_id", taskId);
        data.Add("jump_in_coordinates", coordinates);
        data.Add("quest_ui_source", uiSource.ToString());
        GenericAnalytics.SendAnalytic(QUEST_JUMP_IN_PRESSED, data);
    }
}