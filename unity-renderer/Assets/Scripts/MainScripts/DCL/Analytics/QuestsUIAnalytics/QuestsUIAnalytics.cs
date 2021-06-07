using System;
using System.Collections.Generic;

public static class QuestsUIAnalytics
{
    private const string QUEST_PIN_CHANGED = "quest_pin_changed";
    private const string QUEST_JUMP_IN_PRESSED = "quest_jump_in_pressed";
    private const string QUEST_LOG_VISIBILITY_CHANGED = "quest_log_visibility_changed";

    private static DateTime? questLogSetVisibleTimeStamp = null;

    public enum UIContext
    {
        QuestsLog,
        QuestDetails,
        QuestsTracker,
    }

    public static void SendQuestPinChanged(string questId, bool isPinned, UIContext uiContext)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("quest_id", questId);
        data.Add("quest_ui_context", uiContext.ToString());
        data.Add("quest_is_pinned", isPinned.ToString());
        GenericAnalytics.SendAnalytic(QUEST_PIN_CHANGED, data);
    }

    public static void SendJumpInPressed(string questId, string taskId, string coordinates, UIContext uiContext)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("quest_id", questId);
        data.Add("task_id", taskId);
        data.Add("jump_in_coordinates", coordinates);
        data.Add("quest_ui_context", uiContext.ToString());
        GenericAnalytics.SendAnalytic(QUEST_JUMP_IN_PRESSED, data);
    }

    public static void SendQuestLogVisibiltyChanged(bool isVisible, string triggerContext)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("quests_log_visible", isVisible.ToString());
        data.Add("trigger_context", triggerContext);
        if (isVisible)
            questLogSetVisibleTimeStamp = DateTime.Now;
        else
        {
            if (questLogSetVisibleTimeStamp.HasValue)
            {
                data.Add("open_duration_ms", (DateTime.Now - questLogSetVisibleTimeStamp.Value).TotalMilliseconds.ToString());
                questLogSetVisibleTimeStamp = null;
            }
        }

        GenericAnalytics.SendAnalytic(QUEST_LOG_VISIBILITY_CHANGED, data);
    }
}