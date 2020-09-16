using System.Collections;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the Explore window from the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_ExploreButton : TutorialStep_Tooltip
    {
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
    }
}