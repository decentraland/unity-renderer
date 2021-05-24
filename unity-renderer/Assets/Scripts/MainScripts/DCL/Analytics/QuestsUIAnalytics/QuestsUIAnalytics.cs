using System.Collections.Generic;

/// <summary>
/// These events should be tracked in the quest server.
/// In the meantime we will implement them here
/// </summary>
public static class QuestsUIAnalytics
{
    private const string QUEST_PIN_CHANGED = "quest_pin_changed";
    private const string QUEST_JUMP_IN_PRESSED = "quest_jump_in_pressed";
    private const string QUEST_LOG_OPENED = "quest_log_opened";
    private const string QUEST_LOG_CLOSED = "quest_log_closed";

    public enum UIContext
    {
        QuestsLog,
        QuestDetails,
        QuestsTracker,
        QuestsTaskbarButton,
    }

    public static void SendQuestPinChanged(string questId, bool isPinned, UIContext uiContext)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("quest_id", questId);
        data.Add("ui_source", uiContext.ToString());
        data.Add("quest_is_pinned", isPinned.ToString());
        GenericAnalytics.SendAnalytic(QUEST_PIN_CHANGED, data);
    }

    public static void SendJumpInPressed(string questId, string taskId, string coordinates, UIContext uiContext)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("quest_id", questId);
        data.Add("task_id", taskId);
        data.Add("jump_in_coordinates", coordinates);
        data.Add("quest_ui_source", uiContext.ToString());
        GenericAnalytics.SendAnalytic(QUEST_JUMP_IN_PRESSED, data);
    }

    public static void SendQuestLogVisibiltyChanged(bool isVisible, UIContext uiContext)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("quests_log_visible", isVisible.ToString());
        data.Add("quest_ui_source", uiContext.ToString());
        GenericAnalytics.SendAnalytic(QUEST_LOG_OPENED, data);
    }
}