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
                tutorialController.hudController.avatarEditorHud != null)
            {
                tutorialController.hudController.avatarEditorHud.OnOpen += AvatarEditorHud_OnOpen;
                tutorialController.hudController.avatarEditorHud.OnClose += AvatarEditorHud_OnClose;
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.avatarEditorHud != null)
            {
                tutorialController.hudController.avatarEditorHud.OnOpen -= AvatarEditorHud_OnOpen;
                tutorialController.hudController.avatarEditorHud.OnClose -= AvatarEditorHud_OnClose;
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

        private void AvatarEditorHud_OnOpen()
        {
            isRelatedFeatureActived = true;
            stepIsFinished = true;
            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }

        private void AvatarEditorHud_OnClose()
        {
            if (isRelatedFeatureActived)
                isRelatedFeatureActived = false;
        }
    }
}