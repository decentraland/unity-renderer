using DCL;
using UnityEngine;

public interface IQuestSectionComponentView
{
    /// <summary>
    /// Encapsulates the quest HUD into the section.
    /// </summary>
    void ConfigureQuest();
}

public class QuestSectionComponentView : BaseComponentView, IQuestSectionComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Transform contentContainer;

    public override void RefreshControl() { }

    public void ConfigureQuest() { DataStore.i.exploreV2.configureQuestInFullscreenMenu.Set(contentContainer, true); }
}