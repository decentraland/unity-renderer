using DG.Tweening;
using UnityEngine;

public class PlaceCardAnimator : PlaceCardAnimatorBase
{
    [SerializeField] private CanvasGroup focusInfo;
    [SerializeField] private CanvasGroup idleInfo;

    public override void Focus()
    {
        focusInfo.DOFade(1, 0.1f)
                 .SetEase(Ease.InOutQuad);
        idleInfo.DOFade(0, 0.1f)
                .SetEase(Ease.InOutQuad);
    }

    public override void Idle()
    {
        focusInfo.DOFade(0, 0.1f)
                 .SetEase(Ease.Linear);
        idleInfo.DOFade(1, 0.1f)
                .SetEase(Ease.Linear);
    }
}