using UnityEngine;

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
            }

            DataStore.i.exploreV2.isOpen.OnChange += ExploreV2IsOpenChanged;
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.profileHud != null)
            {
                tutorialController.hudController.profileHud.OnOpen -= ProfileHud_OnOpen;
            }

            DataStore.i.exploreV2.isOpen.OnChange -= ExploreV2IsOpenChanged;
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (DataStore.i.exploreV2.profileCardTooltipReference.Get() != null)
                tooltipTransform.position = DataStore.i.exploreV2.profileCardTooltipReference.Get().position;
        }

        internal void ProfileHud_OnOpen()
        {
            tutorialController.SetNextSkippedSteps(0);
            stepIsFinished = true;
        }

        internal void ExploreV2IsOpenChanged(bool current, bool previous)
        {
            if (!current)
            {
                tutorialController.SetNextSkippedSteps(7);
                stepIsFinished = true;
            }
        }
    }
}