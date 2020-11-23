namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the more menu in the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_TaskbarMoreButton : TutorialStep_Tooltip
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud != null)
                tutorialController.hudController.taskbarHud.moreMenu.OnMoreMenuOpened += MoreMenu_OnMoreMenuOpened;
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud != null)
                tutorialController.hudController.taskbarHud.moreMenu.OnMoreMenuOpened -= MoreMenu_OnMoreMenuOpened;
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud != null)
            {
                if (tutorialController.hudController.taskbarHud.tutorialTooltipReference)
                {
                    tooltipTransform.position =
                        tutorialController.hudController.taskbarHud.tutorialTooltipReference.position;
                }
            }
        }

        private void MoreMenu_OnMoreMenuOpened(bool isVisible)
        {
            if (isVisible)
            {
                isRelatedFeatureActived = true;
                stepIsFinished = true;
                tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
            }
            else if (isRelatedFeatureActived)
            {
                isRelatedFeatureActived = false;
            }
        }
    }
}