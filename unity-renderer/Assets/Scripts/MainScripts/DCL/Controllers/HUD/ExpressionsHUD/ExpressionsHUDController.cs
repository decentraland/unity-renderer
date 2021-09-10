using System;
using DCL;
using UnityEngine;

public class ExpressionsHUDController : IHUD
{
    internal ExpressionsHUDView view;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    public ExpressionsHUDController()
    {
        view = ExpressionsHUDView.Create();
        view.Initialize(ExpressionCalled);

        ownUserProfile.OnAvatarExpressionSet += OnAvatarExpressionSet;
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisiblity(visible);

        if ( visible )
        {
            ownUserProfile.snapshotObserver.AddListener(view.UpdateAvatarSprite);
        }
        else
        {
            ownUserProfile.snapshotObserver.RemoveListener(view.UpdateAvatarSprite);
        }
    }

    public void Dispose()
    {
        ownUserProfile.snapshotObserver.RemoveListener(view.UpdateAvatarSprite);
        ownUserProfile.OnAvatarExpressionSet -= OnAvatarExpressionSet;

        if (view != null)
        {
            view.CleanUp();
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    public void ExpressionCalled(string id) { UserProfile.GetOwnUserProfile().SetAvatarExpression(id); }

    private void OnAvatarExpressionSet(string id, long timestamp)
    {
        if (view.IsContentVisible())
        {
            view.HideContent();
        }
    }
}