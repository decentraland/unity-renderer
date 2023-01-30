using System;
using MainScripts.DCL.Utils;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.Profile
{
    public class ProfileHUDControllerDesktop : ProfileHUDController
    {
        private ProfileHUDViewDesktop viewDesktop;

        public ProfileHUDControllerDesktop(IUserProfileBridge userProfileBridge) : base(userProfileBridge)
        {
            viewDesktop = (ProfileHUDViewDesktop)view;
            viewDesktop.getButtonSignUp.onClick.RemoveAllListeners();
            viewDesktop.getButtonSignUp.onClick.AddListener(OnExitButtonClick); // When you exit the renderer, you will see the launcher where you can sign up
            viewDesktop.exitButtons.ForEach(e => e.onClick.AddListener(OnExitButtonClick));
        }

        private void OnExitButtonClick()
        {
            DesktopUtils.Quit();
        }

        public new void Dispose()
        {
            base.Dispose();
            viewDesktop.getButtonSignUp.onClick.RemoveAllListeners(); // When you exit the renderer, you will see the launcher where you can sign up
            viewDesktop.exitButtons.ForEach(e => e.onClick.RemoveAllListeners());
        }

        protected override GameObject GetViewPrefab()
        {
            return Resources.Load<GameObject>("ProfileHUDDesktop");
        }
    }
}
