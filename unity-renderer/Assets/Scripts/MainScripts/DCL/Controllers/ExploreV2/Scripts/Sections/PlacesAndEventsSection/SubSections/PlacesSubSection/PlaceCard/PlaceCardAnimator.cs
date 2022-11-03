using DG.Tweening;
using UnityEngine;

public class PlaceCardAnimator : PlaceCardAnimatorBase
{
    private const float DURATION = 0.167f;
    [SerializeField] private CanvasGroup focusInfo;
    [SerializeField] private CanvasGroup idleInfo;

    public override void Focus()
    {
        focusInfo.DOFade(1, DURATION)
                 .SetEase(Ease.InOutQuad)
                 .SetLink(focusInfo.gameObject, LinkBehaviour.KillOnDestroy);
        idleInfo.DOFade(0, DURATION)
                .SetEase(Ease.InOutQuad)
                .SetLink(idleInfo.gameObject, LinkBehaviour.KillOnDestroy);
    }

    public override void Idle()
    {
        focusInfo.DOFade(0, DURATION)
                 .SetEase(Ease.Linear)
                 .SetLink(focusInfo.gameObject, LinkBehaviour.KillOnDestroy);
        idleInfo.DOFade(1, DURATION)
                .SetEase(Ease.Linear)
                .SetLink(idleInfo.gameObject, LinkBehaviour.KillOnDestroy);
    }
}