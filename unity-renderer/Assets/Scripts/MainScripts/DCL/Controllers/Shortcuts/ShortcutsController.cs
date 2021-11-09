using System;
using DCL;
using UnityEngine;

public class ShortcutsController : IDisposable
{
    internal InputAction_Trigger toggleQuestsPanel;
    internal InputAction_Trigger toggleAvatarNames;
    internal InputAction_Trigger toggleControls;
    internal InputAction_Trigger toggleAvatarEditor;
    internal InputAction_Trigger toggleExplore;
    internal InputAction_Trigger toggleExpressionsHUD;
    internal InputAction_Trigger toggleNavMap;

    public ShortcutsController()
    {
        toggleQuestsPanel = Resources.Load<InputAction_Trigger>("ToggleQuestsPanelHUD");
        toggleAvatarNames = Resources.Load<InputAction_Trigger>("ToggleAvatarNames");
        toggleControls = Resources.Load<InputAction_Trigger>("ToggleControlsHud");
        toggleAvatarEditor = Resources.Load<InputAction_Trigger>("ToggleAvatarEditorHud");
        toggleExplore = Resources.Load<InputAction_Trigger>("ToggleExploreHud");
        toggleExpressionsHUD = Resources.Load<InputAction_Trigger>("OpenExpressions");

        //TODO: These require a small refactor to convert to DataStore
        //toggleNavMap = Resources.Load<InputAction_Trigger>("ToggleNavMap");

        Subscribe();
    }

    internal void Subscribe()
    {

        toggleControls.OnTriggered += ToggleControlsTriggered;
        toggleAvatarEditor.OnTriggered += ToggleAvatarEditorTriggered;
        toggleAvatarNames.OnTriggered += ToggleAvatarNamesTriggered;
        toggleQuestsPanel.OnTriggered += ToggleQuestPanel;
        toggleExplore.OnTriggered += ToggleExploreTriggered;
        toggleExpressionsHUD.OnTriggered += ToggleExpressionsTriggered;

        //TODO
        //toggleNavMap.OnTriggered += ToggleNavMapTriggered;
    }

    internal void Unsubscribe()
    {
        toggleControls.OnTriggered -= ToggleControlsTriggered;
        toggleAvatarEditor.OnTriggered -= ToggleAvatarEditorTriggered;
        toggleAvatarNames.OnTriggered -= ToggleAvatarNamesTriggered;
        toggleQuestsPanel.OnTriggered -= ToggleQuestPanel;
        toggleExplore.OnTriggered -= ToggleExploreTriggered;
        toggleExpressionsHUD.OnTriggered -= ToggleExpressionsTriggered;

        //TODO
        //toggleNavMap.OnTriggered -= ToggleNavMapTriggered;
    }

    private void ToggleControlsTriggered(DCLAction_Trigger action) { DataStore.i.HUDs.controlsVisible.Set(!DataStore.i.HUDs.controlsVisible.Get()); }
    private void ToggleAvatarEditorTriggered(DCLAction_Trigger action) { DataStore.i.HUDs.avatarEditorVisible.Set(!DataStore.i.HUDs.avatarEditorVisible.Get()); }
    private void ToggleAvatarNamesTriggered(DCLAction_Trigger action) { DataStore.i.HUDs.avatarNamesVisible.Set(!DataStore.i.HUDs.avatarNamesVisible.Get()); }
    private void ToggleQuestPanel(DCLAction_Trigger action)
    {
        bool value = !DataStore.i.HUDs.questsPanelVisible.Get();
        QuestsUIAnalytics.SendQuestLogVisibiltyChanged(value, "input_action");
        DataStore.i.HUDs.questsPanelVisible.Set(value);
    }
    private void ToggleExploreTriggered(DCLAction_Trigger action) { DataStore.i.exploreV2.isOpen.Set(!DataStore.i.exploreV2.isOpen.Get()); }
    private void ToggleExpressionsTriggered(DCLAction_Trigger action) { DataStore.i.HUDs.expressionsVisible.Set(!DataStore.i.HUDs.expressionsVisible.Get()); }

    public void Dispose() { Unsubscribe(); }

}