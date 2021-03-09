using UnityEngine;

internal class AnimationHandler
{
    private static readonly int paramLoadingComplete = Animator.StringToHash("LoadingComplete");
    private static readonly int paramInitialize = Animator.StringToHash("Initialize");

    private readonly Animator animator;

    public AnimationHandler(Animator animator)
    {
        this.animator = animator;
    }

    public void SetLoaded()
    {
        if (animator.gameObject.activeInHierarchy)
            animator.SetTrigger(paramLoadingComplete);
    }
}
