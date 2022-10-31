using DG.Tweening;
using UnityEngine;

public class PlaceLongCardAnimator : PlaceCardAnimatorBase
{
    [SerializeField] private CanvasGroup cardShadow;
    [SerializeField] private RectTransform infoTransform;

    public override void Focus()
    {
        cardShadow.DOFade(1, 0.1f)
                  .SetEase(Ease.Linear);
        infoTransform.DOAnchorPosY(5, 0.1f)
                     .SetEase(Ease.InOutQuad);
    }

    public override void Idle()
    {
        cardShadow.DOFade(0, 0.1f)
                  .SetEase(Ease.Linear);
        infoTransform.DOAnchorPosY(0, 0.1f)
                     .SetEase(Ease.InOutQuad);
    }
}