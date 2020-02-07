using System.Collections;
using UnityEngine;

public class TutorialStep : MonoBehaviour
{
    public enum Id
    {
        NONE = 0,
        INITIAL_SCENE = 1,
        GENESIS_PLAZA = 2,
        CHAT_AND_AVATAR_EXPRESSIONS = 3,
        FINISHED = 99,
    }

    public Id stepId;

    public virtual void OnStepStart()
    {
    }
    public virtual void OnStepFinished()
    {
    }

    public virtual IEnumerator OnStepExecute()
    {
        yield break;
    }

    public virtual IEnumerator ShowTooltip(TutorialTooltip tooltip, bool autoHide = true)
    {
        if (tooltip != null)
        {
            tooltip.Show();

            if (autoHide)
            {
                yield return WaitForSecondsWhenRenderingEnabled(tooltip.secondsOnScreen);
                tooltip.Hide();
            }
        }
    }

    public virtual void HideTooltip(TutorialTooltip tooltip)
    {
        tooltip.Hide();
    }

    public virtual IEnumerator WaitIdleTime()
    {
        yield return WaitForSecondsWhenRenderingEnabled(TutorialController.DEFAULT_STAGE_IDLE_TIME);
    }

    public virtual IEnumerator WaitForSecondsWhenRenderingEnabled(float seconds)
    {
        yield return new WaitUntil(() => RenderingController.i.renderingEnabled);
        yield return new WaitForSeconds(seconds);
    }

}
