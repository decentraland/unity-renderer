using DG.Tweening;
using UnityEngine;

public class EventCardAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup cardShadow;
    [SerializeField] private RectTransform infoTransform;

    public void Focus()
    {
        cardShadow.DOFade(1, 0.1f)
                  .SetEase(Ease.Linear);
        infoTransform.DOAnchorPosY(5, 0.1f)
                     .SetEase(Ease.InOutQuad);
    }

    public void Idle()
    {
        cardShadow.DOFade(0, 0.1f)
                  .SetEase(Ease.Linear);
        infoTransform.DOAnchorPosY(0, 0.1f)
                     .SetEase(Ease.InOutQuad);
    }
}