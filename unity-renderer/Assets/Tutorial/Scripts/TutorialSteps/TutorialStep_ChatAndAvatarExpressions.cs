using System.Collections;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Tutorial
{
    public class TutorialStep_ChatAndAvatarExpressions : TutorialStep
    {
        const bool ENABLE_DAILY_REWARD_TOOLTIP = false;

        [SerializeField] TutorialTooltip chatTooltip = null;
        [SerializeField] TutorialTooltip avatarExpressionTooltip = null;
        [SerializeField] TutorialTooltip gotoCommandTooltip = null;
        [SerializeField] TutorialTooltip avatarHUDTooltip = null;
        [SerializeField] TutorialTooltip dailyRewardTooltip = null;

        bool isWelcomeHudVisible = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            if (HUDController.i != null)
            {
                HUDController.i.avatarHud.SetVisibility(false);
                HUDController.i.expressionsHud.SetVisibility(false);
                if (HUDController.i.messageOfTheDayHud != null)
                {
                    HUDController.i.messageOfTheDayHud.OnConfirmed += OnWelcomePopupConfirm;
                    HUDController.i.messageOfTheDayHud.OnDismissed += OnWelcomePopupDismiss;
                }
            }

            TutorialController.i.SetChatVisible(false);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();
            if (HUDController.i != null)
            {
                if (HUDController.i.messageOfTheDayHud != null)
                {
                    HUDController.i.messageOfTheDayHud.OnConfirmed -= OnWelcomePopupConfirm;
                    HUDController.i.messageOfTheDayHud.OnDismissed -= OnWelcomePopupDismiss;
                }
            }
        }

        public override IEnumerator OnStepExecute()
        {
            yield return WaitIdleTime();

            TutorialController.i?.SetChatVisible(true);

            yield return chatTooltip.ShowAndHideRoutine();
            yield return WaitIdleTime();

            HUDController.i?.expressionsHud.SetVisibility(true);

            yield return avatarExpressionTooltip.ShowAndHideRoutine();
            yield return WaitIdleTime();

            yield return gotoCommandTooltip.ShowAndHideRoutine();
            yield return WaitIdleTime();

            HUDController.i?.avatarHud.SetVisibility(true);

            yield return avatarHUDTooltip.ShowAndHideRoutine();
            yield return WaitIdleTime();

            if (ENABLE_DAILY_REWARD_TOOLTIP)
            {
                yield return dailyRewardTooltip.ShowAndHideRoutine();
                yield return WaitIdleTime();
            }

            isWelcomeHudVisible = false;
            if (HUDController.i != null && HUDController.i.messageOfTheDayHud != null)
            {
                isWelcomeHudVisible = true;
                HUDController.i.messageOfTheDayHud.SetVisibility(true);
            }
            yield return new WaitUntil(() => !isWelcomeHudVisible);

            Utils.UnlockCursor();
            TutorialController.i.TriggerEmailPrompt();
        }

        private void OnWelcomePopupConfirm()
        {
            if (UserProfile.GetOwnUserProfile().hasConnectedWeb3)
            {
                DCLCharacterController.OnPositionSet += OnTeleport;
            }
            else
            {
                isWelcomeHudVisible = false;
            }
        }

        private void OnWelcomePopupDismiss()
        {
            isWelcomeHudVisible = false;
        }

        private void OnTeleport(DCLCharacterPosition characterPosition)
        {
            DCLCharacterController.OnPositionSet -= OnTeleport;
            isWelcomeHudVisible = false;
        }
    }
}
