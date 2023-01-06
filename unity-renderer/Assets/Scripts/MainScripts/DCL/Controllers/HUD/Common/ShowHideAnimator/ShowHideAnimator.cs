using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup)), DisallowMultipleComponent]
public class ShowHideAnimator : MonoBehaviour
{
    private const float BASE_DURATION = 0.2f;

    public event System.Action<ShowHideAnimator> OnWillFinishHide;
    public event System.Action<ShowHideAnimator> OnWillFinishStart;

    public bool hideOnEnable = true;
    public float animSpeedFactor = 1.0f;
    public bool disableAfterFadeOut;

    [SerializeField] private CanvasGroup canvasGroup;

    public bool isVisible => canvasGroup == null || canvasGroup.blocksRaycasts;

    private void Awake()
    {
        if (TryGetComponent(out Animator animator)) //Remove old behaviour
            Destroy(animator);

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (hideOnEnable) { Hide(true); }
    }

    public void Show(bool instant = false)
    {
        if (canvasGroup == null)
            Debug.LogError($"Show Hide Animator in GameObject: {gameObject.name} has no canvasgroup assigned");
        canvasGroup.blocksRaycasts = true;

        //When instant, we use duration 0 instead of just modifying the canvas group to mock the old animator behaviour which needs a frame.
        var duration = instant ? 0 : BASE_DURATION * animSpeedFactor;
        canvasGroup.DOKill();

        canvasGroup.DOFade(1, duration)
                   .SetEase(Ease.InOutQuad)
                   .OnComplete(() => OnWillFinishStart?.Invoke(this))
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDestroy)
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDisable);
    }

    public void Hide(bool instant = false)
    {
        if (canvasGroup == null)
            Debug.LogError($"Show Hide Animator in GameObject: {gameObject.name} has no canvasgroup assigned");
        canvasGroup.blocksRaycasts = false;

        //When instant, we use duration 0 instead of just modifying the canvas group to mock the old animator behaviour which needs a frame.
        var duration = instant ? 0 : BASE_DURATION * animSpeedFactor;
        canvasGroup.DOKill();

        canvasGroup.DOFade(0, duration)
                   .SetEase(Ease.InOutQuad)
                   .OnComplete(() =>
                    {
                        OnWillFinishHide?.Invoke(this);

                        if (disableAfterFadeOut && gameObject != null) { gameObject.SetActive(false); }
                    })
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDestroy)
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDisable);
    }
}
