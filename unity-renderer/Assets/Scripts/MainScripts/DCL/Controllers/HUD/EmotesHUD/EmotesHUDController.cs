using DCL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmotesHUDController : IHUD
{
    internal EmotesHUDView view;
    private BaseVariable<bool> emotesVisible => DataStore.i.HUDs.emotesVisible;
    private BaseCollection<StoredEmote> equippedEmotes => DataStore.i.emotes.equippedEmotes;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private InputAction_Trigger closeWindow;

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
}