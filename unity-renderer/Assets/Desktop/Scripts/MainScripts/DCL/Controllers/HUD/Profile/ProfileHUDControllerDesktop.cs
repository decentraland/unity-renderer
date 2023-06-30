using DCL;
using DCL.Browser;
using DCL.Helpers;
using DCL.MyAccount;
using SocialFeaturesAnalytics;

namespace MainScripts.DCL.Controllers.HUD.Profile
{
    public class ProfileHUDControllerDesktop : ProfileHUDController
    {
        private readonly ProfileHUDViewDesktop_V2 viewDesktop;

        public ProfileHUDControllerDesktop(ProfileHUDViewDesktop_V2 view, IUserProfileBridge userProfileBridge,
            ISocialAnalytics socialAnalytics, DataStore dataStore,
            MyAccountCardController myAccountCardController, IBrowserBridge browserBridge)
            : base(view, userProfileBridge, socialAnalytics, dataStore, myAccountCardController, browserBridge)
        {
            viewDesktop = view;
            viewDesktop.getButtonSignUp.onClick.RemoveAllListeners();
            viewDesktop.getButtonSignUp.onClick.AddListener(OnExitButtonClick); // When you exit the renderer, you will see the launcher where you can sign up
            viewDesktop.exitButtons.ForEach(e => e.onClick.AddListener(OnExitButtonClick));
        }

        private void OnExitButtonClick()
        {
            Utils.QuitApplication();
        }

        public override void Dispose()
        {
            base.Dispose();
            viewDesktop.getButtonSignUp.onClick.RemoveAllListeners(); // When you exit the renderer, you will see the launcher where you can sign up
            viewDesktop.exitButtons.ForEach(e => e.onClick.RemoveAllListeners());
        }
    }
}

