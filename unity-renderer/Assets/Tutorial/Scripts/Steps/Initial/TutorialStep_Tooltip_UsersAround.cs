using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to view the players in scene and configure voice chat.
    /// </summary>
    public class TutorialStep_Tooltip_UsersAround : TutorialStep_Tooltip
    {
        private const string PLAYER_PREFS_VOICE_CHAT_FEATURE_SHOWED = "VoiceChatFeatureShowed";

        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.usersAroundListHud != null)
            {
                tutorialController.hudController.usersAroundListHud.OnOpen += UsersAroundListHud_OnOpen;
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            // TODO (Santi): This a TEMPORAL fix. It will be removed when we refactorize the tutorial system in order to make it compatible with incremental features.
            PlayerPrefs.SetInt(PLAYER_PREFS_VOICE_CHAT_FEATURE_SHOWED, 1);

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.usersAroundListHud != null)
            {
                tutorialController.hudController.usersAroundListHud.OnOpen -= UsersAroundListHud_OnOpen;
            }
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.minimapHud.usersAroundTooltipReference)
            {
                tooltipTransform.position = tutorialController.hudController.minimapHud.usersAroundTooltipReference.position;
            }
        }

        private void UsersAroundListHud_OnOpen()
        {
            stepIsFinished = true;
            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }
    }
}