using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShowHideAnimator : MonoBehaviour
{
    public bool hideOnEnable = true;
    public float animSpeedFactor = 1.0f;
    public bool disableAfterFadeOut;
    public string visibleParam = "visible";

    private Animator animatorValue;

    private int? visibleParamHashValue = null;

    public bool isVisible => animator.GetBool(visibleParamHash);

    private Animator animator => animatorValue ??= GetComponent<Animator>();

    private int visibleParamHash
    {
        get
        {
            if (!visibleParamHashValue.HasValue)
                visibleParamHashValue = Animator.StringToHash(visibleParam);

            return visibleParamHashValue.Value;
        }
    }

    private void OnEnable()
    {
        if ( hideOnEnable )
        {
            Hide(true);
        }
    }

    public event System.Action<ShowHideAnimator> OnWillFinishHide;
    public event System.Action<ShowHideAnimator> OnWillFinishStart;

    public void Show(bool instant = false)
    {
        animator.speed = animSpeedFactor;

        if ( animator.isActiveAndEnabled )
            animator.SetBool(visibleParamHash, true);

        if (instant)
            animator.Update(10);
    }

    public void Hide(bool instant = false)
    {
        animator.speed = animSpeedFactor;

        if ( animator.isActiveAndEnabled )
            animator.SetBool(visibleParamHash, false);

        if (instant)
            animator.Update(10);
    }

    [UsedImplicitly]
    public void AnimEvent_HideFinished()
    {
        OnWillFinishHide?.Invoke(this);

        if (disableAfterFadeOut && gameObject != null)
        {
            gameObject.SetActive(false);
        }
    }

    [UsedImplicitly]
    public void AnimEvent_ShowFinished() =>
        OnWillFinishStart?.Invoke(this);
}