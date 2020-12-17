namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the Backpack window from the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_BackpackButton : TutorialStep_Tooltip
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.profileHud != null)
            {
                tutorialController.hudController.profileHud.OnOpen += ProfileHud_OnOpen;
                tutorialController.hudController.profileHud.OnClose += ProfileHud_OnClose;
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.profileHud != null)
            {
                tutorialController.hudController.profileHud.OnOpen -= ProfileHud_OnOpen;
                tutorialController.hudController.profileHud.OnClose -= ProfileHud_OnClose;
            }
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.profileHud.backpackTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.profileHud.backpackTooltipReference.position;
            }
        }

        private void ProfileHud_OnOpen()
        {
            isRelatedFeatureActived = true;
            stepIsFinished = true;
            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }

        private void ProfileHud_OnClose()
        {
            if (isRelatedFeatureActived)
                isRelatedFeatureActived = false;
        }
    }
}