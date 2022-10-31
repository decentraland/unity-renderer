using DG.Tweening;
using UnityEngine;

public class EventCardAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup cardShadow;
    [SerializeField] private RectTransform infoTransform;

    public void Focus()
    {
        cardShadow.DOFade(1, 0.1f)
                  .SetEase(Ease.Linear)
                  .SetLink(cardShadow.gameObject, LinkBehaviour.KillOnDestroy);
        infoTransform.DOAnchorPosY(5, 0.1f)
                     .SetEase(Ease.InOutQuad)
                     .SetLink(infoTransform.gameObject, LinkBehaviour.KillOnDestroy);
    }

    public void Idle()
    {
        cardShadow.DOFade(0, 0.1f)
                  .SetEase(Ease.Linear)
                  .SetLink(cardShadow.gameObject, LinkBehaviour.KillOnDestroy);
        infoTransform.DOAnchorPosY(0, 0.1f)
                     .SetEase(Ease.InOutQuad)
                     .SetLink(infoTransform.gameObject, LinkBehaviour.KillOnDestroy);
    }
}