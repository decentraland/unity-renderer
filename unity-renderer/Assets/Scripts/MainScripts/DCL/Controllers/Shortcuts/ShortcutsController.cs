using System;
using DCL;
using ExploreV2Analytics;
using UnityEngine;

public class ShortcutsController : IDisposable
{
    internal InputAction_Trigger toggleQuestsPanel;
    internal InputAction_Trigger toggleAvatarNames;
    internal InputAction_Trigger toggleControls;
    internal InputAction_Trigger toggleAvatarEditor;
    internal InputAction_Trigger toggleStartMenu;
    internal InputAction_Trigger toggleNavMap;
    internal InputAction_Trigger togglePlacesAndEvents;
    internal InputAction_Hold toggleExpressionsHUD;

    public ShortcutsController()
    {
        toggleQuestsPanel = Resources.Load<InputAction_Trigger>("ToggleQuestsPanelHUD");
        toggleAvatarNames = Resources.Load<InputAction_Trigger>("ToggleAvatarNames");
        toggleControls = Resources.Load<InputAction_Trigger>("ToggleControlsHud");
        toggleAvatarEditor = Resources.Load<InputAction_Trigger>("ToggleAvatarEditorHud");
        toggleStartMenu = Resources.Load<InputAction_Trigger>("ToggleStartMenu");
        toggleNavMap = Resources.Load<InputAction_Trigger>("ToggleNavMap");
        togglePlacesAndEvents = Resources.Load<InputAction_Trigger>("TogglePlacesAndEventsHud");
        toggleExpressionsHUD = Resources.Load<InputAction_Hold>("OpenExpressions");

        Subscribe();
    }

    internal void Subscribe()
    {

        toggleControls.OnTriggered += ToggleControlsTriggered;
        toggleAvatarEditor.OnTriggered += ToggleAvatarEditorTriggered;
        toggleAvatarNames.OnTriggered += ToggleAvatarNamesTriggered;
        toggleQuestsPanel.OnTriggered += ToggleQuestPanel;
        toggleStartMenu.OnTriggered += ToggleStartMenuTriggered;
        toggleNavMap.OnTriggered += ToggleNavMapTriggered;
        togglePlacesAndEvents.OnTriggered += TogglePlacesAndEventsTriggered;
        toggleExpressionsHUD.OnFinished += ToggleExpressionsTriggered;
    }

    internal void Unsubscribe()
    {
        toggleControls.OnTriggered -= ToggleControlsTriggered;
        toggleAvatarEditor.OnTriggered -= ToggleAvatarEditorTriggered;
        toggleAvatarNames.OnTriggered -= ToggleAvatarNamesTriggered;
        toggleQuestsPanel.OnTriggered -= ToggleQuestPanel;
        toggleStartMenu.OnTriggered -= ToggleStartMenuTriggered;
        toggleNavMap.OnTriggered -= ToggleNavMapTriggered;
        togglePlacesAndEvents.OnTriggered -= TogglePlacesAndEventsTriggered;
        toggleExpressionsHUD.OnFinished -= ToggleExpressionsTriggered;
    }

    private void ToggleControlsTriggered(DCLAction_Trigger action) { DataStore.i.HUDs.controlsVisible.Set(!DataStore.i.HUDs.controlsVisible.Get()); }
    
    private void ToggleAvatarEditorTriggered(DCLAction_Trigger action) 
    {
        if (!DataStore.i.HUDs.isAvatarEditorInitialized.Get())
            return;

        DataStore.i.HUDs.avatarEditorVisible.Set(!DataStore.i.HUDs.avatarEditorVisible.Get()); 
    }

    private void ToggleAvatarNamesTriggered(DCLAction_Trigger action) { DataStore.i.HUDs.avatarNamesVisible.Set(!DataStore.i.HUDs.avatarNamesVisible.Get()); }

    private void ToggleQuestPanel(DCLAction_Trigger action)
    {
        if (!DataStore.i.Quests.isInitialized.Get())
            return;

        bool value = !DataStore.i.HUDs.questsPanelVisible.Get();
        SendQuestToggledAnalytic(value);
        DataStore.i.HUDs.questsPanelVisible.Set(value);
    }

    private void ToggleStartMenuTriggered(DCLAction_Trigger action)
    {
        bool value = !DataStore.i.exploreV2.isOpen.Get();
        if (DataStore.i.common.isSignUpFlow.Get()) return;

        if (value)
        {
            SendExploreToggledAnalytics(value);
            DataStore.i.exploreV2.isOpen.Set(value);
        }
        else
        {
            DataStore.i.exploreV2.currentSectionIndex.Set(DataStore.i.exploreV2.currentSectionIndex.Get() + 1);
        }
    }

    private void ToggleNavMapTriggered(DCLAction_Trigger action) 
    {
        if (!DataStore.i.HUDs.isNavMapInitialized.Get())
            return;

        DataStore.i.HUDs.navmapVisible.Set(!DataStore.i.HUDs.navmapVisible.Get()); 
    }

    private void TogglePlacesAndEventsTriggered(DCLAction_Trigger action)
    {
        if (!DataStore.i.exploreV2.isPlacesAndEventsSectionInitialized.Get())
            return;

        DataStore.i.exploreV2.placesAndEventsVisible.Set(!DataStore.i.exploreV2.placesAndEventsVisible.Get());
    }

    private void ToggleExpressionsTriggered(DCLAction_Hold action) { DataStore.i.HUDs.emotesVisible.Set(!DataStore.i.HUDs.emotesVisible.Get()); }

    public void Dispose() { Unsubscribe(); }

    // In the future the analytics will be received through DI in the shape of a service locator,
    // so we can remove these methods and mock the locator itself    
    internal virtual void SendQuestToggledAnalytic(bool value) { QuestsUIAnalytics.SendQuestLogVisibiltyChanged(value, "input_action"); }
    internal virtual void SendExploreToggledAnalytics(bool value) { new ExploreV2Analytics.ExploreV2Analytics().SendStartMenuVisibility(value, ExploreUIVisibilityMethod.FromShortcut); }

}