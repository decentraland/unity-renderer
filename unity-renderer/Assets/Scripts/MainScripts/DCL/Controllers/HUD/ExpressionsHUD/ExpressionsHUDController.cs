using System;
using DCL;
using UnityEngine;

public class ExpressionsHUDController : IHUD
{
    internal ExpressionsHUDView view;
    private BaseVariable<bool> expressionsVisible => DataStore.i.HUDs.expressionsVisible;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private InputAction_Trigger closeWindow;

    public ExpressionsHUDController()
    {
        closeWindow = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindow.OnTriggered += OnCloseWindowPressed;
        view = ExpressionsHUDView.Create();
        view.Initialize(ExpressionCalled);
        view.OnClose += OnViewClosed;

        ownUserProfile.OnAvatarExpressionSet += OnAvatarExpressionSet;
        expressionsVisible.OnChange += OnExpressionVisibleChanged;
        OnExpressionVisibleChanged(expressionsVisible.Get(), false);
    }

    public void SetVisibility(bool visible)
    {
        //TODO once kernel sends visible properly
        //expressionsVisible.Set(visible);
    }
    private void OnExpressionVisibleChanged(bool current, bool previous) { SetVisibility_Internal(current); }
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
        ownUserProfile.OnAvatarExpressionSet -= OnAvatarExpressionSet;
        expressionsVisible.OnChange -= OnExpressionVisibleChanged;

        if (view != null)
        {
            view.CleanUp();
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    public void ExpressionCalled(string id) { UserProfile.GetOwnUserProfile().SetAvatarExpression(id); }

    private void OnViewClosed() { expressionsVisible.Set(false); }
    private void OnAvatarExpressionSet(string id, long timestamp) { expressionsVisible.Set(false); }
    private void OnCloseWindowPressed(DCLAction_Trigger action) { expressionsVisible.Set(false); }
}