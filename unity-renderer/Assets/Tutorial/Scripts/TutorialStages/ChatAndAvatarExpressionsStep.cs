using System.Collections;
using UnityEngine;

public class ChatAndAvatarExpressionsStep : TutorialStep
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
    }

    public override IEnumerator OnStepExecute()
    {
        yield return WaitIdleTime();

        TutorialController.i?.SetChatVisible(true);

        yield return ShowTooltip(chatTooltip);
        yield return WaitIdleTime();

        HUDController.i?.expressionsHud.SetVisibility(true);
        yield return ShowTooltip(avatarExpressionTooltip);
        yield return WaitIdleTime();

        yield return ShowTooltip(gotoCommandTooltip);
        yield return WaitIdleTime();

        HUDController.i?.avatarHud.SetVisibility(true);
        yield return ShowTooltip(avatarHUDTooltip);
        yield return WaitIdleTime();

        yield return ShowTooltip(dailyRewardTooltip);
        yield return WaitIdleTime();
    }
}
