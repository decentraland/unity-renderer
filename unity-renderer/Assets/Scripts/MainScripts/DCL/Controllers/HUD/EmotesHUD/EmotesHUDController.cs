using System;
using DCL;
using UnityEngine;

public class EmotesHUDController : IHUD
{
    internal EmotesHUDView view;
    private BaseVariable<bool> emotesVisible => DataStore.i.HUDs.emotesVisible;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private InputAction_Trigger closeWindow;

    public EmotesHUDController()
    {
        closeWindow = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindow.OnTriggered += OnCloseWindowPressed;
        view = EmotesHUDView.Create();
        view.Initialize(EmoteCalled);
        view.OnClose += OnViewClosed;

        ownUserProfile.OnAvatarExpressionSet += OnAvatarEmoteSet;
        emotesVisible.OnChange += OnEmoteVisibleChanged;
        OnEmoteVisibleChanged(emotesVisible.Get(), false);
    }

    public void SetVisibility(bool visible)
    {
        //TODO once kernel sends visible properly
        //expressionsVisible.Set(visible);
    }
    private void OnEmoteVisibleChanged(bool current, bool previous) { SetVisibility_Internal(current); }
    public void SetVisibility_Internal(bool visible)
    {
        view.SetVisiblity(visible);

        if ( visible )
        {
            DCL.Helpers.Utils.UnlockCursor();
            ownUserProfile.snapshotObserver.AddListener(view.UpdateAvatarSprite);
        }
        else
        {
            DCL.Helpers.Utils.LockCursor();
            ownUserProfile.snapshotObserver.RemoveListener(view.UpdateAvatarSprite);
        }
    }

    public void Dispose()
    {
        ownUserProfile.snapshotObserver.RemoveListener(view.UpdateAvatarSprite);
        closeWindow.OnTriggered -= OnCloseWindowPressed;
        ownUserProfile.OnAvatarExpressionSet -= OnAvatarEmoteSet;
        emotesVisible.OnChange -= OnEmoteVisibleChanged;

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