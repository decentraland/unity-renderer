using UnityEngine;

/// <summary>
/// Attaching this component to a canvas, will hide/show it after triggering the ToggleUIVisibility input action.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ShowHideUIByTrigger : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private ShowHideAnimator showHideAnimator;
    private Animator animator;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        showHideAnimator = GetComponent<ShowHideAnimator>();
        animator = GetComponent<Animator>();
        CommonScriptableObjects.allUIHidden.OnChange += AllUIVisible_OnChange;
        SetUIVisibility(!CommonScriptableObjects.allUIHidden.Get());
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.allUIHidden.OnChange -= AllUIVisible_OnChange;
    }

    private void AllUIVisible_OnChange(bool current, bool previous)
    {
        SetUIVisibility(!current);
    }

    private void SetUIVisibility(bool isVisible)
    {
        if (showHideAnimator != null)
            showHideAnimator.enabled = isVisible;

        if (animator != null)
            animator.enabled = isVisible;

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }
}
