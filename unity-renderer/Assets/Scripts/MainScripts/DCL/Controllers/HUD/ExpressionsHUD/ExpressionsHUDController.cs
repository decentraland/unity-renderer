using System;

public class ExpressionsHUDController : IHUD
{
    internal ExpressionsHUDView view;
    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private Action<UserProfile> userProfileUpdateDelegate;

    public ExpressionsHUDController()
    {
        view = ExpressionsHUDView.Create();
        view.Initialize(ExpressionCalled);
        userProfileUpdateDelegate = profile => view.UpdateAvatarSprite(profile.faceSnapshot);
        userProfileUpdateDelegate.Invoke(ownUserProfile);
        ownUserProfile.OnUpdate += userProfileUpdateDelegate;
        ownUserProfile.OnAvatarExpressionSet += OnAvatarExpressionSet;
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisiblity(visible);
    }

    public void Dispose()
    {
        ownUserProfile.OnUpdate -= userProfileUpdateDelegate;
        ownUserProfile.OnAvatarExpressionSet -= OnAvatarExpressionSet;
        view.CleanUp();
    }

    public void ExpressionCalled(string id)
    {
        UserProfile.GetOwnUserProfile().SetAvatarExpression(id);
    }

    private void OnAvatarExpressionSet(string id)
    {
        if (view.IsContentVisible())
        {
            view.HideContent();
        }
    }
}
