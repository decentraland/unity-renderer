using System.Collections;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the more menu in the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_TaskbarMoreButton : TutorialStep_Tooltip
    {
        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud.tutorialTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.taskbarHud.tutorialTooltipReference.position;
            }
        }
    }
}