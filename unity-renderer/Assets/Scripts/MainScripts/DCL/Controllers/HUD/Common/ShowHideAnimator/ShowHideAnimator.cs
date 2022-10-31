using DG.Tweening;
using UnityEngine;

public class ShowHideAnimator : MonoBehaviour
{
    private const float BASE_DURATION = 0.024f;

    public event System.Action<ShowHideAnimator> OnWillFinishHide;
    public event System.Action<ShowHideAnimator> OnWillFinishStart;

    public bool hideOnEnable = true;
    public float animSpeedFactor = 1.0f;
    public bool disableAfterFadeOut;

    [SerializeField] private CanvasGroup canvasGroup;

    private int? visibleParamHashValue = null;

    public bool isVisible => canvasGroup == null || canvasGroup.alpha >= 0;

    private void Awake()
    {
        if (TryGetComponent(out Animator animator)) //Remove old behaviour
            Destroy(animator);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (hideOnEnable)
        {
            Hide(true);
        }
    }

    public void Show(bool instant = false)
    {
        canvasGroup.blocksRaycasts = true;

        if (instant)
        {
            canvasGroup.alpha = 1;
            OnWillFinishStart?.Invoke(this);
            return;
        }

        canvasGroup.DOFade(1, BASE_DURATION * animSpeedFactor)
                   .SetEase(Ease.InOutQuad)
                   .OnComplete(() => OnWillFinishStart?.Invoke(this))
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDestroy)
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDisable);
    }

    public void Hide(bool instant = false)
    {
        canvasGroup.blocksRaycasts = false;

        if (instant)
        {
            canvasGroup.alpha = 0;
            OnWillFinishHide?.Invoke(this);
            return;
        }

        canvasGroup.DOFade(0, BASE_DURATION * animSpeedFactor)
                   .SetEase(Ease.InOutQuad)
                   .OnComplete(() =>
                   {
                       OnWillFinishHide?.Invoke(this);

                       if (disableAfterFadeOut && gameObject != null)
                       {
                           gameObject.SetActive(false);
                       }
                   })
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDestroy)
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDisable);
    }
}