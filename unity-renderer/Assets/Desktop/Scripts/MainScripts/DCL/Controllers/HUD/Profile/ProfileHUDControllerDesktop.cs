using DCL;
using SocialFeaturesAnalytics;
using UnityEngine;
using DCL.Helpers;


namespace MainScripts.DCL.Controllers.HUD.Profile
{
    public class ProfileHUDControllerDesktop : ProfileHUDController
    {
        private readonly ProfileHUDViewDesktop_V2 viewDesktop;

        public ProfileHUDControllerDesktop(IProfileHUDView view, IUserProfileBridge userProfileBridge, ISocialAnalytics socialAnalytics, DataStore dataStore)
            : base(view, userProfileBridge, socialAnalytics, dataStore)
        {
            viewDesktop = (ProfileHUDViewDesktop_V2)view;
            viewDesktop.getButtonSignUp.onClick.RemoveAllListeners();
            viewDesktop.getButtonSignUp.onClick.AddListener(OnExitButtonClick); // When you exit the renderer, you will see the launcher where you can sign up
            viewDesktop.exitButtons.ForEach(e => e.onClick.AddListener(OnExitButtonClick));
        }

        private void OnExitButtonClick()
        {
            Utils.QuitApplication();
        }

        public new void Dispose()
        {
            base.Dispose();
            viewDesktop.getButtonSignUp.onClick.RemoveAllListeners(); // When you exit the renderer, you will see the launcher where you can sign up
            viewDesktop.exitButtons.ForEach(e => e.onClick.RemoveAllListeners());
        }
    }
}

