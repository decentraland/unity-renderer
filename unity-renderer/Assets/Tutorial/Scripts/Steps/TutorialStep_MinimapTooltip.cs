using System.Collections;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the Minimap.
    /// </summary>
    public class TutorialStep_MinimapTooltip : TutorialStep_Tooltip
    {
        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.minimapHud.minimapTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.minimapHud.minimapTooltipReference.position;
            }
        }
    }
}