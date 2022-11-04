using DG.Tweening;
using UnityEngine;

public class EventCardAnimator : MonoBehaviour
{
    private const float DURATION = 0.167f;
    [SerializeField] private CanvasGroup cardShadow;
    [SerializeField] private RectTransform infoTransform;

    public void Focus()
    {
        cardShadow.DOFade(1, DURATION)
                  .SetEase(Ease.Linear)
                  .SetLink(cardShadow.gameObject, LinkBehaviour.KillOnDestroy);
        infoTransform.DOAnchorPosY(5, DURATION)
                     .SetEase(Ease.InOutQuad)
                     .SetLink(infoTransform.gameObject, LinkBehaviour.KillOnDestroy);
    }

    public void Idle()
    {
        cardShadow.DOFade(0, DURATION)
                  .SetEase(Ease.Linear)
                  .SetLink(cardShadow.gameObject, LinkBehaviour.KillOnDestroy);
        infoTransform.DOAnchorPosY(0, DURATION)
                     .SetEase(Ease.InOutQuad)
                     .SetLink(infoTransform.gameObject, LinkBehaviour.KillOnDestroy);
    }
}