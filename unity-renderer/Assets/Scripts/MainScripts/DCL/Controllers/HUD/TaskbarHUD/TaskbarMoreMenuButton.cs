using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarMoreMenuButton : MonoBehaviour
{
    internal enum AnimationStatus
    {
        Hide = 0,
        Visible = 1,
        In = 2,
        Out = 3,
    }

    [SerializeField] internal Button mainButton;
    [SerializeField] internal TMP_Text buttonText;
    [SerializeField] internal Animator buttonAnimator;
    [SerializeField] internal Color notInteractableColor;

    internal AnimationStatus lastPlayedAnimation { get; private set; } = AnimationStatus.Hide;

    private Color originalTextColor;

    private void Start()
    {
        if (buttonText != null)
            originalTextColor = buttonText.color;
    }

    private void OnDisable() { lastPlayedAnimation = AnimationStatus.Hide; }

    internal void PlayAnimation(AnimationStatus newStatus)
    {
        switch (newStatus)
        {
            case AnimationStatus.Hide:
                buttonAnimator.SetTrigger("Hide");
                break;
            case AnimationStatus.Visible:
                buttonAnimator.SetTrigger("Visible");
                break;
            case AnimationStatus.In:
                buttonAnimator.SetTrigger("In");
                break;
            case AnimationStatus.Out:
                buttonAnimator.SetTrigger("Out");
                break;
        }

        lastPlayedAnimation = newStatus;
    }

    internal float GetAnimationLenght()
    {
        if (buttonAnimator.GetCurrentAnimatorClipInfoCount(0) == 0 ||
            buttonAnimator.GetCurrentAnimatorClipInfo(0).Length == 0)
            return 0f;

        return buttonAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    }

    internal void SetInteractable(bool isInteractable)
    {
        mainButton.interactable = isInteractable;

        if (buttonText != null)
            buttonText.color = isInteractable ? originalTextColor : notInteractableColor;
    }
}