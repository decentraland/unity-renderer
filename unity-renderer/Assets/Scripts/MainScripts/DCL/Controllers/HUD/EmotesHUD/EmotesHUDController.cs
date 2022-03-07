using DCL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmotesHUDController : IHUD
{
    internal EmotesHUDView view;
    private BaseVariable<bool> emotesVisible => DataStore.i.HUDs.emotesVisible;
    private BaseCollection<StoredEmote> equippedEmotes => DataStore.i.emotes.equippedEmotes;
    private BaseVariable<bool> isStarMenuOpen => DataStore.i.exploreV2.isOpen;
    private bool shortcutsCanBeUsed => !isStarMenuOpen.Get();

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private InputAction_Trigger closeWindow;

    internal InputAction_Trigger shortcut0InputAction;
    internal InputAction_Trigger shortcut1InputAction;
    internal InputAction_Trigger shortcut2InputAction;
    internal InputAction_Trigger shortcut3InputAction;
    internal InputAction_Trigger shortcut4InputAction;
    internal InputAction_Trigger shortcut5InputAction;
    internal InputAction_Trigger shortcut6InputAction;
    internal InputAction_Trigger shortcut7InputAction;
    internal InputAction_Trigger shortcut8InputAction;
    internal InputAction_Trigger shortcut9InputAction;

    public EmotesHUDController()
    {
        closeWindow = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindow.OnTriggered += OnCloseWindowPressed;
        view = EmotesHUDView.Create();
        view.OnClose += OnViewClosed;
        view.onEmoteClicked += EmoteCalled;

        ownUserProfile.OnAvatarExpressionSet += OnAvatarEmoteSet;
        emotesVisible.OnChange += OnEmoteVisibleChanged;
        OnEmoteVisibleChanged(emotesVisible.Get(), false);

        equippedEmotes.OnSet += OnEquippedEmotesSet;
        OnEquippedEmotesSet(equippedEmotes.Get());

        ConfigureShortcuts();
    }

    public void SetVisibility(bool visible)
    {
        //TODO once kernel sends visible properly
        //expressionsVisible.Set(visible);
    }

    private void OnEmoteVisibleChanged(bool current, bool previous) { SetVisibility_Internal(current); }

    private void OnEquippedEmotesSet(IEnumerable<StoredEmote> equippedEmotes) { view.SetEmotes(equippedEmotes.ToList()); }

    public void SetVisibility_Internal(bool visible)
    {
        view.SetVisiblity(visible);

        if ( visible )
            DCL.Helpers.Utils.UnlockCursor();
    }

    public void Dispose()
    {
        view.OnClose -= OnViewClosed;
        view.onEmoteClicked -= EmoteCalled;
        closeWindow.OnTriggered -= OnCloseWindowPressed;
        ownUserProfile.OnAvatarExpressionSet -= OnAvatarEmoteSet;
        emotesVisible.OnChange -= OnEmoteVisibleChanged;
        equippedEmotes.OnSet -= OnEquippedEmotesSet;
        shortcut0InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut1InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut2InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut3InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut4InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut5InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut6InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut7InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut8InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        shortcut9InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;

        if (view != null)
        {
            view.CleanUp();
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    public void EmoteCalled(string id) { UserProfile.GetOwnUserProfile().SetAvatarExpression(id); }

    private void OnViewClosed() { emotesVisible.Set(false); }
    private void OnAvatarEmoteSet(string id, long timestamp) { emotesVisible.Set(false); }
    private void OnCloseWindowPressed(DCLAction_Trigger action) { emotesVisible.Set(false); }

    internal void ConfigureShortcuts()
    {
        shortcut0InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut0");
        shortcut0InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut1InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut1");
        shortcut1InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut2InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut2");
        shortcut2InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut3InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut3");
        shortcut3InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut4InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut4");
        shortcut4InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut5InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut5");
        shortcut5InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut6InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut6");
        shortcut6InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut7InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut7");
        shortcut7InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut8InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut8");
        shortcut8InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

        shortcut9InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut9");
        shortcut9InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;
    }

    internal void OnNumericShortcutInputActionTriggered(DCLAction_Trigger action)
    {
        if (!shortcutsCanBeUsed)
            return;

        switch (action)
        {
            case DCLAction_Trigger.ToggleShortcut0:
                PlayExpression(equippedEmotes[0]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut1:
                PlayExpression(equippedEmotes[1]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut2:
                PlayExpression(equippedEmotes[2]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut3:
                PlayExpression(equippedEmotes[3]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut4:
                PlayExpression(equippedEmotes[4]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut5:
                PlayExpression(equippedEmotes[5]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut6:
                PlayExpression(equippedEmotes[6]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut7:
                PlayExpression(equippedEmotes[7]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut8:
                PlayExpression(equippedEmotes[8]?.id);
                break;
            case DCLAction_Trigger.ToggleShortcut9:
                PlayExpression(equippedEmotes[9]?.id);
                break;
        }
    }

    internal void PlayExpression(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        ownUserProfile.SetAvatarExpression(id);
    }
}