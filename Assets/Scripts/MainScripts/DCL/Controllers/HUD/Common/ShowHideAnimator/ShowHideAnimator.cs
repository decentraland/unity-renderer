using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShowHideAnimator : MonoBehaviour
{
    public event System.Action<ShowHideAnimator> OnWillFinishHide;
    public event System.Action<ShowHideAnimator> OnWillFinishStart;

    public bool hideOnEnable = true;
    public float animSpeedFactor = 1.0f;
    public bool disableAfterFadeOut;
    public string visibleParam = "visible";

    public bool isVisible => animator.GetBool(visibleParamHash);

    private Animator animatorValue;
    private Animator animator
    {
        get
        {
            if (animatorValue == null)
            {
                animatorValue = GetComponent<Animator>();
            }

            return animatorValue;
        }
    }
    private int? visibleParamHashValue = null;

    private int visibleParamHash
    {
        get
        {
            if (!visibleParamHashValue.HasValue)
                visibleParamHashValue = Animator.StringToHash(visibleParam);

            return visibleParamHashValue.Value;
        }
    }

    public void Show(bool instant = false)
    {
        animator.speed = animSpeedFactor;
        animator.SetBool(visibleParamHash, true);

        if (instant)
            animator.Update(10);
    }

    public void Hide(bool instant = false)
    {
        animator.speed = animSpeedFactor;
        animator.SetBool(visibleParamHash, false);

        if (instant)
            animator.Update(10);
    }


    public void AnimEvent_HideFinished()
    {
        OnWillFinishHide?.Invoke(this);

        if (disableAfterFadeOut)
        {
            gameObject?.SetActive(false);
        }
    }

    public void AnimEvent_ShowFinished()
    {
        OnWillFinishStart?.Invoke(this);
    }

    private void OnEnable()
    {
        if ( hideOnEnable )
        {
            Hide(true);
        }
    }
}
