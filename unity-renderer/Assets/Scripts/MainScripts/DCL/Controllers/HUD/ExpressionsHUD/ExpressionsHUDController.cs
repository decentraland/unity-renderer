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
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisiblity(visible);
    }

    public void Dispose()
    {
        ownUserProfile.OnUpdate -= userProfileUpdateDelegate;
        view.CleanUp();
    }

    public void ExpressionCalled(string id)
    {
        UserProfile.GetOwnUserProfile().SetAvatarExpression(id);
    }
}
