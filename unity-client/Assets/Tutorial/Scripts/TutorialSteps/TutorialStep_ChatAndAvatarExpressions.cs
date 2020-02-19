using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    public class TutorialStep_ChatAndAvatarExpressions : TutorialStep
    {
        [SerializeField] TutorialTooltip chatTooltip = null;
        [SerializeField] TutorialTooltip avatarExpressionTooltip = null;
        [SerializeField] TutorialTooltip gotoCommandTooltip = null;
        [SerializeField] TutorialTooltip avatarHUDTooltip = null;
        [SerializeField] TutorialTooltip dailyRewardTooltip = null;

        public override void OnStepStart()
        {
            base.OnStepStart();

            HUDController.i?.avatarHud.SetVisibility(false);
            HUDController.i?.expressionsHud.SetVisibility(false);
            TutorialController.i.SetChatVisible(false);
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

            yield return dailyRewardTooltip.ShowAndHideRoutine();
            yield return WaitIdleTime();
        }
    }
}
