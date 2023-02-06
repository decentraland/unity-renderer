using DCL.Helpers;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the Explore window from the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_ExploreButton : TutorialStep_Tooltip
    {
        private const string PLAYER_PREFS_START_MENU_SHOWED = "StartMenuFeatureShowed";
        private const int TEACHER_CANVAS_SORT_ORDER_START = 5;

        public override void OnStepStart()
        {
            base.OnStepStart();

            DataStore.i.exploreV2.isOpen.OnChange += ExploreV2IsOpenChanged;

            tutorialController.SetTeacherCanvasSortingOrder(TEACHER_CANVAS_SORT_ORDER_START);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            // TODO (Santi): This a TEMPORAL fix. It will be removed when we refactorize the tutorial system in order to make it compatible with incremental features.
            PlayerPrefsBridge.SetInt(PLAYER_PREFS_START_MENU_SHOWED, 1);

            DataStore.i.exploreV2.isOpen.OnChange -= ExploreV2IsOpenChanged;
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.profileHud != null &&
                tutorialController.hudController.profileHud.TutorialTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.profileHud.TutorialTooltipReference.position;
            }
        }

        internal void ExploreV2IsOpenChanged(bool current, bool previous)
        {
            if (current)
                stepIsFinished = true;
        }
    }
}