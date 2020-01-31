using System;
using DCL.Interface;

public class ExpressionsHUDController : IHUD, IDisposable
{
    internal ExpressionsHUDView view;
    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private Action<UserProfile> userProfileUpdateDelegate;
    
    public ExpressionsHUDController()
    {
        view = ExpressionsHUDView.Create();
        view.Initialize(ExpressionCalled);
        userProfileUpdateDelegate = profile => view.UpdateAvatarTexture(profile.faceSnapshot); 
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