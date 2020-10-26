namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to Jump In the Genesis Plaza and become a DCL Citizen.
    /// </summary>
    public class TutorialStep_Tooltip_GoToGenesisButton : TutorialStep_Tooltip
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.goToGenesisPlazaHud != null)
            {
                tutorialController.hudController.goToGenesisPlazaHud.OnOpen += GoToGenesisPlazaHud_OnOpen;
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.goToGenesisPlazaHud != null)
            {
                tutorialController.hudController.goToGenesisPlazaHud.OnOpen -= GoToGenesisPlazaHud_OnOpen;
            }
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud.goToGenesisTooltipReference)
            {
                tutorialController.hudController.taskbarHud.ShowGoToGenesisPlazaButton();
                tooltipTransform.position = tutorialController.hudController.taskbarHud.goToGenesisTooltipReference.position;
            }
        }

        private void GoToGenesisPlazaHud_OnOpen(bool isVisible)
        {
            stepIsFinished = true;
            isRelatedFeatureActived = false;
            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }
    }
}