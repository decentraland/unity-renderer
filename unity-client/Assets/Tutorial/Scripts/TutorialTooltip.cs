using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialTooltip : MonoBehaviour, IPointerClickHandler
{
    public float secondsOnScreen = 12f;

    [SerializeField] private AnimationClip showAnimationClip = null;
    [SerializeField] private AnimationClip hideAnimationClip = null;

    private Animation animator = null;
    private bool isShowing = false;

    public void Awake()
    {
        animator = GetComponent<Animation>();
        gameObject.SetActive(false);
        isShowing = false;
    }

    public void Show()
    {
        if (isShowing) return;

        isShowing = true;
        gameObject.SetActive(true);

        if (animator && showAnimationClip)
        {
            animator.clip = showAnimationClip;
            animator.Play();
        }
    }

    public void Hide()
    {
        if (!isShowing) return;

        isShowing = false;
        if (animator && hideAnimationClip)
        {
            animator.clip = hideAnimationClip;
            animator.Play();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Animation event
    public void OnHideFinished()
    {
        gameObject.SetActive(false);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        Hide();
    }
}
