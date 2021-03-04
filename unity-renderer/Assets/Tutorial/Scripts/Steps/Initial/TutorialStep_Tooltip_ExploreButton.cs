using System.Collections;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the Explore window from the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_ExploreButton : TutorialStep_Tooltip
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.exploreHud != null)
            {
                tutorialController.hudController.exploreHud.OnOpen += ExploreHud_OnOpen;
                tutorialController.hudController.exploreHud.OnClose += ExploreHud_OnClose;
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.exploreHud != null)
            {
                tutorialController.hudController.exploreHud.OnOpen -= ExploreHud_OnOpen;
                tutorialController.hudController.exploreHud.OnClose -= ExploreHud_OnClose;
            }
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud.exploreTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.taskbarHud.exploreTooltipReference.position;
            }
        }

        private void ExploreHud_OnOpen()
        {
            isRelatedFeatureActived = true;
            stepIsFinished = true;
            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }

        private void ExploreHud_OnClose()
        {
            if (isRelatedFeatureActived)
                isRelatedFeatureActived = false;
        }
    }
}