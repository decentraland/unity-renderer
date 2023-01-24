using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))] [DisallowMultipleComponent]
public class ShowHideAnimator : MonoBehaviour
{
    private const float BASE_DURATION = 0.2f;

    public bool hideOnEnable = true;
    public float animSpeedFactor = 1.0f;
    public bool disableAfterFadeOut;

    public bool isVisible;

    [SerializeField] private CanvasGroup canvasGroup;

    private GraphicRaycaster raycaster;

    public event Action<ShowHideAnimator> OnWillFinishHide;
    public event Action<ShowHideAnimator> OnWillFinishStart;

    private void Awake()
    {
        RemoveOldAnimator();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        raycaster = GetComponent<GraphicRaycaster>();
    }

    private void OnEnable()
    {
        if (hideOnEnable)
            Hide(instant: true);
    }

    private void RemoveOldAnimator()
    {
        if (TryGetComponent(out Animator animator))
        {
            Debug.LogWarning($"Removing old Animator on UI {gameObject.name}. Consider to remove it on the prefab level!", gameObject);
            Destroy(animator);
        }
    }

    public void Show(bool instant = false)
    {
        SetVisibility(visible: true, OnShowCompleted, instant);

        void OnShowCompleted() =>
            OnWillFinishStart?.Invoke(this);
    }

    public void Hide(bool instant = false)
    {
        SetVisibility(visible: false, OnHideCompleted, instant);

        void OnHideCompleted()
        {
            OnWillFinishHide?.Invoke(this);

            if (disableAfterFadeOut && gameObject != null)
                gameObject.SetActive(false);
        }
    }

    private void SetVisibility(bool visible, TweenCallback onComplete, bool instant = false)
    {
        isVisible = visible;

        if (canvasGroup == null)
        {
            Debug.LogError($"Show Hide Animator in GameObject: {gameObject.name} has no canvasGroup assigned", gameObject);
            return;
        }

        if (raycaster != null)
            raycaster.enabled = isVisible;

        // When instant, we use duration 0 instead of just modifying the canvas group to mock the old animator behaviour which needs a frame.
        float duration = instant ? 0 : BASE_DURATION * animSpeedFactor;

        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;

        canvasGroup.DOKill();

        canvasGroup.DOFade(isVisible ? 1 : 0, duration)
                   .SetEase(Ease.InOutQuad)
                   .OnComplete(onComplete)
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDestroy)
                   .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDisable);
    }
}
